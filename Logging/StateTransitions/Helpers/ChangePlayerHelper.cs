using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Catan10
{
    /// <summary>
    ///     Use this in the logging when you need to change a player
    ///     1. Moves the Current Player
    ///     2. Tells the player that the turn ended
    ///     3. Tells a player that their turn started
    /// </summary>
    public static class ChangePlayerHelper
    {
        public static void ChangePlayer(IGameController gameController, int numberofPositions)
        {
            Contract.Assert(gameController.CurrentPlayer != null);

            //
            //  tell everybody the turn ended
            gameController.PlayingPlayers.ForEach((p) =>
            {
                p.TurnEnded();
            });

            List<PlayerModel> playingPlayers = gameController.PlayingPlayers;

            int idx = playingPlayers.IndexOf(gameController.CurrentPlayer);

            Contract.Assert(idx != -1, "The player needs to be playing!");

            idx += numberofPositions;
            int count = playingPlayers.Count;
            if (idx >= count) idx -= count;
            if (idx < 0) idx += count;

            var newPlayer = playingPlayers[idx];
            gameController.CurrentPlayer = newPlayer;

            //
            //  tell current player that turn started
            gameController.CurrentPlayer.TurnStarted();
        }
    }
}