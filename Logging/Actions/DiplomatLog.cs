using System;
using System.Threading.Tasks;

namespace Catan10
{
    public class DiplomatLog : LogHeader, ILogController
    {
        public int DestroyedRoadIndex { get; set; }
        public Guid RoadOwner { get; set; } = Guid.Empty;
        public bool GrantRoadEntitlement { get; set; } = false;
        public static async Task PostLogEntry(IGameController controller, RoadCtrl destroyedRoad)
        {

            
            DiplomatLog logEntry = new DiplomatLog()
            {
                DestroyedRoadIndex = destroyedRoad.Index,
                RoadOwner = destroyedRoad.Owner.PlayerIdentifier,
                NewState = GameState.WaitingForNext,
                GrantRoadEntitlement = (destroyedRoad.Owner == controller.CurrentPlayer)
            };
            await controller.PostMessage(logEntry, ActionType.Normal);
        }
        public async Task Do(IGameController gameController)
        {
            var road =  gameController.GetRoad(DestroyedRoadIndex);
            await UpdateRoadLog.PostLogEntry(gameController, road, RoadState.Unowned);
            



            gameController.CurrentPlayer.GameData.Resources.ConsumeEntitlement(Entitlement.Diplomat);
            if (GrantRoadEntitlement)
            {
                gameController.CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.Road);
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
            gameController.CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.Diplomat);
            var player = gameController.PlayerFromId(this.RoadOwner);
            if (GrantRoadEntitlement)
            {
                player.GameData.Resources.RevokeEntitlement(Entitlement.Road); // this will be consumed next
            }

            await DefaultTask;

        }
    }
}
