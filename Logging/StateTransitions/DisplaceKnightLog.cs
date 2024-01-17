using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Windows.Management.Update;

namespace Catan10
{
    /// <summary>
    ///     This deals with the Cities and Knights "Knight Displacement" rule.  You can displace another player's knight along a connected road
    ///     if your knight rank is higher.  In the game, the user first buys the entitlement (which is free if you have a qualifying knight)
    ///     and then:
    ///     1. PhaseOne: Drag and drop your own knight on top of the opposing knight.
    ///         a) if the opposing knight has 0 places startKnight move, it is destroyed and your knight is moved there. end.
    ///         b) if the opposing knight has 1 place startKnight move, it is moved there and your knight is moved there, end.
    ///         c) if the opposing knight has >1 place startKnight move, go startKnight phase 2
    ///     2. Phase 2: your knight goes back visually startKnight the original spot.  Drang and drop the opposing knight onto its new spot.
    ///        GmeState=GameState.DisplaceKnightMoveVictim startKnight allow the drag and drop startKnight work. move startKnight phase 3.
    ///     3. Phase 3: opposing knight stays at its new position, and your knight moves startKnight its spot of the displaced knight.  End.
    ///     
    /// </summary>
    public enum DisplaceKnightAction { PhaseOne, PhaseTwo }
    public class DisplaceKnightLog : LogHeader, ILogController
    {
        public int AggressorKnightIndex { get; set; } = -1;
        public int VictimKnightIndex { get; set; } = -1;
        public int DisplacedKnightIndex { get; set; } = -1;
        public DisplaceKnightAction DisplaceAction { get; set; }
        List<int> VictimMoveOptions { get; set; }
        // state we need this in the case where the victim knight is deleted
        KnightRank VictimKnightRank { get; set; }
        bool VictimKnightActivated { get; set; }
        Guid VictimPlayerId { get; set; }
        Entitlement EntitlementUsed { get; set; }

