using Catan.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catan10
{
    public interface INewLog
    {
        void PushAction(ILogHeader entry);
        Task Undo(); // this is a Pop from Action push to Undo
        Task Redo();  // pop from Undo push to Action
    }

    public class NewLog : INewLog
    {
        private readonly List<ILogHeader> ActionStack = new List<ILogHeader>();
        private readonly List<ILogHeader> UndoStack = new List<ILogHeader>();
        public MainPage Page { get; internal set; }
        public GameState GameState
        {
            get
            {

                if (ActionStack.Count == 0)
                {
                    return GameState.WaitingForNewGame;
                }

                return ActionStack.Last().NewState;

            }

        }

        public NewLog(MainPage p)
        {
            Page = p;
        }

        public void PushAction(ILogHeader entry)
        {
            ActionStack.Add(entry);
        }

        public async Task Redo()
        {
            var logEntry = UndoStack.Last();
            UndoStack.RemoveAt(UndoStack.Count - 1);
            //
            //  now get the controller for this particular Action
            switch (logEntry.Action)
            {
                case CatanAction.Rolled:
                    RolledController.Redo(Page, (RolledModel)logEntry);
                    break;
                case CatanAction.ChangedState:
                    break;
                case CatanAction.ChangedPlayer:
                    await ChangedPlayerController.Redo(Page, logEntry as ChangedPlayerModel);
                    break;
                case CatanAction.RandomizeBoard:
                    break;
                case CatanAction.Dealt:
                    break;
                case CatanAction.CardsLost:
                    break;
                case CatanAction.CardsLostToSeven:
                    break;
                case CatanAction.MissedOpportunity:
                    break;
                case CatanAction.DoneSupplemental:
                    break;
                case CatanAction.DoneResourceAllocation:
                    break;
                case CatanAction.PlayedKnight:
                    break;
                case CatanAction.RolledSeven:
                    break;
                case CatanAction.AssignedBaron:
                    break;
                case CatanAction.UpdatedRoadState:
                    break;
                case CatanAction.UpdateBuildingState:
                    break;
                case CatanAction.AssignedPirateShip:
                    break;
                case CatanAction.AddPlayer:
                    break;
                case CatanAction.SelectGame:
                    break;                
                case CatanAction.InitialAssignBaron:
                    break;
                case CatanAction.None:
                    break;
                case CatanAction.SetFirstPlayer:
                    break;
                case CatanAction.RoadTrackingChanged:
                    break;
                case CatanAction.AddResourceCount:
                    break;
                case CatanAction.ChangedPlayerProperty:
                    break;
                case CatanAction.SetRandomTileToGold:
                    break;
                case CatanAction.ChangePlayerAndSetState:
                    break;
                default:
                    break;
            }
        }

        public async Task Undo()
        {
            var logEntry = ActionStack.Last();
            if (logEntry.Action == CatanAction.Started)
            {
                // you can't undo at this point -- just start a new game!
                return;
            }
            ActionStack.RemoveAt(ActionStack.Count - 1);
            UndoStack.Push(logEntry);
            //
            //  now get the controller for this particular Action
            switch (logEntry.Action)
            {
                case CatanAction.Rolled:
                    RolledController.Undo(Page, logEntry as RolledModel);
                    break;
                case CatanAction.ChangedState:
                    break;
                case CatanAction.ChangedPlayer:
                case CatanAction.ChangePlayerAndSetState:
                    await ChangedPlayerController.Undo(Page, logEntry as ChangedPlayerModel);
                    break;
                case CatanAction.Dealt:
                    break;
                case CatanAction.CardsLost:
                    break;
                case CatanAction.CardsLostToSeven:
                    break;
                case CatanAction.MissedOpportunity:
                    break;
                case CatanAction.DoneSupplemental:
                    break;
                case CatanAction.DoneResourceAllocation:
                    break;
                case CatanAction.PlayedKnight:
                    break;
                case CatanAction.RolledSeven:
                    break;
                case CatanAction.AssignedBaron:
                    break;
                case CatanAction.UpdatedRoadState:
                    break;
                case CatanAction.UpdateBuildingState:
                    break;
                case CatanAction.AssignedPirateShip:
                    break;
                case CatanAction.AddPlayer:
                    break;
                case CatanAction.SelectGame:
                    break;
                case CatanAction.InitialAssignBaron:
                    break;
                case CatanAction.None:
                    break;
                case CatanAction.SetFirstPlayer:
                    break;
                case CatanAction.RoadTrackingChanged:
                    break;
                case CatanAction.AddResourceCount:
                    break;
                case CatanAction.ChangedPlayerProperty:
                    break;
                case CatanAction.SetRandomTileToGold:
                    break;
                default:
                    break;
            }
        }
    }
}