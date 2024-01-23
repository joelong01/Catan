using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Management.Update;

namespace Catan10
{
    internal class LongestRoadChangedLog : LogHeader, ILogController
    {
        public Guid OldPlayerIndex { get; set; }
        public Guid NewPlayerIndex { get; set; }
        public int[] PreviousLongestRoads { get; set; }
        public int[] NewLongestRoads { get; set; }
        /// <summary>
        ///     
        ///     Logs the previous longest road state and then calculates what the new longest road state should be
        ///     Do will set the LongestRoad and HasLongestRoad values for each player.
        ///     Undo puts it back to its previous state.
        ///     
        ///     Should be called when a building or road is built
        /// 
        ///     The CurrentPlayer's longest road can get longer.
        ///     The CurrentPlayer can affect somebody else's longest road by building a city
        ///     The CurrentPlayer can remove somebody else's road in Cities and Knights by playing a dev card to destroy a road
        ///     The CurrentPlayer can play a city and break the longest road, giving it to a different player
        ///     
        ///     the LongestRoad *never* changes if there is a tie
        ///     
        /// </summary>
        /// <param name="gameController"></param>
        /// <param name="oldPlayer"></param>
        /// <param name="newPlayer"></param>
        /// <returns></returns>
        public static async Task CalculateAndSetLongestRoad(IGameController gameController)
        {
            PlayerModel prevLongestRoadPlayer = null;
            PlayerModel newLongestRoadPlayer = null;
            var playingPlayers = gameController.PlayingPlayers;
            int[] currentLongestRoads = new int[playingPlayers.Count];
            int[] newLongestRoads = new int[playingPlayers.Count];
            int maxLongestRoad = -1;
            for (int i = 0; i < playingPlayers.Count; i++)
            {
                var player = playingPlayers[i];
                currentLongestRoads[i] = player.GameData.LongestRoad;
                newLongestRoads[i] = player.CalculateLongestRoad(); // updates as well as calculates
                if (newLongestRoads[i] > maxLongestRoad)
                {
                    maxLongestRoad = newLongestRoads[i];
                    // this means the first to max gets set, but we only use this in the case where there is not a tie
                    if (maxLongestRoad > 4)
                    {
                        newLongestRoadPlayer = player;
                    }
                }
                if (player.GameData.HasLongestRoad)
                {
                    prevLongestRoadPlayer = player;
                }
            }

            // the current owner of longest road still has the longest road. this fixes ties.
            if (maxLongestRoad > 4 && prevLongestRoadPlayer != null)
            {

                int prevPlayerIndex = playingPlayers.IndexOf(prevLongestRoadPlayer);
                if (newLongestRoads[prevPlayerIndex] == maxLongestRoad)
                {
                    newLongestRoadPlayer = prevLongestRoadPlayer;
                }

                // need the case where P0 has their road broken and P1 and P2 have the same value > 4
                // whoever got to 5 first won the race and get longest road.  In the meantime you can
                // workaround this unusual case on their next turn by buying them a road, building it
                // so they get longest road and then undo the build and undo the purchase
            }



            // 3. the longest road did change

            LongestRoadChangedLog logEntry = new LongestRoadChangedLog()
            {
                OldPlayerIndex = prevLongestRoadPlayer == null ? Guid.Empty : prevLongestRoadPlayer.PlayerIdentifier,
                NewPlayerIndex = newLongestRoadPlayer== null ? Guid.Empty : newLongestRoadPlayer.PlayerIdentifier,
                PreviousLongestRoads = currentLongestRoads,
                NewLongestRoads = newLongestRoads,
                UndoNext = true, // this will make it so that the action that caused the change is also undone

            };

            await gameController.PostMessage(logEntry, ActionType.Normal);
        }
        public Task Do(IGameController gameController)
        {
            var oldPlayer = gameController.PlayerFromId(OldPlayerIndex);
            var newPlayer = gameController.PlayerFromId(NewPlayerIndex);
            if (oldPlayer != null)
            {
                oldPlayer.GameData.HasLongestRoad = false;
            }
            if (newPlayer != null)
            {
                newPlayer.GameData.HasLongestRoad = true;
            }
            for (int i = 0; i < gameController.PlayingPlayers.Count; i++)
            {
                gameController.PlayingPlayers[i].GameData.LongestRoad = NewLongestRoads[i];
            }
            return Task.Delay(0);
        }

        public async Task Redo(IGameController gameController)
        {
            await Do(gameController);
        }

        public async Task Replay(IGameController gameController)
        {
            await Do(gameController);
        }

        public Task Undo(IGameController gameController)
        {
            var oldPlayer = gameController.PlayerFromId(OldPlayerIndex);
            var newPlayer = gameController.PlayerFromId(NewPlayerIndex);
            if (oldPlayer != newPlayer)
            {
                if (oldPlayer != null)
                {
                    oldPlayer.GameData.HasLongestRoad = true;
                }
                if (newPlayer != null)
                {
                    newPlayer.GameData.HasLongestRoad = false;
                }
            }
            for (int i = 0; i < gameController.PlayingPlayers.Count; i++)
            {
                gameController.PlayingPlayers[i].GameData.LongestRoad = PreviousLongestRoads[i];
            }
            return Task.Delay(0);
        }
    }
}
