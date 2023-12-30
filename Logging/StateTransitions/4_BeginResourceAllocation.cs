using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     This is just a UI pause so people can see what happened
    /// </summary>
    public class WaitingForRollOrderToBeginResourceAllocation : LogHeader, ILogController
    {
        public WaitingForRollOrderToBeginResourceAllocation()
        {
        }

        public static async Task PostLog(IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.FinishedRollOrder);

            WaitingForRollOrderToBeginResourceAllocation logHeader = new WaitingForRollOrderToBeginResourceAllocation()
            {
                CanUndo = false, // Rolls to see who starts are final
                Action = CatanAction.ChangedState,
                NewState = GameState.BeginResourceAllocation,
            };

            Contract.Assert(logHeader.OldState == GameState.FinishedRollOrder);

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        /// <summary>
        ///     This is just a UI pause.
        /// </summary>
        /// <param name="gameController"></param>
        /// <returns></returns>
        public async Task Do(IGameController gameController)
        {

            if (gameController.AutoRespondAndTheHuman)
            {
                await BeginAllocationToAllocateResourcesForward.PostLog(gameController);
            }
        }
        public async Task Replay (IGameController gameController)
        {
             await Task.Delay(0);
        }

        public async Task Redo(IGameController gameController)
        {
             await Task.Delay(0);
        }

        public async Task Undo(IGameController gameController)
        {
             await Task.Delay(0);
        }
    }
}