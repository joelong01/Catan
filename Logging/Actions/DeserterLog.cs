using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.WebUI;

namespace Catan10
{
    /// <summary>
    ///     1/5/2024
    ///         This is the first time I've put two state transitions into the same file, but this handles the whole of the Deserter dev card.
    ///         First the user buys the entitlement, which transitions to the PickDeserter state.  this allows the Knight's LeftClick handler
    ///         to execute, which goes to Callback KnightLeftPointerPressed.  there it checks the state and first calls PickDeserterLog.
    ///         this transitions the state for the user to pick where to place the knight. Once the user clicks on a placement, then 
    ///         KnightLeftPointerPressed will call the PlaceDeserterLog().  We get two log entries, but that makes sense in case Dodgy
    ///         changes his mind where he wants to place the knight.
    ///         
    ///         the placed Knight automatically gets the same Rank as the caller, but is not Activated.
    ///         
    ///         Undo should put the system back into the exact state prior to Destroying/Placing the knight (including Activation State)
    /// </summary>
    /// 

    //
    // this enum disabiguates the CurrentGameState for both Do and Undo, which can be confusing when debugging because of when the 
    // log record is pushed to the stack.  this just makes it explicit of the action.

    public enum DeserterAction { Undefined, PickVictim, PlaceKnight }

    internal class DeserterLog : LogHeader, ILogController
    {
        public UpdateBuildingLog UpdateBuildingLog { get; set; }
        public KnightRank DestroyedKnightRank { get; set; }
        public bool Activated { get; set; }
        public DeserterAction DeserterAction { get; set; }
        public static async Task PickDeserterLog(IGameController controller, BuildingCtrl building)
        {
            Debug.Assert(building.IsKnight);
            var log = new DeserterLog()
            {

                NewState = GameState.PlaceDeserterKnight,
                DestroyedKnightRank = building.Knight.KnightRank,
                Activated = building.Knight.Activated,
                DeserterAction = DeserterAction.PickVictim,
                UpdateBuildingLog = new UpdateBuildingLog()
                {
                    OldBuildingState = BuildingState.Knight,
                    NewBuildingState = BuildingState.None,
                    BuildingIndex = building.Index,
                    OriginalOwnerId = building.Owner.PlayerIdentifier
                },
            };

            await controller.PostMessage(log, ActionType.Normal);
        }

        public static async Task PlaceDeserterLog(IGameController gameController, BuildingCtrl building)
        {
            DeserterLog prevLogEntry = gameController.Log.PeekAction as DeserterLog;
            Debug.Assert(prevLogEntry != null);
            var log = new DeserterLog()
            {

                NewState = GameState.WaitingForNext,
                DestroyedKnightRank =prevLogEntry.DestroyedKnightRank,
                Activated = prevLogEntry.Activated,
                DeserterAction = DeserterAction.PlaceKnight,
                UpdateBuildingLog = new UpdateBuildingLog()
                {
                    OldBuildingState = BuildingState.None,
                    NewBuildingState = BuildingState.Knight,
                    BuildingIndex = building.Index,
                    OriginalOwnerId =  prevLogEntry.UpdateBuildingLog.OriginalOwnerId,
                    NewOwnerId = gameController.CurrentPlayer.PlayerIdentifier
                },
            };

            await gameController.PostMessage(log, ActionType.Normal);
        }
        public async Task Do(IGameController gameController)
        {
            var building = gameController.GetBuilding(UpdateBuildingLog.BuildingIndex);

            if (DeserterAction == DeserterAction.PickVictim)
            {
                Debug.Assert(gameController.CurrentGameState == GameState.PlaceDeserterKnight);
                //
                //  remove the other players knight
                await building.UpdateBuildingState(gameController.PlayerFromId(this.UpdateBuildingLog.OriginalOwnerId), BuildingState.Knight, BuildingState.None);
                return;
            }

            if (DeserterAction == DeserterAction.PlaceKnight)
            {
              //  Debug.Assert(gameController.CurrentGameState == GameState.DoneWithDeserter);
                await building.UpdateBuildingState(gameController.CurrentPlayer, BuildingState.None, BuildingState.Knight);
                building.Knight.KnightRank = this.DestroyedKnightRank;
                building.Knight.Activated = false;
                gameController.CurrentPlayer.GameData.Resources.ConsumeEntitlement(Entitlement.Deserter);
                // Set the state to WaitingForNext, but when the user clicks undo continue past the set state call
                // to Undo the PlaceDeserterKnight
             //   await SetStateLog.SetState(gameController, GameState.WaitingForNext, true);
                return;
            }
        }

        public Task Redo(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public Task Replay(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public async Task Undo(IGameController gameController)
        {
            //
            //  the LogEntry should work correctly for both PickDeserter and PlaceDeserterKnight

            var building = gameController.GetBuilding(UpdateBuildingLog.BuildingIndex);

            if (DeserterAction == DeserterAction.PlaceKnight) 
            {
                Debug.Assert(this.OldState == GameState.PlaceDeserterKnight);
                Debug.Assert(building != null);
                Debug.Assert(building.Owner == gameController.CurrentPlayer);
                Debug.Assert(building.BuildingState == BuildingState.Knight);

                //
                //  get rid of the knight
                await building.UpdateBuildingState(gameController.CurrentPlayer, BuildingState.Knight, BuildingState.None);

                //   make sure they have the entitlement they got with the Deserter card
                gameController.CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.Deserter);


            }
            else if (DeserterAction == DeserterAction.PickVictim)
            {
                Debug.Assert(this.OldState == GameState.PickDeserter);
                //
                //   put the old knight back
                Debug.Assert(building != null);
                Debug.Assert(building.Owner == null);
                Debug.Assert(building.BuildingState == BuildingState.None);


                var victim = gameController.PlayerFromId(this.UpdateBuildingLog.OriginalOwnerId);
                await building.UpdateBuildingState(victim, BuildingState.None, BuildingState.Knight);


                building.Knight.KnightRank = this.DestroyedKnightRank;
                building.Knight.Activated = this.Activated;
            }
            else
            {
                Debug.Assert(false, $"Bad state! {this.OldState}");
            }


        }
    }
}
