using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     Move player
    ///     Grant entitlement
    /// </summary>
    public class AllocateResourcesReverseToAllocateResourcesReverse : LogHeader, ILogController
    {
        #region Methods

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

        #endregion Methods

        #region Constructors

        public AllocateResourcesReverseToAllocateResourcesReverse()
        {
        }

        #endregion Constructors

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
            AllocationPhaseHelper.ChangePlayer(gameController, 1);
            AllocationPhaseHelper.RevokeEntitlements(gameController, gameController.CurrentPlayer.PlayerName);
            return Task.CompletedTask;
        }
    }
}
