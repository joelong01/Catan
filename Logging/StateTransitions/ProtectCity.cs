using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    public class ProtectCityLog : LogHeader, ILogController
    {
        public int BuildingIndex { get; set; } = -1;

       
        public ProtectCityLog() : base()
        {
        }

        public static async Task ProtectCity(IGameController gameController, BuildingCtrl building)
        {


            Contract.Assert(gameController.CurrentPlayer.GameData.Resources.HasEntitlement(Entitlement.Wall));

            ProtectCityLog logHeader = new ProtectCityLog()
            {
              
                BuildingIndex = building.Index,
                Action=CatanAction.UpdateBuildingState
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            return gameController.ProtectCity(this, ActionType.Normal);
        }
        public Task Replay (IGameController gameController)
        {
            return Do(gameController);
        }
        public Task Redo(IGameController gameController)
        {
            return gameController.ProtectCity(this, ActionType.Redo);
        }

        public Task Undo(IGameController gameController)
        {
            return gameController.ProtectCity(this, ActionType.Undo);
        }
    }
}