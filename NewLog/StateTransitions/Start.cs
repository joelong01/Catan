using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    public class WaitingForRollOrderToWaitingForStart : LogHeader, ILogController
    {
        public WaitingForRollOrderToWaitingForStart()
        {
        }

        public static async Task PostLog(IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForRollForOrder);

            WaitingForRollOrderToWaitingForStart logHeader = new WaitingForRollOrderToWaitingForStart()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                OldState = GameState.WaitingForRollForOrder,
                NewState = GameState.WaitingForStart,
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
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
                await WaitingForStartToAllocateResourcesForward.PostLog(gameController);
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
    }
}