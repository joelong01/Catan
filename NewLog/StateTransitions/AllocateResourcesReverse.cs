using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    public class AllocateResourcesForwardToAllocateResourcesReverse : LogHeader, ILogController
    {
        public AllocateResourcesForwardToAllocateResourcesReverse() { }
        internal static async Task PostLog(IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.AllocateResourceForward);

            AllocateResourcesForwardToAllocateResourcesReverse logHeader = new AllocateResourcesForwardToAllocateResourcesReverse()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                OldState = GameState.AllocateResourceForward,
                NewState = GameState.AllocateResourceReverse,
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            return WaitingForStartToAllocateResourcesForward.DoAllocateForwardResources(gameController, this.SentBy);
        }

        public Task Redo(IGameController gameController)
        {
            return WaitingForStartToAllocateResourcesForward.DoAllocateForwardResources(gameController, this.SentBy);
        }

        public Task Undo(IGameController gameController)
        {
            return WaitingForStartToAllocateResourcesForward.UndoAllocateForwardResources(gameController, this.SentBy);
        }
    }
}
