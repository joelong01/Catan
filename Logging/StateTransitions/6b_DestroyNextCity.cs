using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan10
{
    
    internal class DestroyCity_Next : LogHeader, ILogController
    {
        int Count { get; set; }    

        public static async Task PostLog(IGameController gameController)
        {
            var players = gameController.PlayingPlayers;
            GameState newState = GameState.MustDestroyCity;
            PlayerModel nextVictim = null;
            int currentPlayerIndex = players.IndexOf(gameController.CurrentPlayer);
            for (int i=0; i< players.Count; i++)
            {
                int idx = (i + currentPlayerIndex) % players.Count;
                if (players[idx].GameData.Resources.HasUnusedEntitlment(Entitlement.DestroyCity))
                {
                    nextVictim = players[idx];
                    break;
                }
            }
            int toMove = 0;
            if (nextVictim == null) //nobody has anything to play
            {
                if (gameController.CurrentPlayer != gameController.LastPlayerToRoll)
                {
                    toMove = GetPlayerDistance(players, gameController.CurrentPlayer, gameController.LastPlayerToRoll);
                    newState = GameState.WaitingForNext;
                }
            } else
            {
                toMove = GetPlayerDistance(players, gameController.CurrentPlayer, nextVictim);
            }

      

            var logHeader = new DestroyCity_Next()
            {
                Count = toMove,
                NewState = newState

            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public static int GetPlayerDistance(IList<PlayerModel> players, PlayerModel startPlayer, PlayerModel endPlayer)
        {
            int begin = players.IndexOf (startPlayer);
            int end = players.IndexOf (endPlayer);

            return ( end - begin + players.Count ) % players.Count;
        }

        public static int GetNextPlayer(IList<PlayerModel> players, PlayerModel startPlayer)
        {
            int startIndex = players.IndexOf (startPlayer);
            startIndex = (startIndex + 1) % players.Count;
            return startIndex;

        }

        public Task Do(IGameController gameController)
        {
            if (this.Count == 0) return Task.CompletedTask;

            List<PlayerModel> playingPlayers = gameController.PlayingPlayers;

            int idx = playingPlayers.IndexOf(gameController.CurrentPlayer);

            Contract.Assert(idx != -1, "The player needs to be playing!");

            int count = playingPlayers.Count;

            // Calculate the index of the new player, wrapping around if necessary
            int newPlayerIndex = (idx + this.Count) % count;
            if (newPlayerIndex < 0)
            {
                // Ensure it's positive if it became negative due to modulo
                newPlayerIndex += count;
            }

            var newPlayer = playingPlayers[newPlayerIndex];
            gameController.CurrentPlayer = newPlayer;
            MainPage.Current.TheHuman = gameController.CurrentPlayer;
            return Task.CompletedTask;
        }

        public Task Redo(IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Replay(IGameController gameController)
        {
            return Do(gameController);
        }

        public Task Undo(IGameController gameController)
        {
            List<PlayerModel> playingPlayers = gameController.PlayingPlayers;

            int idx = playingPlayers.IndexOf(gameController.CurrentPlayer);

            Contract.Assert(idx != -1, "The player needs to be playing!");

            int count = playingPlayers.Count;

            // Calculate the index of the new player, wrapping around if necessary
            int newPlayerIndex = (idx - this.Count) % count;
            if (newPlayerIndex < 0)
            {
                // Ensure it's positive if it became negative due to modulo
                newPlayerIndex += count;
            }

            var newPlayer = playingPlayers[newPlayerIndex];
            gameController.CurrentPlayer = newPlayer;
            MainPage.Current.TheHuman = gameController.CurrentPlayer;
            return Task.CompletedTask;
        }
    }
}
