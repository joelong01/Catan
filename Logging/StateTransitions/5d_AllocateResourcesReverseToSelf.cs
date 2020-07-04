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

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public AllocateResourcesReverseToAllocateResourcesReverse()
        {
        }

        public async Task Do(IGameController gameController)
        {
            ChangePlayerHelper.ChangePlayer(gameController, -1);
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
            ChangePlayerHelper.ChangePlayer(gameController, 1);
            AllocationPhaseHelper.RevokeEntitlements(gameController, gameController.CurrentPlayer.PlayerName);
            return Task.CompletedTask;
        }
    }
}