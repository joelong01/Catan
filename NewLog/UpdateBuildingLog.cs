using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catan.Proxy;

namespace Catan10
{
    public class UpdateBuildingLog : LogHeader, ILogController
    {

        public BuildingState OldBuildingState { get; set; } = BuildingState.None;
        public BuildingState NewBuildingState { get; set; } = BuildingState.None;
        public int BuildingIndex { get; set; } = -1;


        public UpdateBuildingLog() : base() { }

        public static async Task UpdateBuildingState(IGameController gameController, BuildingCtrl building, BuildingState newState)
        {
            UpdateBuildingLog logHeader = new UpdateBuildingLog()
            {
                OldBuildingState = building.BuildingState,
                NewBuildingState = newState,
                BuildingIndex = building.Index
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }
        public Task Do(IGameController gameController)
        {
            return gameController.UpdateBuilding(this);
        }

        public Task Redo(IGameController gameController)
        {
            return gameController.UpdateBuilding(this);
        }

        public Task Undo(IGameController gameController)
        {
            return gameController.UndoUpdateBuilding(this);
        }
    }
}
