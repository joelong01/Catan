using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;
using Windows.UI.WindowManagement;

namespace Catan10
{
    /// <summary>
    ///     1. move the player
    ///     2. set the random gold tiles
    ///     3. reset ResourcesThisTurn
    ///     4. Reset and Show the roll UI
    /// </summary>
    public class WaitingForNextToPickingRandomGoldTiles : LogHeader, ILogController
    {
        public TradeResources ResourcesThisTurn { get; set; }
        public DevCardModel DevCardPlayedThisTurn { get; set; }

        public static async Task PostLog(IGameController gameController)
        {

            WaitingForNextToPickingRandomGoldTiles logHeader = new WaitingForNextToPickingRandomGoldTiles()
            {
                ResourcesThisTurn = ResourceCardCollection.ToTradeResources(gameController.CurrentPlayer.GameData.Resources.ResourcesThisTurn),
                Action = CatanAction.ChangedState,
                NewState = GameState.PickingRandomGoldTiles,
                DevCardPlayedThisTurn = gameController.CurrentPlayer.GameData.Resources.ThisTurnsDevCard

            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            //
            //  remember the log is already written!
            ChangePlayerHelper.ChangePlayer(gameController, 1);
            await gameController.ResetRollControl();
            gameController.StopHighlightingTiles();
            await ToPickGold.PostLog(gameController, MainPage.Current.RandomGoldTileCount); // this will move the state to waiting for roll
        }

        public async Task Replay (IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForRoll);
            ChangePlayerHelper.ChangePlayer(gameController, 1);
            await gameController.ResetRollControl();
            gameController.StopHighlightingTiles();
        }

        public async Task Redo(IGameController gameController)
        {
            await Do(gameController);
        }

        public async Task Undo(IGameController gameController)
        {

            ChangePlayerHelper.ChangePlayer(gameController, -1);

            //
            //  hide the rolls in the public data control
            gameController.PlayingPlayers.ForEach((p) =>
            {
                p.GameData.Resources.ResourcesThisTurn.Reset();
            });

            gameController.CurrentPlayer.GameData.Resources.ThisTurnsDevCard = this.DevCardPlayedThisTurn;

            await DefaultTask;
        }
    }
}