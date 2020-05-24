using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    /// <summary>
    ///     This class has all the data associated with changing a player.
    /// </summary>
    public class ChangePlayerLog : LogHeader, ILogController
    {
        public ChangePlayerLog()
        {
        }

        public List<int> HighlightedTiles { get; set; } = new List<int>();
        public string NewCurrentPlayer { get; set; }
        public List<int> NewRandomGoldTiles { get; set; } = new List<int>();
        public List<int> OldRandomGoldTiles { get; set; } = new List<int>();
        public string PreviousPlayer { get; set; }

        private bool ListsEqual(List<int> l1, List<int> l2)
        {
            if (l1.Count != l2.Count)
                return false;

            for (int i = 0; i < l1.Count; i++)
            {
                if (l1[i] != l2[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static async Task ChangePlayer(IGameController gameController, int numberofPositions, GameState newState)
        {
            Contract.Assert(gameController.CurrentPlayer != null);

            List<PlayerModel> playingPlayers = gameController.PlayingPlayers;

            int idx = playingPlayers.IndexOf(gameController.CurrentPlayer);

            Contract.Assert(idx != -1, "The player needs to be playing!");

            idx += numberofPositions;
            int count = playingPlayers.Count;
            if (idx >= count) idx -= count;
            if (idx < 0) idx += count;

            var newPlayer = playingPlayers[idx];
            await SetCurrentPlayer(gameController, newPlayer, newState);
        }

        public static async Task SetCurrentPlayer(IGameController gameController, PlayerModel newPlayer, GameState newState)
        {
            ChangePlayerLog logHeader = new ChangePlayerLog
            {
                SentBy = gameController.CurrentPlayer.PlayerName,
                PreviousPlayer = gameController.CurrentPlayer.PlayerName,
                OldState = gameController.CurrentGameState,
                NewState = newState,
                Action = CatanAction.ChangePlayerAndSetState,
                NewCurrentPlayer = newPlayer.PlayerName,
                OldRandomGoldTiles = gameController.CurrentRandomGoldTiles,
                NewRandomGoldTiles = gameController.NextRandomGoldTiles,
                HighlightedTiles = gameController.HighlightedTiles
            };

            await gameController.PostMessage(logHeader, CatanMessageType.Normal);
        }

        public Task Do(IGameController gameController)
        {
            return gameController.ChangePlayer(this);
        }

        /// <summary>
        ///     These are not static because we use them when Deserializing log records, so we already have an instance.
        /// </summary>
        /// <param name="gameController"></param>
        /// <param name="logHeader"></param>
        /// <returns></returns>
        public Task Redo(IGameController gameController)
        {
            return gameController.ChangePlayer(this);
        }

        public override string ToString()
        {
            return $"[NewCurrentPlayer={NewCurrentPlayer}][Previous={PreviousPlayer}]" + base.ToString() + $"[OldRandomGoldTiles.Count={OldRandomGoldTiles}][NewRandomGoldTiles={NewRandomGoldTiles}]";
        }

        public Task Undo(IGameController gameController)
        {
            return gameController.UndoChangePlayer(this);
        }
    }
}