using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     UI Pause
    ///     wipes the resources for the turn
    /// </summary>
    public class AllocateResourcesReverseToDoneAllocResources : LogHeader, ILogController
    {
        internal static async Task PostLog(IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.AllocateResourceReverse);

            AllocateResourcesReverseToDoneAllocResources logHeader = new AllocateResourcesReverseToDoneAllocResources()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                OldState = GameState.AllocateResourceReverse,
                NewState = GameState.DoneResourceAllocation,
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public AllocateResourcesReverseToDoneAllocResources()
        {
        }

        /// <summary>
        ///    Resets ResourcesThis Turn for each players, pauses for UI continue
        /// </summary>
        /// <param name="gameController"></param>
        /// <returns></returns>

        public Task Do(IGameController gameController)
        {
            gameController.MainPageModel.PlayingPlayers.ForEach((p) => p.GameData.Resources.ResourcesThisTurn = new TradeResources());
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