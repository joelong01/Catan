using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Catan10
{
    public class RollOrderFinalizedLog : LogHeader, ILogController
    {
        #region Methods

        public static Task PostLog(IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForRollForOrder);

            RollOrderFinalizedLog logHeader = new RollOrderFinalizedLog()
            {
                CanUndo = false,
                Action = CatanAction.ChangedState,
                NewState = GameState.FinishedRollOrder,
            };

            return gameController.PostMessage(logHeader, Catan.Proxy.ActionType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            return Task.CompletedTask;
        }

        public Task Redo(IGameController gameController)
        {
            return Task.CompletedTask;
        }

        public Task Undo(IGameController gameController)
        {
            return Task.CompletedTask;
        }

        #endregion Methods
    }
}