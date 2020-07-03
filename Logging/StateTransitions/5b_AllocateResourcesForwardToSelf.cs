using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     Move to the Next Player (Previous on Undo)
    ///     Allocate (Revoke) one road and one settlement to the player
    /// </summary>
    public class AllocateResourcesForwardToAllocateResourcesForward : LogHeader, ILogController
    {
        internal static async Task PostLog(IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.AllocateResourceForward);

            AllocateResourcesForwardToAllocateResourcesForward logHeader = new AllocateResourcesForwardToAllocateResourcesForward()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                OldState = GameState.AllocateResourceForward,
                NewState = GameState.AllocateResourceForward,
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            ChangePlayerHelper.ChangePlayer(gameController, 1);
            AllocationPhaseHelper.GrantEntitlements(gameController, gameController.CurrentPlayer.PlayerName);
            if (gameController.CurrentPlayer == gameController.TheHuman && gameController.MainPageModel.Settings.AutoRespond)
            {
                await gameController.PickSettlementsAndRoads();
            }
           
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Undo(IGameController gameController)
        {
            ChangePlayerHelper.ChangePlayer(gameController, -1);
            AllocationPhaseHelper.RevokeEntitlements(gameController, gameController.CurrentPlayer.PlayerName);
            return Task.CompletedTask;
        }
    }
}