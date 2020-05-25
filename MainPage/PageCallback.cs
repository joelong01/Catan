using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Catan10
{
    public sealed partial class MainPage : Page, IGameCallback, ITileControlCallback, ILogParserHelper
    {
        private int _roadSkipped = 0;

        private PlayerModel MaxRoadPlayer
        {
            get
            {
                foreach (PlayerModel player in MainPageModel.PlayingPlayers)
                {
                    if (player.GameData.HasLongestRoad)
                    {
                        return player;
                    }
                }

                return null;
            }
        }

        public GameContainerCtrl GameContainer
        {
            get
            {
                return _gameView;
            }
        }

        //
        //   when a Knight is played
        //      1. Increment the target's TimesTargetted
        //      2. Move the ship or baron to the target tile
        //      3. set the flag that says a Knight has been played this turn or that the Robber has been moved because of a 7
        //      4. If Knight Played Increment the source player (which is always the current player) Knights played
        //      5. Log that it happened.
        //      6. check to see if we should update the Largest Army
        private async Task AssignBaronOrKnight(PlayerModel targetPlayer, TileCtrl targetTile, TargetWeapon weapon, CatanAction action, LogType logType)
        {
            bool flagState = true;
            int inc = 1;
            if (logType == LogType.Undo)
            {
                //
                //   if this is an undo action, decrement the counter and set the flag to false so the player can do it again
                flagState = false;
                inc = -1;
            }

            if (targetPlayer != null)
            {
                targetPlayer.GameData.TimesTargeted += inc;
            }

            TileCtrl startTile;

            if (weapon == TargetWeapon.PirateShip)
            {
                startTile = _gameView.PirateShipTile;
                _gameView.PirateShipTile = targetTile;
            }
            else
            {
                startTile = _gameView.BaronTile;
                _gameView.BaronTile = targetTile;
            }

            if (action == CatanAction.PlayedKnight)
            {
                CurrentPlayer.GameData.PlayedKnightThisTurn = flagState;
                CurrentPlayer.GameData.KnightsPlayed += inc;
                AssignLargestArmy();
            }
            else
            {
                CurrentPlayer.GameData.MovedBaronAfterRollingSeven = flagState;
            }

            await AddLogEntry(CurrentPlayer, GameStateFromOldLog, action, true, logType, 1, new LogBaronOrPirate(_gameView.CurrentGame.Index, targetPlayer, CurrentPlayer, startTile, targetTile, weapon, action));

            if (GameStateFromOldLog == GameState.MustMoveBaron && logType != LogType.Undo)
            {
                await SetStateAsync(CurrentPlayer, GameState.WaitingForNext, false, logType);
            }

            if (GameStateFromOldLog == GameState.WaitingForRoll)
            {
                if (logType == LogType.Undo)
                {
                    CanMoveBaronBeforeRoll = true;
                }
                else if (logType == LogType.Normal)
                {
                    //
                    //  we assigned the baron before rolling --

                    Debug.Assert(this.CanMoveBaronBeforeRoll, "To hit this condition, the Baron Button needed to be checked");
                    CanMoveBaronBeforeRoll = false;
                }
            }
        }

        //
        //  if the current player ends up with more than 3 knights, see if they have the largest army by looking
        //  at everybody and check to see who has the most knights played.
        //
        //  since this is called from Undo, you have to set it to false if it is less than 2 in case you undid the one that made you the larget army
        //
        private void AssignLargestArmy()
        {
            if (CurrentPlayer.GameData.KnightsPlayed > 2)
            {
                //
                //  loop through and find who has largest army, and how many knights they've played
                int knightCount = 0;
                PlayerModel largestArmyPlayer = null;
                foreach (PlayerModel p in MainPageModel.PlayingPlayers)
                {
                    if (p.GameData.LargestArmy)
                    {
                        largestArmyPlayer = p;
                        knightCount = p.GameData.KnightsPlayed;
                    }
                }

                if (CurrentPlayer.GameData.KnightsPlayed > knightCount)
                {
                    if (largestArmyPlayer != null)
                    {
                        largestArmyPlayer.GameData.LargestArmy = false;
                    }

                    CurrentPlayer.GameData.LargestArmy = true;
                }
            }
            else
            {
                CurrentPlayer.GameData.LargestArmy = false;
            }
        }

        /// <summary>
        ///         this looks at the global state of all the roads and makes sure that it
        ///         1. keeps track of who gets to a road count >= 5 first
        ///         2. makes sure that the right player gets the longest road
        ///         3. works when an Undo action happens
        ///         5. works when a road is "broken"
        /// </summary>
        private void CalculateAndSetLongestRoad(RoadRaceTracking raceTracking)
        {
            var PlayingPlayers = MainPageModel.PlayingPlayers;

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
                    p.GameData.LongestRoad = CalculateLongestRoad(p, p.GameData.RoadsAndShips);

                    //
                    //  remove any tracking for roads greater than their current longest road
                    //  e.g. if they had a road of length 7 and somebody broke it, remove the
                    //  entries that said they had built roads of length 5+
                    for (int i = p.GameData.LongestRoad + 1; i < GameContainer.CurrentGame.MaxRoads; i++)
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
                raceTracking.EndChanges(CurrentPlayer, GameStateFromOldLog);
            }
        }

        //
        //  Start is just any old road you want to start counting from
        //  counted are all the roads that have been counted so far -- presumably starts with .Count = 0
        //  blockedFork roads is set when we recurse so that we can pick a direction.  we need it in case of closed loops
        private int CalculateLongestRoad(RoadCtrl start, List<RoadCtrl> counted, RoadCtrl blockedFork)
        {
            int count = 1;
            int max = 1;
            counted.Add(start); // it is counted in the "max=1" above
            RoadCtrl next = start;
            List<RoadCtrl> ownedAdjacentNotCounted = null;  // the list of adjacent roads owned by the current player that haven't been counted yet
            ownedAdjacentNotCounted = next.OwnedAdjacentRoadsNotCounted(counted, blockedFork, out bool adjacentFork);
            do
            {
                switch (ownedAdjacentNotCounted.Count)
                {
                    case 0:
                        return max;

                    case 1:
                        {
                            count++;
                            next = ownedAdjacentNotCounted[0];
                            counted.Add(next);                  // we counted it, add it to the counted list.

                            if (count > max)
                            {
                                max = count;
                            }

                            ownedAdjacentNotCounted = next.OwnedAdjacentRoadsNotCounted(counted, blockedFork, out adjacentFork);
                            if (adjacentFork)
                            {
                                //ah...the loop
                                count++;
                                counted.Add(next); // we shouldn't have to do this more than once
                                if (count > max)
                                {
                                    max = count;
                                }

                                return max;
                            }
                        }
                        //
                        //  loop to the next road to see if it terminates, forks, or just continues...
                        break;

                    default:

                        //
                        //   general strategy:  for each fork in the road, pretend that all but one of the forks are already counted
                        //                      then count the remaining one.  after that, pick another to be counted
                        //                      because we "count" the entered line, there are only ever 2 forks in the road

                        // ownedAdjacentNotCounted.Count > 1
                        //  usually there means there is a fork like this

                        //                           /
                        //                          /    <=== fork1
                        //                         /
                        //                  ------     <=== always counted
                        //                         \
                        //                          \   <=== Fork 2
                        //                           \

                        //  if we ever get this or the equivalent:
                        //
                        //                           /
                        //                          /    <=== fork1
                        //                         /
                        //                  ------     <=== always counted
                        //                /        \
                        //   Fork 3 -->  /          \   <=== Fork 2
                        //              /            \
                        //
                        //  e.g the adjacent count is > 2 then the road with all the forks around it (the horizontal in ascii art) doesn't have to be counted because we'll count all the
                        //  roads coming into that fork

                        List<RoadCtrl> forks = new List<RoadCtrl>();
                        forks.AddRange(ownedAdjacentNotCounted);
                        if (forks.Count > 2)
                        {
                            //
                            //  if the fork count is not 2 then that means we are in a middle segment, and we don't need to start there
                            _roadSkipped++;
                            return max;
                        }
                        foreach (RoadCtrl road in ownedAdjacentNotCounted)
                        {
                            forks.Remove(road);// now the list has everything except this one road...so we've effectively picked a direction
                            int forkCount = CalculateLongestRoad(road, counted, forks[0]); // --> only one element in the forks list at this point

                            if (count + forkCount > max)
                            {
                                max = count + forkCount;
                            }

                            forks.Add(road); // put fork back so we can count that fork
                        }

                        return max;
                }
            } while (ownedAdjacentNotCounted.Count != 0);

            return max;
        }

        //
        //   when a user clicks on a Road, return the next state for it
        //
        //  7/6/2018:  Made a change so that you don't cycle the states - instead
        //             of going back to Unowned when you click on a Road (or a ship)
        //             it just stays where it is.  if you want to go back to Unowned,
        //             use Undo. This makes the semantics of figuring out longest road
        //             easier since I don't have to figure out if you landed on Unowned
        //             because of an implicit Undo (e.g. if we've logged a state change
        //             in a road, and RoadState == UnOwned then LogType must be LogType.Undo)
        //
        private RoadState NextRoadState(RoadCtrl road)
        {
            bool nextToSea = false;
            if (road.Keys.Count == 1 && _gameView.CurrentGame.MaxShips > 0)
            {
                // this means we are on an outer edge
                nextToSea = true;
            }
            else
            {
                foreach (RoadKey key in road.Keys)
                {
                    if (key.Tile.ResourceType == ResourceType.Sea)
                    {
                        nextToSea = true;
                        break;
                    }
                }
            }
            RoadState nextState = RoadState.Unowned;
            switch (road.RoadState)
            {
                case RoadState.Unowned:
                    nextState = RoadState.Road;
                    break;

                case RoadState.Road:
                    if (nextToSea)
                    {
                        nextState = RoadState.Ship;
                    }
                    else
                    {
                        nextState = RoadState.Road;
                    }

                    break;

                case RoadState.Ship:
                    nextState = RoadState.Ship;
                    break;

                default:
                    break;
            }

            return nextState;
        }

        private bool RoadAllowed(RoadCtrl road)
        {
            if (!ValidateBuilding)
            {
                return true;
            }

            if (TheHuman.PlayerIdentifier != CurrentPlayer.PlayerIdentifier) return false; // can't build a road if it isn't your turn

            //
            //  is there an adjacent Road

            foreach (RoadCtrl adjacentRoad in road.AdjacentRoads)
            {
                //  this.TraceMessage("You updated this -- if roads don't work,  look here");
                //if (adjacentRoad.Color == road.Color && adjacentRoad.RoadState != RoadState.Unowned)
                //{
                //    return true;
                //}
                if (adjacentRoad.Owner == CurrentPlayer && adjacentRoad.RoadState != RoadState.Unowned)
                {
                    return true;
                }
            }

            //
            //   is it next to a Settlement
            foreach (BuildingCtrl s in road.AdjacentBuildings)
            {
                if (s.Owner == CurrentPlayer)
                {
                    return true;
                }
            }

            return false;
        }

        //
        //  location is where you want to build something
        //  Catan doesn't allow settlemnts next to each other
        //  during the intial phase of the game (GameState.AllocateResourceForward and GameState.AllocateResourceReverse) you can place them without being adjacent to a road
        //  but after that, a road you must have!
        //
        //  to build you want this to return FALSE
        private bool SettlementsWithinOneSpace(BuildingCtrl settlement)
        {
            foreach (BuildingCtrl adjacent in settlement.AdjacentBuildings)
            {
                if (adjacent.BuildingState == BuildingState.City || adjacent.BuildingState == BuildingState.Settlement)
                {
                    return true;
                }
            }

            return false;
        }

        private async Task UndoMoveBaron(LogEntry logLine)
        {
            LogBaronOrPirate undoObject = logLine.Tag as LogBaronOrPirate;
            await AssignBaronOrKnight(undoObject.TargetPlayer, undoObject.StartTile, undoObject.TargetWeapon, logLine.Action, LogType.Undo);
        }

        private void UpdateTileBuildingOwner(PlayerModel player, BuildingCtrl building, BuildingState newState, BuildingState oldState)
        {
            foreach (BuildingKey key in building.Clones)
            {
                if (key.Tile.ResourceType == ResourceType.Sea)
                {
                    continue;
                }

                if (newState == BuildingState.None)
                {
                    // tell the tile that this settlement is no longer owned
                    key.Tile.OwnedBuilding.Remove(building);
                    if (_gameView.HasIslands)
                    {
                        Island island = _gameView.GetIsland(key.Tile);
                        if (island != null)
                        {
                            if (island.BonusPoint)
                            {
                                player?.GameData.RemoveIsland(island);
                            }
                        }
                    }
                }
                else
                {
                    // tell the tile that this settlement is owned
                    if (key.Tile.OwnedBuilding.Contains(building) == false)
                    {
                        key.Tile.OwnedBuilding.Add(building);
                    }
                    if (_gameView.HasIslands)
                    {
                        Island island = _gameView.GetIsland(key.Tile);
                        if (island != null)
                        {
                            if (island.BonusPoint && oldState == BuildingState.None) // only addref when you go from none
                            {
                                player?.GameData.AddIsland(island);
                            }
                        }
                    }
                }
            }
        }

        private void UpdateTurnFlag()
        {
            foreach (PlayerModel pd in MainPageModel.PlayingPlayers)
            {
                pd.GameData.IsCurrentPlayer = false; // views with Timers should turn off
            }

            CurrentPlayer.GameData.IsCurrentPlayer = true; // this should start the timer for this view
        }

        internal PlayerModel PlayerNameToPlayer(string name, ICollection<PlayerModel> players)
        {
            foreach (var player in players)
            {
                if (player.PlayerName == name)
                    return player;
            }
            throw new Exception("bad name passed PlayerNameToPlayer");
        }

        /// <summary>
        ///     called after the settlement status has been updated.  the PlayerData has already been fixed to represent the new state
        ///     the Views bind directly to the PlayerData, so we don't do anything with the Score (or anything else with PlayerData)
        ///     This View knows how to Log and about the other Buildings and Roads, so put anything in here that is impacted by building something (or "unbuilding" it)
        ///     in this case, recalc the longest road (a buidling can "break" a road) and then log it.
        ///     we also clear all the Pip ellipses if we are in the allocating phase
        /// </summary>
        public async Task BuildingStateChanged(PlayerModel player, BuildingCtrl building, BuildingState oldState)
        {
            if (building.BuildingState != BuildingState.Pips && building.BuildingState != BuildingState.None) // but NOT if if is transitioning to the Pips state - only happens from the Menu "Show Highest Pip Count"
            {
                await HideAllPipEllipses();
                _showPipGroupIndex = 0;
            }

            if (CurrentGameState == GameState.AllocateResourceReverse)
            {
                if (building.BuildingState == BuildingState.Settlement && (oldState == BuildingState.None || oldState == BuildingState.Pips))
                {
                    TradeResources tr = new TradeResources();
                    foreach (var kvp in building.BuildingToTileDictionary)
                    {
                        tr.Add(kvp.Value.ResourceType, 1);
                    }
                    CurrentPlayer.GameData.Resources.GrantResources(tr);
                }
                else if ((building.BuildingState == BuildingState.None) && (oldState == BuildingState.Settlement))
                {
                    //
                    //  user did an undo
                    TradeResources tr = new TradeResources();
                    foreach (var kvp in building.BuildingToTileDictionary)
                    {
                        tr.Add(kvp.Value.ResourceType, -1);
                    }
                    CurrentPlayer.GameData.Resources.GrantResources(tr);
                }
            }

            //
            //  NOTE:  these have to be called in this order so that the undo works correctly
            //  await AddLogEntry(CurrentPlayer, GameStateFromOldLog, CatanAction.UpdateBuildingState, true, logType, building.Index, new LogBuildingUpdate(_gameView.CurrentGame.Index, null, building, oldState, building.BuildingState));
            UpdateTileBuildingOwner(player, building, building.BuildingState, oldState);
            CalculateAndSetLongestRoad();
        }

        /// <summary>
        ///     called by the BuildingCtrl during PointerPressed to see if it is ok to change the state of the building.
        ///     we can only do that if the state is WaitingForNext and the CurrentPlayer == the owner of the building
        /// </summary>
        /// <returns></returns>
        public bool BuildingStateChangeOk(BuildingCtrl building)
        {
            if (!ValidateBuilding) return true;
            if (CurrentPlayer.PlayerIdentifier != TheHuman.PlayerIdentifier) return false;
            Contract.Assert(CurrentPlayer == TheHuman);

            if (building.Owner != null)
            {
                if (building.Owner != CurrentPlayer) // you can only click on your own stuff and when it is your turn
                {
                    return false;
                }
                else
                {
                    //
                    //  they clicked on one of their own cities to upgrade it -- make sure they have a City to go to
                    if (building.BuildingState == BuildingState.Settlement)
                    {
                        if (CurrentPlayer.GameData.Resources.UnspentEntitlements.Contains(Entitlement.City))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            if (GameStateFromOldLog == GameState.AllocateResourceForward || GameStateFromOldLog == GameState.AllocateResourceReverse || GameStateFromOldLog == GameState.Supplemental || GameStateFromOldLog == GameState.WaitingForNext)
            {
                if (building.BuildingState == BuildingState.Settlement)
                {
                    if (CurrentPlayer.GameData.Resources.UnspentEntitlements.Contains(Entitlement.City))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                if (building.BuildingState == BuildingState.Build || building.BuildingState == BuildingState.Pips) // would be NoEntitlement if they didn't have the entitlement
                {
                    Contract.Assert(CurrentPlayer.GameData.Resources.HasEntitlement(Entitlement.Settlement));
                    return true;
                }
            }
            if (building.BuildingState == BuildingState.NoEntitlement) return false;
            return false;
        }

        /// <summary>
        ///         this looks at the global state of all the roads and makes sure that it
        ///         1. keeps track of who gets to a road count >= 5 first
        ///         2. makes sure that the right player gets the longest road
        ///         3. works when an Undo action happens
        ///         5. works when a road is "broken"
        /// </summary>
        public void CalculateAndSetLongestRoad()

        {
            //
            //  make the compiler error go away
            // await Task.Delay(0);

            PlayerModel longestRoadPlayer = null;
            int maxRoads = -1;
            List<PlayerModel> tiedPlayers = new List<PlayerModel>();

            try
            {
                _raceTracking.BeginChanges();
                //
                //  first loop over the players and find the set of players that have the longest road
                //
                foreach (PlayerModel p in MainPageModel.PlayingPlayers)
                {
                    if (p.GameData.HasLongestRoad)
                    {
                        longestRoadPlayer = p;  // this one currently has the longest road bit -- it may or may not be correct now
                    }
                    // calculate the longest road each player has -- we do this for *every* road/bulding state transition as one person can impact another (e.g. break a road)
                    p.GameData.LongestRoad = CalculateLongestRoad(p, p.GameData.RoadsAndShips);

                    //
                    //  remove any tracking for roads greater than their current longest road
                    //  e.g. if they had a road of length 7 and somebody broke it, remove the
                    //  entries that said they had built roads of length 5+
                    for (int i = p.GameData.LongestRoad + 1; i < _gameView.CurrentGame.MaxRoads; i++)
                    {
                        _raceTracking.RemovePlayer(p, i);
                    }

                    if (p.GameData.LongestRoad >= 5)
                    {
                        //
                        //  Now we add everybody who has more than 5 rows to the "race" tracking --
                        //  this has a Dictionary<int, List> where the list is ordered by road count
                        _raceTracking.AddPlayer(p, p.GameData.LongestRoad); // throws away duplicates
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
                _raceTracking.GetRaceWinner(maxRoads).GameData.HasLongestRoad = true;
            }
            finally
            {
                //
                //  this pattern makes it so we can change race tracking multiple times but only end up with
                //  one log write
                _raceTracking.EndChanges(CurrentPlayer, this.GameStateFromOldLog);
            }
        }

        //
        //  loop through all the players roads calculating the longest road from that point and then return the max found
        public int CalculateLongestRoad(PlayerModel player, ObservableCollection<RoadCtrl> roads)
        {
            int max = 0;
            RoadCtrl maxRoadStartedAt = null;
            foreach (RoadCtrl startRoad in roads)
            {
                {
                    int count = CalculateLongestRoad(startRoad, new List<RoadCtrl>(), null);
                    if (count > max)
                    {
                        max = count;
                        maxRoadStartedAt = startRoad;
                        if (max == player.GameData.Roads.Count) // the most roads you can have…only count once
                        {
                            break;
                        }
                    }
                }
            }
            return max;
        }

        public bool CanBuildRoad()

        {
            if (ValidateBuilding == false) return true;
            if (MainPageModel.Log == null)
            {
                return false;
            }

            if (CurrentPlayer != TheHuman) return false;

            if (!CurrentPlayer.GameData.Resources.HasEntitlement(Entitlement.Road))
            {
                return false;
            }

            GameState state = CurrentGameState;

            if (state == GameState.WaitingForNext || // I can build after I roll
                state == GameState.AllocateResourceForward || // I can build during the initial phase )
                state == GameState.AllocateResourceReverse || state == GameState.Supplemental ||
                state == GameState.PickingBoard)
            {
                return true;
            }

            return false;
        }

        public async Task ChangeGame(CatanGameCtrl game)
        {
            if (game == _gameView.CurrentGame)
            {
                return;
            }

            if (CurrentGameState == GameState.WaitingForNewGame)
            {
                _gameView.CurrentGame = game;

                return;
            }
            if (await StaticHelpers.AskUserYesNoQuestion("Are you sure you want to change the game?  The board will reshuffle and you won't be able to get back to this game.", "Yes", "No"))
            {
                _gameView.CurrentGame = game;

                await VisualShuffle();
            }
        }

        public async Task CurrentPlayerChanged()
        {
            //
            //  the next player can always play a baron once
            CurrentPlayer.GameData.PlayedKnightThisTurn = false;
            CurrentPlayer.GameData.MovedBaronAfterRollingSeven = null;

            UpdateTurnFlag();

            _stopWatchForTurn.TotalTime = TimeSpan.FromSeconds(0);
            _stopWatchForTurn.StartTimer();

            if (GameStateFromOldLog == GameState.AllocateResourceForward || GameStateFromOldLog == GameState.AllocateResourceReverse)
            {
                await HideAllPipEllipses();
                _showPipGroupIndex = 0;
            }

            // tell all the Buildings that the CurrentPlayer has changed
            foreach (BuildingCtrl building in _gameView.AllBuildings)
            {
                building.CurrentPlayer = CurrentPlayer;
            }
        }

        public BuildingCtrl GetBuilding(int settlementIndex)
        {
            return _gameView.GetBuilding(settlementIndex);
        }

        public PlayerModel GetPlayerData(int playerIndex)
        {
            return MainPageModel.AllPlayers[playerIndex];
        }

        public RoadCtrl GetRoad(int roadIndex)
        {
            return _gameView.GetRoad(roadIndex);
        }

        public TileCtrl GetTile(int tileIndex)
        {
            return _gameView.GetTile(tileIndex);
        }

        public Task OnNewGame()
        {
            return Task.CompletedTask;
            //if (MainPageModel.Log != null && MainPageModel.Log.ActionCount != 0)
            //{
            //    if (State.GameState != GameState.WaitingForNewGame)
            //    {
            //        if (await StaticHelpers.AskUserYesNoQuestion("Start a new game?", "Yes", "No") == false)
            //        {
            //            return;
            //        }
            //    }
            //}

            //try
            //{
            //    if (MainPageModel.AllPlayers.Count == 0)
            //    {
            //        await LoadGameData();
            //    }

            //    Debug.Assert(MainPageModel.AllPlayers.Count > 0);

            //    NewGameDlg dlg = new NewGameDlg(MainPageModel.AllPlayers, _gameView.Games);

            //    ContentDialogResult result = await dlg.ShowAsync();
            //    if ((dlg.PlayingPlayers.Count < 3 || dlg.PlayingPlayers.Count > 6) && result == ContentDialogResult.Primary)
            //    {
            //        string content = String.Format($"You must pick at least 3 players and no more than 6 to play the game.");
            //        MessageDialog msgDlg = new MessageDialog(content);
            //        await msgDlg.ShowAsync();
            //        return;
            //    }

            //    if (dlg.SelectedGame == null)
            //    {
            //        string content = String.Format($"Pick a game!!");
            //        MessageDialog msgDlg = new MessageDialog(content);
            //        await msgDlg.ShowAsync();
            //        return;
            //    }

            //    if (result != ContentDialogResult.Secondary)
            //    {
            //        _gameView.Reset();
            //        await this.Reset();
            //        await MainPageModel.Log.Init(dlg.SaveFileName);
            //        await SetStateAsync(null, GameState.WaitingForNewGame, true);
            //        _gameView.CurrentGame = dlg.SelectedGame;

            //        SavedGames.Insert(0, MainPageModel.Log);
            //        await AddLogEntry(null, GameState.GamePicked, CatanAction.SelectGame, true, LogType.Normal, dlg.SelectedIndex);
            //        await StartGame(dlg.PlayingPlayers, dlg.SelectedIndex);
            //    }

            //}
            //finally
            //{
            //    VerifyRoundTrip<MainPageModel>(MainPageModel);
            //}
        }

        public void RoadEntered(RoadCtrl road, PointerRoutedEventArgs e)
        {
            if (!CanBuildRoad())
            {
                return;
            }

            if (road.IsOwned)
            {
                return;
            }

            road.CurrentPlayer = CurrentPlayer;

            road.Show(true, RoadAllowed(road));

            //
            //   if you forgot, this is good for debugging the layout -- left here in case you change it...again.

            //foreach (var r in road.AdjacentRoads)
            //{
            //    r.Show(true);
            //}
            //foreach (var s in road.AdjacentBuildings)
            //{
            //    s.ShowBuildEllipse();
            //}
        }

        public void RoadExited(RoadCtrl road, PointerRoutedEventArgs e)
        {
            if (!CanBuildRoad())
            {
                return;
            }

            if (road.IsOwned)
            {
                return;
            }

            road.Show(false);

            //
            //  if you forgot, this is good for debugging the layout -- left here in case you change it...again.

            //foreach (var r in road.AdjacentRoads)
            //{
            //    r.Show(false);
            //}
            //foreach (var s in road.AdjacentSettlements)
            //{
            //    s.HideBuildEllipse();
            //}
        }

        public async void RoadPressed(RoadCtrl road, PointerRoutedEventArgs e)
        {
            if (!CanBuildRoad())
            {
                return;
            }

            if (!RoadAllowed(road)) // clicked on a random road away from a settlement or road
            {
                return;
            }

            if (road.IsOwned)
            {
                if (road.Owner != CurrentPlayer) // this is not my road I'm clicking on -- bail
                {
                    return;
                }
            }
            await UpdateRoadLog.SetRoadState(this, road, NextRoadState(road), _raceTracking);
            //
            //  UpdateRoad state will be done in the IGameController
        }

        //
        //  this is where the CurrentPlayer can pick somebody to target for the Baron
        //
        //  it can happen because the user rolled 7 or they played a Knight, or they could do both in the same turn
        //
        //  if they do this before a roll, it is a knight played.
        //  if they do it after a roll, and it is not a 7, it is a baron
        //  if they do it after a roll, and it is a 7, then it is not a baron
        //  if they do it after a roll, and it is a 7, and they have already done it once, then it is a knight.
        //
        //
        public void TileRightTapped(TileCtrl targetTile, RightTappedRoutedEventArgs rte)
        {
            if (GameStateFromOldLog != GameState.MustMoveBaron && GameStateFromOldLog != GameState.WaitingForNext &&
                GameStateFromOldLog != GameState.WaitingForRoll)
            {
                return;
            }

            if ((GameStateFromOldLog == GameState.WaitingForRoll && !this.CanMoveBaronBeforeRoll) && GameStateFromOldLog != GameState.MustMoveBaron)
            {
                Debug.WriteLine("You need to check the baron button before assiging baron");
                return;
            }

            //if (GameState != GameState.WaitingForNext &&
            //    GameState != GameState.WaitingForRoll)
            //    return;

            PlayerGameModel playerGameData = CurrentPlayer.GameData;

            CatanAction action = CatanAction.None;
            TargetWeapon weapon = TargetWeapon.Baron;

            if (playerGameData.MovedBaronAfterRollingSeven != false && playerGameData.PlayedKnightThisTurn) // not eligible to move baron
            {
                return;
            }

            async void Baron_MenuClicked(object s, RoutedEventArgs e)
            {
                PlayerModel player = (PlayerModel)((MenuFlyoutItem)s).Tag;

                await AssignBaronOrKnight(player, targetTile, weapon, action, LogType.Normal);
            }

            _menuBaron.Items.Clear();
            MenuFlyoutItem item = null;
            if (targetTile.ResourceType == ResourceType.Sea) // we move pirate instead of Baron
            { // this means we are moving the pirate ship
                weapon = TargetWeapon.PirateShip;
                List<RoadCtrl> roads = new List<RoadCtrl>();
                foreach (RoadLocation location in Enum.GetValues(typeof(RoadLocation)))
                {
                    if (location == RoadLocation.None)
                    {
                        continue;
                    }

                    RoadCtrl r = _gameView.GetRoadAt(targetTile, location);
                    if (r.IsOwned && r.RoadState == RoadState.Ship)
                    {
                        roads.Add(r);
                    }
                }

                foreach (RoadCtrl road in roads)
                {
                    string s = "";
                    if (playerGameData.MovedBaronAfterRollingSeven == false)
                    {
                        s = String.Format($"Rolled 7. Pirate Target: {road.Owner.PlayerName}");
                        action = CatanAction.RolledSeven;
                    }
                    else if (playerGameData.PlayedKnightThisTurn == false)
                    {
                        s = String.Format($"Knight Played. Pirate Target: {road.Owner.PlayerName}");
                        action = CatanAction.PlayedKnight;
                    }
                    else
                    {
                        return;
                    }

                    bool found = false;
                    foreach (MenuFlyoutItem mnuItem in _menuBaron.Items)
                    {
                        if (mnuItem.Text == s)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        item = new MenuFlyoutItem
                        {
                            Text = s,
                            Tag = road.Owner
                        };
                        item.Click += Baron_MenuClicked;
                        _menuBaron.Items.Add(item);
                    }
                }

                if (roads.Count == 0)
                {
                    if (playerGameData.MovedBaronAfterRollingSeven == false)
                    {
                        action = CatanAction.RolledSeven;
                    }
                    else if (playerGameData.PlayedKnightThisTurn == false)
                    {
                        action = CatanAction.PlayedKnight;
                    }

                    item = new MenuFlyoutItem
                    {
                        Text = "Pirate Targets Nobody (how nice!)"
                    };
                    item.Click += Baron_MenuClicked;
                    _menuBaron.Items.Add(item);
                }
                item = new MenuFlyoutItem
                {
                    Text = "Cancel"
                };
                _menuBaron.Items.Add(item);
                _menuBaron.ShowAt(targetTile, rte.GetPosition(targetTile));
            }
            else
            {
                foreach (BuildingCtrl settlement in targetTile.OwnedBuilding)
                {
                    string s = "";
                    if (playerGameData.MovedBaronAfterRollingSeven == false)
                    {
                        s = String.Format($"Rolled 7. Target: {settlement.Owner.PlayerName}");
                        action = CatanAction.RolledSeven;
                    }
                    else if (playerGameData.PlayedKnightThisTurn == false)
                    {
                        s = String.Format($"Knight Played. Target: {settlement.Owner.PlayerName}");
                        action = CatanAction.PlayedKnight;
                    }
                    else
                    {
                        return;
                    }

                    bool found = false;
                    foreach (MenuFlyoutItem mnuItem in _menuBaron.Items)
                    {
                        if (mnuItem.Text == s)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)  // this is so we only add each person once in case they have multiple settlements on the same tile
                    {
                        item = new MenuFlyoutItem
                        {
                            Text = s,
                            Tag = settlement.Owner
                        };
                        item.Click += Baron_MenuClicked;
                        _menuBaron.Items.Add(item);
                    }
                }

                if (targetTile.OwnedBuilding.Count == 0)
                {
                    if (playerGameData.MovedBaronAfterRollingSeven == false)
                    {
                        action = CatanAction.RolledSeven;
                    }
                    else if (playerGameData.PlayedKnightThisTurn == false)
                    {
                        action = CatanAction.PlayedKnight;
                    }
                    item = new MenuFlyoutItem
                    {
                        Text = "Target Nobody (how nice!)"
                    };
                    item.Click += Baron_MenuClicked;
                    _menuBaron.Items.Add(item);
                }
                item = new MenuFlyoutItem
                {
                    Text = "Cancel"
                };
                _menuBaron.Items.Add(item);
                _menuBaron.ShowAt(targetTile, rte.GetPosition(targetTile));
            }
        }

        //
        //  why put this in a seperate function?  so you can find it with CTL+, w/o having to remember it is because of a PointerPressed event...
        ///
        public Task UpdateRoadState(RoadCtrl road, RoadState oldState, RoadState newState, LogType logType)
        {
            if (newState == oldState)
            {
                return Task.CompletedTask;
            }

            road.RoadState = newState;
            switch (newState)
            {
                case RoadState.Unowned:
                    if (oldState == RoadState.Ship)
                    {
                        CurrentPlayer.GameData.Ships.Remove(road);
                    }
                    else
                    {
                        CurrentPlayer.GameData.Roads.Remove(road);
                    }

                    road.Owner = null;
                    road.Number = -1;
                    break;

                case RoadState.Road:
                    road.Number = CurrentPlayer.GameData.Roads.Count; // undo-able
                    CurrentPlayer.GameData.Roads.Add(road);
                    road.Owner = CurrentPlayer;
                    break;

                case RoadState.Ship:
                    CurrentPlayer.GameData.Roads.Remove(road); // can't be a ship if you aren't a road
                    CurrentPlayer.GameData.Ships.Add(road);
                    break;

                default:
                    break;
            }

            CalculateAndSetLongestRoad();
            return Task.CompletedTask;
        }

        //
        //
        //
        public BuildingState ValidateBuildingLocation(BuildingCtrl building)
        {
            if ((CurrentGameState == GameState.WaitingForNewGame || CurrentGameState == GameState.WaitingForStart) && ValidateBuilding)
            {
                return BuildingState.None;
            }

            if (!ValidateBuilding)
            {
                return BuildingState.Build;
            }

            //if (!CanBuildRoad())
            //{
            //    return BuildingState.None;
            //}

            if (CurrentPlayer == null) // this happens if you move the mouse over the board before a new game is started
            {
                return BuildingState.None;
            }

            if (CurrentGameState == GameState.PickingBoard)
            {
                return BuildingState.Pips;
            }

            bool allocationPhase = false;

            if (CurrentGameState == GameState.AllocateResourceForward || CurrentGameState == GameState.AllocateResourceReverse)
            {
                allocationPhase = true;
            }

            bool error = SettlementsWithinOneSpace(building);

            if (CurrentGameState == GameState.AllocateResourceForward || CurrentGameState == GameState.AllocateResourceReverse)
            {
                if (building.BuildingToTileDictionary.Count > 0)
                {
                    if (_gameView.GetIsland(building.BuildingToTileDictionary.First().Value) != null)
                    {
                        //  we are on an island - you can't build on an island when you are allocating resources
                        return BuildingState.Error;
                    }
                }
            }

            //
            //  make sure that we have at least one buildable tile
            bool buildableTile = false;
            foreach (KeyValuePair<BuildingLocation, TileCtrl> kvp in building.BuildingToTileDictionary)
            {
                if (kvp.Value.ResourceType != ResourceType.Sea)
                {
                    buildableTile = true;
                    break;
                }
            }

            if (!buildableTile)
            {
                return BuildingState.None;
            }

            if (!allocationPhase && error == false)
            {
                error = true;
                //
                //   if the settlement is not next to another settlement and we are not in allocation phase, we have to be next to a road
                foreach (RoadCtrl road in building.AdjacentRoads)
                {
                    if (road.Owner == CurrentPlayer && road.RoadState != RoadState.Unowned)
                    {
                        error = false;
                        break;
                    }
                }
            }

            //
            //  if we get here, we have a valid place to build (or we've bypassed the business logic)...make sure there is an entitlement

            if (error == false)
            {
                if (CurrentPlayer.GameData.Resources.HasEntitlement(Entitlement.Settlement))
                {
                    return BuildingState.Build;
                }
                else
                {
                    return BuildingState.NoEntitlement;
                }
            }

            return BuildingState.Error;
        }
    }
}