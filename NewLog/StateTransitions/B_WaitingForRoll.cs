
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     Need to get the random gold tiles for this turn
    /// </summary>
    public class DoneAllocResourcesToWaitingForRoll : LogHeader, ILogController
    {
        public List<int> GoldTiles { get; set; }
        internal static async Task PostLog(IGameController gameController)
        {

            Contract.Assert(gameController.CurrentGameState == GameState.DoneResourceAllocation);
            

            DoneAllocResourcesToWaitingForRoll logHeader = new DoneAllocResourcesToWaitingForRoll()
            {
                CanUndo = true,
                GoldTiles = gameController.GetRandomGoldTiles(),
                Action = CatanAction.ChangedState,
                OldState = GameState.DoneResourceAllocation,
                NewState = GameState.WaitingForRoll,
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            await gameController.SetRandomTileToGold(GoldTiles);
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public async Task Undo(IGameController gameController)
        {
            await gameController.ResetRandomGoldTiles();
        }
    }
}
