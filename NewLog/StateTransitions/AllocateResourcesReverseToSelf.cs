using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    public class AllocateResourcesReverseToAllocateResourcesReverse : LogHeader, ILogController
    {
        public AllocateResourcesReverseToAllocateResourcesReverse() { }
        internal static async Task PostLog(IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.AllocateResourceReverse);

            AllocateResourcesReverseToAllocateResourcesReverse logHeader = new AllocateResourcesReverseToAllocateResourcesReverse()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                OldState = GameState.AllocateResourceReverse,
                NewState = GameState.AllocateResourceReverse,
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            AllocationPhaseHelper.ChangePlayer(gameController, -1);
            AllocationPhaseHelper.GrantEntitlements(gameController, gameController.CurrentPlayer.PlayerName);
            return Task.CompletedTask;
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Undo(IGameController gameController)
        {
            AllocationPhaseHelper.ChangePlayer(gameController, -1);
            AllocationPhaseHelper.RevokeEntitlements(gameController, gameController.CurrentPlayer.PlayerName);
            return Task.CompletedTask;
        }
    }
}
