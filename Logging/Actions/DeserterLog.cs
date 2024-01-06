using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    internal class DeserterLog : LogHeader, ILogController
    {
        public UpdateBuildingLog UpdateBuildingLog { get; set; }
        public KnightRank DestroyedKnightRank { get; set; }
        public bool Activated { get; set; }

        public static async Task PickDeserterLog(IGameController controller, BuildingCtrl building)
        {
            Debug.Assert(building.IsKnight);
            var log = new DeserterLog()
            {

                NewState = GameState.PlaceDeserterKnight,
                DestroyedKnightRank = building.Knight.KnightRank,
                Activated = building.Knight.Activated,
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

                NewState = GameState.DoneWithDeserter,
                DestroyedKnightRank =prevLogEntry.DestroyedKnightRank,
                Activated = prevLogEntry.Activated,
                UpdateBuildingLog = new UpdateBuildingLog()
                {
                    OldBuildingState = BuildingState.None,
                    NewBuildingState = BuildingState.Knight,
                    BuildingIndex = building.Index,
                    OriginalOwnerId =  prevLogEntry.UpdateBuildingLog.OriginalOwnerId
                },
            };

            await gameController.PostMessage(log, ActionType.Normal);
        }
        public async Task Do(IGameController gameController)
        {
            //
            //  this works for both log entries -- the first DeserterLog will delete the knight
            //  the second will build a new knight
            await gameController.UpdateBuilding(this.UpdateBuildingLog, ActionType.Normal);

            if (gameController.CurrentGameState == GameState.DoneWithDeserter)
            {
                // the knight is already in the right spot - just update its rank and set the activated flag correctly
                var building = gameController.GetBuilding(UpdateBuildingLog.BuildingIndex);
                Debug.Assert(building != null);
                Debug.Assert(building.Owner != null);
        
                building.Knight.KnightRank = this.DestroyedKnightRank;
                building.Knight.Activated = false;
                gameController.CurrentPlayer.GameData.Resources.ConsumeEntitlement(Entitlement.Deserter);
                // Set the state to WaitingForNext, but when the user clicks undo continue past the set state call
                // to Undo the PlaceDeserterKnight
                await SetStateLog.SetState(gameController, GameState.WaitingForNext, true); 
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
            await gameController.UndoUpdateBuilding(this.UpdateBuildingLog);

            if (this.NewState == GameState.PlaceDeserterKnight)
            {
                //
                //   make sure they have the entitlement they got with the Deserter card
                gameController.CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.Deserter);

            }
            else
            {
                // put the destroyed knight back into its orginal state -- the UndoUpdateBuilding above will set teh bulding state to Knight, but 
                // we need to fix Rank and Actived
                Debug.Assert(this.NewState == GameState.PickDeserter);
                var building = gameController.GetBuilding(UpdateBuildingLog.BuildingIndex);
                Debug.Assert(building != null);
                Debug.Assert(building.Owner != null);
                Debug.Assert(building.Owner != gameController.CurrentPlayer);

                building.Knight.KnightRank = this.DestroyedKnightRank;
                building.Knight.Activated = this.Activated;
            }


        }
    }
}
