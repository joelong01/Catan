using System.Threading.Tasks;
using Catan.Proxy;

namespace Catan10
{
    internal class SupplementalToWaitingForRoll : LogHeader, ILogController
    {
        public static async Task PostLog(IGameController gameController)
        {

            SupplementalToWaitingForRoll logHeader = new SupplementalToWaitingForRoll()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                NewState = GameState.WaitingForRoll,
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }


        public Task Do(IGameController gameController)
        {
            ChangePlayerHelper.ChangePlayer(gameController, 2);
            return Task.CompletedTask;
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Replay(IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Undo(IGameController gameController)
        {
            ChangePlayerHelper.ChangePlayer(gameController, -2);
            return Task.CompletedTask;
        }
    }
}