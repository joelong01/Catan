
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     This is just for a UI Pause
    /// </summary>
    public class DoneAllocResourcesToWaitingForRoll : LogHeader, ILogController
    {
        internal static async Task PostLog(IGameController gameController)
        {

            Contract.Assert(gameController.CurrentGameState == GameState.DoneResourceAllocation);

            DoneAllocResourcesToWaitingForRoll logHeader = new DoneAllocResourcesToWaitingForRoll()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                OldState = GameState.DoneResourceAllocation,
                NewState = GameState.WaitingForRoll,
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
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
    }
}
