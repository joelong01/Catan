
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Catan10
{


    /// <summary>
    ///     This class has all the data associated with a updating a road

    /// </summary>
    public class UpdateRoadModel : LogHeader
    {
        public int RoadIndex { get; set; } = -1;
        public RoadState OldRoadState { get; set; } = RoadState.Unowned;
        public RoadState NewRoadState { get; set; } = RoadState.Unowned;
        public RoadRaceTracking OldRaceTracking { get; set; } = new RoadRaceTracking();
        public RoadRaceTracking NewRaceTracking { get; set; } = new RoadRaceTracking();
        [JsonIgnore]
        public RoadCtrl Road { get; set; } = null;
        public int GameIndex { get; set; } = -1;

        public UpdateRoadModel() { }


        //   await AddLogEntry(CurrentPlayer, GameState, CatanAction.UpdatedRoadState, true, logType, road.Number, new LogRoadUpdate(_gameView.CurrentGame.Index, road, oldState, road.RoadState));
    }

    public class UpdateRoadController
    {
        public static UpdateRoadModel SetRoadState(MainPage page, RoadCtrl road, RoadState newState, RoadRaceTracking raceTracker)
        {
            RoadState oldState = road.RoadState;

            if (newState == oldState)
            {
                throw new Exception("Why are you updating the road state to be the same state it already is?");
            }

            UpdateRoadModel model = new UpdateRoadModel()
            {
                Page = page,
                Player = page.CurrentPlayer,
                PlayerIndex = page.CurrentPlayer.AllPlayerIndex,
                PlayerName = page.CurrentPlayer.PlayerName,
                OldState = page.NewGameState,
                Action = CatanAction.UpdatedRoadState,
                RoadIndex = road.Index,
                OldRoadState = road.RoadState,
                NewRoadState = newState,
                GameIndex = page.GameContainer.CurrentGame.Index,
                OldRaceTracking = raceTracker

            };

            road.RoadState = newState;
            switch (newState)
            {
                case RoadState.Unowned:
                    if (oldState == RoadState.Ship)
                    {
                        model.Player.GameData.Ships.Remove(road);
                    }
                    else
                    {
                        model.Player.GameData.Roads.Remove(road);
                    }

                    road.Owner = null;
                    road.Number = -1;
                    break;
                case RoadState.Road:
                    road.Number = model.Player.GameData.Roads.Count; // undo-able                    
                    model.Player.GameData.Roads.Add(road);
                    road.Owner = model.Player;
                    break;
                case RoadState.Ship:
                    model.Player.GameData.Roads.Remove(road); // can't be a ship if you aren't a road
                    model.Player.GameData.Ships.Add(road);
                    break;
                default:
                    break;
            }

            string raceTrackCopy = JsonSerializer.Serialize<RoadRaceTracking>(raceTracker);
            RoadRaceTracking newRaceTracker = JsonSerializer.Deserialize<RoadRaceTracking>(raceTrackCopy);

            //await AddLogEntry(CurrentPlayer, GameState, CatanAction.UpdatedRoadState, true, logType, road.Number, new LogRoadUpdate(_gameView.CurrentGame.Index, road, oldState, road.RoadState));
            CalculateAndSetLongestRoad(page, newRaceTracker);

            model.NewRaceTracking = newRaceTracker;

            return model;
        }

        /// <summary>
        ///         this looks at the global state of all the roads and makes sure that it
        ///         1. keeps track of who gets to a road count >= 5 first
        ///         2. makes sure that the right player gets the longest road
        ///         3. works when an Undo action happens
        ///         5. works when a road is "broken"
        /// </summary>
        private static void CalculateAndSetLongestRoad(MainPage page, RoadRaceTracking raceTracking)
        {
            var PlayingPlayers = page.MainPageModel.PlayingPlayers;

            //
            //  make the compiler error go away
            // await Task.Delay(0);

            PlayerModel longestRoadPlayer = null;
            int maxRoads = -1;
            List<PlayerModel> tiedPlayers = new List<PlayerModel>();

            try
            {

                raceTracking.BeginChanges();
                //
                //  first loop over the players and find the set of players that have the longest road
                //
                foreach (PlayerModel p in PlayingPlayers)
                {
                    if (p.GameData.HasLongestRoad)
                    {
                        longestRoadPlayer = p;  // this one currently has the longest road bit -- it may or may not be correct now
                    }
                    // calculate the longest road each player has -- we do this for *every* road/bulding state transition as one person can impact another (e.g. break a road)
                    p.GameData.LongestRoad = page.CalculateLongestRoad(p, p.GameData.RoadsAndShips);

                    //
                    //  remove any tracking for roads greater than their current longest road
                    //  e.g. if they had a road of length 7 and somebody broke it, remove the
                    //  entries that said they had built roads of length 5+
                    for (int i = p.GameData.LongestRoad + 1; i < page.GameContainer.CurrentGame.MaxRoads; i++)
                    {
                        raceTracking.RemovePlayer(p, i);
                    }


                    if (p.GameData.LongestRoad >= 5)
                    {
                        //
                        //  Now we add everybody who has more than 5 rows to the "race" tracking -- 
                        //  this has a Dictionary<int, List> where the list is ordered by road count
                        raceTracking.AddPlayer(p, p.GameData.LongestRoad); // throws away duplicates
                    }
                    if (p.GameData.LongestRoad > maxRoads)
                    {
                        tiedPlayers.Clear();
                        tiedPlayers.Add(p);
                        maxRoads = p.GameData.LongestRoad;
                    }
                    else if (p.GameData.LongestRoad == maxRoads)
                    {
                        tiedPlayers.Add(p);
                    }
                }

                //
                //  somebody had longest road, but they are not tied for max roads - turn off the bit
                if (longestRoadPlayer != null && !tiedPlayers.Contains(longestRoadPlayer))
                {
                    longestRoadPlayer.GameData.HasLongestRoad = false;
                    longestRoadPlayer = null;
                }

                //
                //  can't have longest road if there aren't enough of them
                if (maxRoads < 5) // "5" is a "magic" Catan number - you need at least 5 roads to get Longest Road
                {
                    if (longestRoadPlayer != null)
                    {
                        longestRoadPlayer.GameData.HasLongestRoad = false;
                    }
                    return;
                }

                //
                //  if only one person has longest road
                if (tiedPlayers.Count == 1)
                {

                    tiedPlayers[0].GameData.HasLongestRoad = true;
                    return;
                }

                //
                //  more than one player has it -- give it to the one that has won the tie
                //  first turn it off for everybody...this is needed because somebody might
                //  be tied, but second in the race. they get the next number of roads and then undo it.
                //  we need to give the longest road back to the first player to get to the road count
                foreach (PlayerModel p in tiedPlayers)
                {
                    p.GameData.HasLongestRoad = false;

                }
                //
                //  now turn it on for the winner!
                raceTracking.GetRaceWinner(maxRoads).GameData.HasLongestRoad = true;
            }
            finally
            {
                //
                //  this pattern makes it so we can change race tracking multiple times but only end up with 
                //  one log write
                raceTracking.EndChanges(page.CurrentPlayer, page.GameState, LogType.Normal);
            }

        }

    }


}
