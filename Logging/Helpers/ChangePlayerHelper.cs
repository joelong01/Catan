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
        public static void ChangePlayer(IGameController gameController, int numberOfPositions)
        {
            Contract.Assert(gameController.CurrentPlayer != null);

            // Notify all players that the turn ended
            gameController.PlayingPlayers.ForEach(p => p.TurnEnded());

            List<PlayerModel> playingPlayers = gameController.PlayingPlayers;

            int idx = playingPlayers.IndexOf(gameController.CurrentPlayer);

            Contract.Assert(idx != -1, "The player needs to be playing!");

            int count = playingPlayers.Count;

            // Calculate the index of the new player, wrapping around if necessary
            int newPlayerIndex = (idx + numberOfPositions) % count;
            if (newPlayerIndex < 0)
            {
                // Ensure it's positive if it became negative due to modulo
                newPlayerIndex += count;
            }

            var newPlayer = playingPlayers[newPlayerIndex];
            gameController.CurrentPlayer = newPlayer;

            // Notify the current player that the turn started
            gameController.CurrentPlayer.TurnStarted();
            MainPage.Current.TheHuman = gameController.CurrentPlayer;

        }

    }
}