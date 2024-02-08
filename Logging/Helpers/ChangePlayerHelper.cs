using System;
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
        public static void ChangePlayerTo(IGameController gameController, Guid playerId)
        {
            // Notify all players that the turn ended
            gameController.PlayingPlayers.ForEach(p => p.TurnEnded());
            var newPlayer = gameController.PlayerFromId(playerId);
            gameController.CurrentPlayer = newPlayer;
            gameController.CurrentPlayer.TurnStarted();
            MainPage.Current.TheHuman = gameController.CurrentPlayer;
        }

        public static int DistanceBetweenPlayers(IGameController gameController, Guid startId, Guid endId)
        {
            List<PlayerModel> playingPlayers = gameController.PlayingPlayers;
            var startPlayer = gameController.PlayerFromId(startId);
            var endPlayer = gameController.PlayerFromId(endId);
            int idxStart = playingPlayers.IndexOf(startPlayer);
            int idxEnd = playingPlayers.IndexOf(endPlayer);

            int distance = (idxEnd - idxStart + playingPlayers.Count) % playingPlayers.Count;
            return distance;

            
        }

        public static Guid NextPlayerId(IGameController gameController, Guid startPlayerId,  int numberOfPositions)
        {
            List<PlayerModel> playingPlayers = gameController.PlayingPlayers;
            var startPlayer = gameController.PlayerFromId(startPlayerId);
            int idx = playingPlayers.IndexOf(startPlayer);

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
            return newPlayer.PlayerIdentifier;
        }

        public static void ChangePlayer(IGameController gameController, int numberOfPositions)
        {
            Contract.Assert(gameController.CurrentPlayer != null);
            var id = NextPlayerId(gameController, gameController.CurrentPlayer.PlayerIdentifier, numberOfPositions);
            ChangePlayerTo(gameController, id);

        }

    }
}