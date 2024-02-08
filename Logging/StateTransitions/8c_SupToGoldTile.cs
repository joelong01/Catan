using System;
using System.Threading.Tasks;
using Catan.Proxy;

namespace Catan10
{
    internal class SupplementalToPickingGoldTiles : LogHeader, ILogController
    {
        public Guid StartPlayerId { get; set; }
        public Guid CurrentPlayerId { get; set; }
        public static async Task PostLog(IGameController gameController, Guid startPlayerId)
        {

            SupplementalToPickingGoldTiles logHeader = new SupplementalToPickingGoldTiles()
            {
                CanUndo = true,
                Action = CatanAction.ChangedState,
                NewState = GameState.WaitingForRoll,
                StartPlayerId = startPlayerId, 
                CurrentPlayerId = gameController.CurrentPlayer.PlayerIdentifier
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            var nextPlayer = ChangePlayerHelper.NextPlayerId(gameController, StartPlayerId, 1); // first player after the last player to roll
            ChangePlayerHelper.ChangePlayerTo(gameController, nextPlayer);

            await ToPickGold.PostLog(gameController, MainPage.Current.RandomGoldTileCount);
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Replay(IGameController gameController)
        {
            return Do(gameController);
        }

        public async Task Undo(IGameController gameController)
        {
            ChangePlayerHelper.ChangePlayerTo(gameController, CurrentPlayerId);
            await DefaultTask;
        }
    }
}