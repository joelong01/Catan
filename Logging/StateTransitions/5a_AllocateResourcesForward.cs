using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     Allocate one road and one settlement to the player
    ///     Reset the roll control for next time
    /// </summary>
    public class BeginAllocationToAllocateResourcesForward : LogHeader, ILogController
    {
        internal static async Task PostLog(IGameController gameController)
        {
            //
            //  Player Order and Current players should be consistent
            //  Verify the players are consistent across the machines
            await VerifyPlayers.PostMessage(gameController);

            Contract.Assert(gameController.CurrentGameState == GameState.BeginResourceAllocation);

            BeginAllocationToAllocateResourcesForward logHeader = new BeginAllocationToAllocateResourcesForward()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                NewState = GameState.AllocateResourceForward,
            };

            Contract.Assert(logHeader.OldState == GameState.BeginResourceAllocation);

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
           

            AllocationPhaseHelper.GrantEntitlements(gameController, gameController.CurrentPlayer.PlayerName);
             await Task.Delay(0);
        }
        public Task Replay (IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public async Task Undo(IGameController gameController)
        {
            AllocationPhaseHelper.RevokeEntitlements(gameController, gameController.CurrentPlayer.PlayerName);
             await Task.Delay(0);
        }
    }
}