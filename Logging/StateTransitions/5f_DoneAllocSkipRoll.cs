using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     Need to get the random gold tiles for this turn
    /// </summary>
    public class DoneAllocResourcesToPickingGoldTile : LogHeader, ILogController
    {
        internal static async Task PostLog(IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.DoneResourceAllocation ||
                gameController.CurrentGameState == GameState.PickingRandomGoldTiles);

            DoneAllocResourcesToPickingGoldTile logHeader = new DoneAllocResourcesToPickingGoldTile()
            {
                CanUndo = true,
                RollState = gameController.GetNextRollState(),
                Action = CatanAction.ChangedState,
                NewState = GameState.PickingRandomGoldTiles,
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public RollState RollState { get; set; } = null;

       

        public async Task Do(IGameController gameController)
        {
            Contract.Assert(this.RollState.PlayerName == gameController.CurrentPlayer.PlayerName);

            //
            //  hide the rolls in the public data control
            gameController.PlayingPlayers.ForEach((p) =>
            {
                p.GameData.Resources.ResourcesThisTurn.Reset();
            });

            //
            //  pick random gold count
            await ToPickGold.PostLog(gameController, MainPage.Current.RandomGoldTileCount);
        }

        public async Task Replay (IGameController gameController)
        {

            //
            //  hide the rolls in the public data control
            gameController.PlayingPlayers.ForEach((p) =>
            {
                p.GameData.Resources.ResourcesThisTurn.Reset();
            });

             await DefaultTask;

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
                p.GameData.Resources.ResourcesThisTurn.AddResources(p.GameData.Resources.CurrentResources);
            });
        }
    }
}