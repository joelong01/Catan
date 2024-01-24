using Catan.Proxy;
using System.Threading.Tasks;

namespace Catan10
{
    internal class SupplementalToSupplemental : LogHeader, ILogController
    {
        public static async Task PostLog(IGameController gameController)
        {

            SupplementalToSupplemental logHeader = new SupplementalToSupplemental()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                NewState = GameState.Supplemental,
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {

            ChangePlayerHelper.ChangePlayer(gameController, 1);
             await DefaultTask;

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
            ChangePlayerHelper.ChangePlayer(gameController, -1);
             await DefaultTask;
        }
    }
}