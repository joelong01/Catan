using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    public class UpdateBuildingLog : LogHeader, ILogController
    {
        public int BuildingIndex { get; set; } = -1;

        public BuildingState NewBuildingState { get; set; } = BuildingState.None;

        public BuildingState OldBuildingState { get; set; } = BuildingState.None;

        public UpdateBuildingLog() : base()
        {
        }

        public static async Task UpdateBuildingState(IGameController gameController, BuildingCtrl building, BuildingState newState)
        {
            Entitlement entitlement = Entitlement.Undefined;
            if (newState == BuildingState.Settlement) entitlement = Entitlement.Settlement;
            if (newState == BuildingState.City) entitlement = Entitlement.City;
            if (newState == BuildingState.Knight) entitlement = Entitlement.BuyOrUpgradeKnight;

            if (newState == BuildingState.Settlement && building.BuildingState == BuildingState.City) entitlement = Entitlement.DestroyCity;

            if (gameController.CurrentPlayer.GameData.Resources.HasEntitlement(entitlement) == false)
            {
                return; // user double clicked
            }

            UpdateBuildingLog logHeader = new UpdateBuildingLog()
            {
                OldBuildingState = building.BuildingState,
                NewBuildingState = newState,
                BuildingIndex = building.Index,
                Action=CatanAction.UpdateBuildingState
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            return gameController.UpdateBuilding(this, ActionType.Normal);
        }
        public Task Replay (IGameController gameController)
        {
            return Do(gameController);
        }
        public Task Redo(IGameController gameController)
        {
            gameController.CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.DestroyCity);
            return Do(gameController);
        }

        public Task Undo(IGameController gameController)
        {
            return gameController.UndoUpdateBuilding(this);
        }
    }
}