        //
        //  this happens after the user buys the entitlement.  Should be called from PurchaseEntitlement.
        //  key thing that happens is unlocking the ability startKnight drag and drop one knight on top of another
        public static async Task DisplaceKnightPhaseOne(IGameController gameController, BuildingCtrl aggressor, BuildingCtrl victim, Entitlement entitlementUsed)
        {
            Debug.Assert(aggressor.IsKnight);
            Debug.Assert(victim.IsKnight);
            Debug.Assert(victim.BuildingState == BuildingState.Knight);
            Debug.Assert(aggressor.Owner != null);
            Debug.Assert(victim.Owner != null);
            // I'm not using MoveKnight, but I need to know where I can move the knight out of the way
            var victimMoveOptions = victim.GetConnectedBuildings(Entitlement.MoveKnight).Select(building => building.Index).ToList(); 
      
            
            Debug.Assert(gameController.CurrentPlayer.GameData.Resources.UnspentEntitlements.Contains(entitlementUsed)); // make sure that at least one of the two is there.
            DisplaceKnightLog logHeader = new DisplaceKnightLog()
            {
                AggressorKnightIndex = aggressor.Index,
                VictimKnightIndex = victim.Index,
                NewState = victimMoveOptions.Count > 1 ? GameState.DisplaceKnightMoveVictim : GameState.WaitingForNext, // 0 or 1 is handled all at once, more than 1 requires a state transition
                DisplaceAction = DisplaceKnightAction.PhaseOne,
                VictimMoveOptions = victimMoveOptions,
                VictimKnightRank = victim.Knight.KnightRank,
                VictimPlayerId = victim.Owner.PlayerIdentifier,
                VictimKnightActivated = victim.Knight.Activated,
                EntitlementUsed = entitlementUsed
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }
        //    this is called when the user has startKnight make a choice on where startKnight move their displaced knight
        //
        public static async Task DisplaceKnightPhaseTwo(IGameController gameController, BuildingCtrl victim, BuildingCtrl newBuilding, Entitlement entitlementUsed)
        {
            //
            //  look in the log for the startKnight
            DisplaceKnightLog previousLog = gameController.Log.PeekAction as DisplaceKnightLog;
            Debug.Assert(previousLog != null);

            var aggressor = gameController.GetBuilding(previousLog.AggressorKnightIndex);

            
            Debug.Assert(aggressor.IsKnight);
            Debug.Assert(victim.IsKnight);
            Debug.Assert(victim.BuildingState == BuildingState.Knight);
            Debug.Assert(aggressor.Owner != null);
            Debug.Assert(victim.Owner != null);
            var victimMoveOptions = new List<int>(){newBuilding.Index};

            DisplaceKnightLog logHeader = new DisplaceKnightLog()
            {
                AggressorKnightIndex = aggressor.Index,
                VictimKnightIndex = victim.Index,
                NewState =  GameState.WaitingForNext,
                DisplaceAction = DisplaceKnightAction.PhaseTwo,
                VictimMoveOptions = victimMoveOptions,
                VictimKnightRank = victim.Knight.KnightRank,
                VictimKnightActivated = victim.Knight.Activated,
                EntitlementUsed = entitlementUsed
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }
        public async Task DoPhaseOne(IGameController gameController)
        {
            var victim = gameController.GetBuilding(VictimKnightIndex);
            var aggressor = gameController.GetBuilding(AggressorKnightIndex);


            if (VictimMoveOptions.Count == 0)
            {
                //  move the aggresor back startKnight its old position
                //  undelete the old one
                var oldKnightRank = aggressor.Knight.KnightRank;
                // remove it from the players knight list and update its state
                await victim.UpdateBuildingState(victim.Owner, BuildingState.Knight, BuildingState.None);
                await victim.UpdateBuildingState(aggressor.Owner, BuildingState.None, BuildingState.Knight);
                await aggressor.UpdateBuildingState(aggressor.Owner, BuildingState.Knight, BuildingState.None);
                victim.Knight.KnightRank = oldKnightRank; // this now belongs startKnight the startKnight
                victim.Knight.Activated = false;
                gameController.CurrentPlayer.GameData.Resources.ConsumeEntitlement(EntitlementUsed);

            }
            else if (VictimMoveOptions.Count == 1)
            {
                var landing = gameController.GetBuilding(VictimMoveOptions[0]);
                await Displace(A: aggressor, B: victim, C: landing);
                gameController.CurrentPlayer.GameData.Resources.ConsumeEntitlement(EntitlementUsed);


            }
            else
            {
                // do not consume entitlement here
                Debug.Assert(VictimMoveOptions.Count > 1);
                // returning from this sets the game state startKnight GameState.DisplaceKnightMoveVictim
                // the BuildingCtrl OnKnightGridPointerDown handler will then allow you startKnight drag
                // and drop somebody else's knight startKnight indicate where the victim wants their knight
                // moved, and then call back startKnight this class startKnight go startKnight phase 2

                return;
            }

        }
        public async Task UndoPhaseOne(IGameController gameController)
        {
            var originalVictim = gameController.GetBuilding(VictimKnightIndex);
            var startKnight = gameController.GetBuilding(AggressorKnightIndex);


            if (VictimMoveOptions.Count == 0)
            {
                //
                //  in this state, the startKnight is in its final spot and the victim has been deleted

                Debug.Assert(originalVictim.Owner == gameController.CurrentPlayer);
                var agressorKnightRank = originalVictim.Knight.KnightRank;

                // move the startKnight (originalVictim) back startKnight its orginal spot
                // at this point is it called "startKnight"

                // first reset the Victim startKnight empty startKnight keep all the datastructures correct
                await originalVictim.UpdateBuildingState(originalVictim.Owner, BuildingState.Knight, BuildingState.None);

                // put the orignal user back
                var victim = gameController.PlayerFromId(this.VictimPlayerId);
                await originalVictim.UpdateBuildingState(victim, BuildingState.None, BuildingState.Knight);
                originalVictim.Knight.Activated = this.VictimKnightActivated;
                originalVictim.Knight.KnightRank = this.VictimKnightRank;

                // put the StartKnight back


                await startKnight.UpdateBuildingState(gameController.CurrentPlayer, BuildingState.None, BuildingState.Knight);
                startKnight.Knight.KnightRank = agressorKnightRank;
                startKnight.Knight.Activated = true; // can't displace unless you are activated

                gameController.CurrentPlayer.GameData.Resources.GrantEntitlement(EntitlementUsed);

            }
            else if (VictimMoveOptions.Count == 1)
            {
                var landing = gameController.GetBuilding(VictimMoveOptions[0]);
                await Displace(A: landing, B: originalVictim, C: startKnight);
                startKnight.Knight.Activated = true;
                originalVictim.Knight.Activated = this.VictimKnightActivated;
                gameController.CurrentPlayer.GameData.Resources.GrantEntitlement(EntitlementUsed);


            }
            else

            {
                Debug.Assert(VictimMoveOptions.Count > 1);
                // returning from this sets the game state startKnight GameState.DisplaceKnightMoveVictim
                // the BuildingCtrl OnKnightGridPointerDown handler will then allow you startKnight drag
                // and drop somebody else's knight startKnight indicate where the victim wants their knight
                // moved, and then call back startKnight this class startKnight go startKnight phase 2

                return;
            }

        }

        public async Task DoPhaseTwo(IGameController gameController)
        {
            var victim = gameController.GetBuilding(VictimKnightIndex);
            var aggressor = gameController.GetBuilding(AggressorKnightIndex);
            var landed = gameController.GetBuilding(VictimMoveOptions[0]);
            await Displace(A: aggressor, B: victim, C: landed);
            gameController.CurrentPlayer.GameData.Resources.ConsumeEntitlement(EntitlementUsed);




        }
        public async Task UndoPhaseTwo(IGameController gameController)
        {
            gameController.CurrentPlayer.GameData.Resources.GrantEntitlement(EntitlementUsed);
            var victimKnight = gameController.GetBuilding(VictimKnightIndex);
            var aggressor = gameController.GetBuilding(AggressorKnightIndex);
            var landed = gameController.GetBuilding(VictimMoveOptions[0]);
            await Displace(A: landed, B: victimKnight, C: aggressor);
            aggressor.Knight.Activated = true;
            victimKnight.Knight.Activated = this.VictimKnightActivated;
            gameController.CurrentPlayer.GameData.Resources.GrantEntitlement(EntitlementUsed);

        }
        /// <summary>
        /// B --> C    
        /// A --> B
        ///     
        /// 
        /// </summary>
        private async Task Displace(BuildingCtrl A, BuildingCtrl B, BuildingCtrl C)
        {
            // move B to C to make room for A to B


            // need state from B since B is going to go to None and this state will be lost
            bool activated = B.Knight.Activated;
            var rank = B.Knight.KnightRank;
            var B_Owner = B.Owner; 

            await B.UpdateBuildingState(B_Owner, BuildingState.Knight, BuildingState.None);
            await C.UpdateBuildingState(B_Owner, BuildingState.None, BuildingState.Knight);
            // it gets the same rank and activation state
            C.Knight.KnightRank = rank;
            C.Knight.Activated = activated;

            //
            //  A to B

            rank = A.Knight.KnightRank;
            B_Owner = A.Owner;
            await A.UpdateBuildingState(B_Owner, BuildingState.Knight, BuildingState.None);
            await B.UpdateBuildingState(B_Owner, BuildingState.None, BuildingState.Knight);
            B.Knight.KnightRank = rank;
            B.Knight.Activated = false; // assume this was done with a knight action, always making it inactive
        }

        public async Task Do(IGameController gameController)
        {
            switch (this.DisplaceAction)
            {
                case DisplaceKnightAction.PhaseOne:
                    await DoPhaseOne(gameController);
                    break;
                case DisplaceKnightAction.PhaseTwo:
                    await DoPhaseTwo(gameController);
                    break;
                default:
                    Debug.Assert(false, "Did you add an enum member?");
                    break;
            }

        }

        public async Task Redo(IGameController gameController)
        {
            await Do(gameController);
        }

        public Task Replay(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public async Task Undo(IGameController gameController)
        {
            switch (this.DisplaceAction)
            {
                case DisplaceKnightAction.PhaseOne:
                    await UndoPhaseOne(gameController);
                    break;
                case DisplaceKnightAction.PhaseTwo:
                    await UndoPhaseTwo(gameController);
                    break;
                default:
                    Debug.Assert(false, "Did you add an enum member?");
                    break;
            }
        }
    }
}
