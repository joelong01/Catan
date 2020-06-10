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
        #region Constructors

        public WaitingForRollOrderToBeginResourceAllocation()
        {
        }

        #endregion Constructors

        #region Methods

        public static async Task PostLog(IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForRollForOrder);

            WaitingForRollOrderToBeginResourceAllocation logHeader = new WaitingForRollOrderToBeginResourceAllocation()
            {
                CanUndo = false, // Rolls to see who starts are final
                Action = CatanAction.ChangedState,
                NewState = GameState.BeginResourceAllocation,
            };

            Contract.Assert(logHeader.OldState == GameState.WaitingForRollForOrder);

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
