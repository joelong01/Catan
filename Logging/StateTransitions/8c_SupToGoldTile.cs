using System.Threading.Tasks;
using Catan.Proxy;

namespace Catan10
{
    internal class SupplementalToPickingGoldTiles : LogHeader, ILogController
    {
        public static async Task PostLog(IGameController gameController)
        {

            SupplementalToPickingGoldTiles logHeader = new SupplementalToPickingGoldTiles()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                NewState = GameState.WaitingForRoll,
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            ChangePlayerHelper.ChangePlayer(gameController, 2);

            await ToPickGold.PostLog(gameController, MainPage.Current.RandomGoldTileCount);
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Replay(IGameController gameController)
        {
            return Do(gameController);
        }

        public async Task Undo(IGameController gameController)
        {
            ChangePlayerHelper.ChangePlayer(gameController, -2);
             await DefaultTask;
        }
    }
}