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
        #region Methods

        internal static async Task PostLog(IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.DoneResourceAllocation);

            DoneAllocResourcesToWaitingForRoll logHeader = new DoneAllocResourcesToWaitingForRoll()
            {
                CanUndo = true,
                GoldTiles = gameController.NextRandomGoldTiles,
                Action = CatanAction.ChangedState,
                NewState = GameState.WaitingForRoll,
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        #endregion Methods

        #region Properties

        public List<int> GoldTiles { get; set; }

        #endregion Properties

        public async Task Do(IGameController gameController)
        {
            //
            //  hide the rolls in the public data control
            gameController.PlayingPlayers.ForEach((p) =>
            {
                p.GameData.RollOrientation = TileOrientation.FaceDown;
                p.GameData.Resources.ResourcesThisTurn = new TradeResources();
                p.GameData.KnightEligible = false;
            });

            gameController.CurrentPlayer.GameData.KnightEligible = (gameController.CurrentPlayer.GameData.Resources.UnplayedKnights > 0);
            //
            // if we have a roll for this turn already, use it.
            if (gameController.RollLog.CanRedo)
            {
                await WaitingForRollToWaitingForNext.PostLog(gameController, gameController.RollLog.NextRolls);
            }

            await gameController.SetRandomTileToGold(GoldTiles);
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public async Task Undo(IGameController gameController)
        {
            await gameController.ResetRandomGoldTiles();
            gameController.PlayingPlayers.ForEach((p) =>
            {
                p.GameData.Resources.ResourcesThisTurn += p.GameData.Resources.Current;
            });
        }
    }
}
