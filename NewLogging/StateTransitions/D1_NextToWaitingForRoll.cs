
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;
using Windows.Media.Playback;

namespace Catan10
{
    /// <summary>
    ///     1. move the player
    ///     2. set the random gold tiles
    ///     3. reset ResourcesThisTurn
    ///     4. Reset and Show the roll UI 
    /// </summary>
    public class WaitingForNextToWaitingForRoll : LogHeader, ILogController
    {
        public List<int> GoldTiles { get; set; }
        public bool KnightEligible { get; set; }
        public TradeResources ResourcesThisTurn { get; set; }
        public static async Task PostLog(IGameController gameController)
        {
            WaitingForNextToWaitingForRoll logHeader = new WaitingForNextToWaitingForRoll()
            {
                CanUndo = true,
                ResourcesThisTurn = gameController.CurrentPlayer.GameData.Resources.ResourcesThisTurn,
                KnightEligible = gameController.CurrentPlayer.GameData.KnightEligible, // need for undo
                GoldTiles = gameController.CurrentRandomGoldTiles,
                Action = CatanAction.ChangedState,
                NewState = GameState.WaitingForRoll,
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);

        }
        public async Task Do(IGameController gameController)
        {
            //
            //  remember the log is already written!

            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForRoll);
            AllocationPhaseHelper.ChangePlayer(gameController, 1);

            //
            //  hide the rolls in the public data control
            gameController.PlayingPlayers.ForEach((p) =>
            {
                p.GameData.RollOrientation = TileOrientation.FaceDown;
                p.GameData.Resources.ResourcesThisTurn = new TradeResources(); // reset PublicDataCtrl resources
                p.GameData.KnightEligible = false; // will set the current player's flag below
            });

            gameController.CurrentPlayer.GameData.KnightEligible = (gameController.CurrentPlayer.GameData.Resources.UnplayedKnights > 0);
            await gameController.ResetRollControl();
            //
            // if we have a roll for this turn already, use it.
            if (gameController.RollLog.CanRedo)
            {
                await WaitingForRollToWaitingForNext.PostLog(gameController, gameController.RollLog.NextRolls);
            }

            

            await gameController.SetRandomTileToGold(gameController.NextRandomGoldTiles);
        }

        public async Task Redo(IGameController gameController)
        {
            await Do(gameController);
        }

        public async Task Undo(IGameController gameController)
        {
            AllocationPhaseHelper.ChangePlayer(gameController, -1);

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
    }
}
