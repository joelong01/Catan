﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Catan10
{
    public interface INewLog
    {
        void PushAction(ILogHeader entry);
        Task Undo(); // this is a Pop from Action push to Undo
        void Redo();  // pop from Undo push to Action
    }

    public class NewLog : INewLog
    {
        private readonly List<ILogHeader> ActionStack = new List<ILogHeader>();
        private readonly List<ILogHeader> UndoStack = new List<ILogHeader>();
        public MainPage Page { get; internal set; }


        public NewLog(MainPage p)
        {
            Page = p;
        }

        public void PushAction(ILogHeader entry)
        {
            ActionStack.Add(entry);
        }

        public void Redo()
        {
            var logEntry = UndoStack.Last();
            UndoStack.RemoveAt(UndoStack.Count - 1);
            //
            //  now get the controller for this particular Action
            switch (logEntry.Action)
            {
                case CatanAction.Rolled:
                    {
                        RolledModel model = logEntry as RolledModel;
                        RolledController controller = new RolledController(Page, model);
                        controller.Redo(model);
                    }
                    break;
                case CatanAction.ChangedState:
                    break;
                case CatanAction.ChangedPlayer:
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
                case CatanAction.RandomizeTiles:
                    break;
                case CatanAction.AssignHarbors:
                    break;
                case CatanAction.SelectGame:
                    break;
                case CatanAction.AssignRandomTiles:
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
            var logEntry = UndoStack.Last();
            UndoStack.RemoveAt(UndoStack.Count - 1);
            //
            //  now get the controller for this particular Action
            switch (logEntry.Action)
            {
                case CatanAction.Rolled:
                    {
                        RolledModel model = logEntry as RolledModel;
                        RolledController controller = new RolledController(Page, model);
                        await controller.Undo(model);
                    }
                    break;
                case CatanAction.ChangedState:
                    break;
                case CatanAction.ChangedPlayer:
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
                case CatanAction.RandomizeTiles:
                    break;
                case CatanAction.AssignHarbors:
                    break;
                case CatanAction.SelectGame:
                    break;
                case CatanAction.AssignRandomTiles:
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
    }
}