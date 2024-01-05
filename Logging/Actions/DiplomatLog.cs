using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.FaceAnalysis;

namespace Catan10
{
    public class DiplomatLog : LogHeader, ILogController
    {
        public int DestroyedRoadIndex { get; set; }
        public Guid RoadOwner { get; set; } = Guid.Empty;
        public static async Task PostLogEntry(IGameController controller, RoadCtrl destroyedRoad)
        {
            DiplomatLog logEntry = new DiplomatLog()
            {
                DestroyedRoadIndex = destroyedRoad.Index,
                RoadOwner = destroyedRoad.Owner.PlayerIdentifier,
                NewState = GameState.WaitingForNext
            };
            await controller.PostMessage(logEntry, ActionType.Normal);
        }
        public async Task Do(IGameController gameController)
        {
            UpdateRoadLog updateRoadLog = new UpdateRoadLog()
            {
                Action = CatanAction.UpdatedRoadState,
                RoadIndex = DestroyedRoadIndex,
                OldRoadState = RoadState.Road,
                NewRoadState = RoadState.Unowned,
                OldRaceTracking = gameController.RaceTracking,
                SentBy = gameController.CurrentPlayer
            };
            await gameController.SetRoadState(updateRoadLog); // this grants CurrentUser an entitlement
            if (gameController.CurrentPlayer.PlayerIdentifier != RoadOwner)
            {
                gameController.CurrentPlayer.GameData.Resources.ConsumeEntitlement(Entitlement.Road);
            }

            gameController.CurrentPlayer.GameData.Resources.ConsumeEntitlement(Entitlement.Diplomat);

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
            if (this.RoadOwner != gameController.CurrentPlayer.PlayerIdentifier)
            {
                player.GameData.Resources.GrantEntitlement(Entitlement.Road); // this will be consumed next
            }
            gameController.UpdateRoadState(player, gameController.GetRoad(DestroyedRoadIndex), RoadState.Unowned, RoadState.Road, gameController.RaceTracking);
            await Task.Delay(0);

        }
    }
}
