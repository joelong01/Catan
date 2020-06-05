using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     Grant/Revoke (Do/Undo) one settlement and one road
    /// </summary>
    public class AllocateResourcesForwardToAllocateResourcesReverse : LogHeader, ILogController
    {
        #region Methods

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

        #endregion Methods

        #region Constructors

        public AllocateResourcesForwardToAllocateResourcesReverse()
        {
        }

        #endregion Constructors

        public Task Do(IGameController gameController)
        {
            AllocationPhaseHelper.GrantEntitlements(gameController, gameController.CurrentPlayer.PlayerName);
            return Task.CompletedTask;
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Undo(IGameController gameController)
        {
            AllocationPhaseHelper.RevokeEntitlements(gameController, gameController.CurrentPlayer.PlayerName);
            return Task.CompletedTask;
        }
    }
}
