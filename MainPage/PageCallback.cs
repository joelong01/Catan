using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Catan10
{
    public sealed partial class MainPage : Page, IGameCallback, ITileControlCallback, ILogParserHelper
    {

        public async Task ChangeGame(CatanGame game)
        {
            if (game == _gameView.CurrentGame)
                return;


            if (State.GameState == GameState.WaitingForNewGame)
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


        //
        //  returns True if it undid something, false if the undo action has no UI affect (e.g. true if the user would think undo happened)
        private async Task<bool> UndoLogLine(LogEntry logLine, bool replayingLog)
        {
            switch (logLine.Action)
            {
                case CatanAction.Rolled:
                    int roll = PopRoll(); // we keep a side list of all rolls...remove this one
                    if (roll != 7)
                    {
                        //
                        //  take the resources away from the players who got resources because of the roll
                        //  

                        List<TileCtrl> tilesWithNumber = new List<TileCtrl>();
                        foreach (TileCtrl t in _gameView.AllTiles)
                        {
                            if (t.Number == roll)
                                tilesWithNumber.Add(t);
                        }

                        CountResourcesForRoll(tilesWithNumber, true);
                        CurrentPlayer.GameData.MovedBaronAfterRollingSeven = null; // it wasn't a 7, so don't set this to true or falce
                    }
                    await AddLogEntry(CurrentPlayer, GameState.WaitingForRoll, CatanAction.Rolled, true, LogType.Undo, roll);
                    UpdateChart();
                    break;

                case CatanAction.CardsLost:
                    LogCardsLost lcl = logLine.Tag as LogCardsLost;
                    _undoingCardLostHack = true;
                    logLine.PlayerData.GameData.CardsLost = lcl.OldVal;
                    _undoingCardLostHack = false;
                    await LogPlayerLostCards(logLine.PlayerData, lcl.NewVal, lcl.OldVal, LogType.Undo); // values swapped                    
                    break;
                case CatanAction.MissedOpportunity:
                case CatanAction.CardsLostToSeven:
                    //UndoAction(i);      // this will remove one or many
                    break;
                case CatanAction.ChangedState:
                    LogStateTranstion lst = logLine.Tag as LogStateTranstion;
                    if (lst.OldState == GameState.WaitingForStart) break;
                    await SetStateAsync(logLine.PlayerData, lst.OldState, true, LogType.Undo);

                    break;
                case CatanAction.ChangedPlayer:
                    LogChangePlayer lcp = logLine.Tag as LogChangePlayer;
                    await AnimateToPlayerIndex(lcp.From, LogType.Undo);
                    break;
                case CatanAction.DoneSupplemental:
                    _supplementalStartIndex = logLine.Number; // we don't stop undoing on this one -- just set the number we need to terminate the loop and continue on
                    await AddLogEntry(CurrentPlayer, GameState, CatanAction.DoneSupplemental, false, LogType.Undo);
                    break;
                case CatanAction.Dealt:
                    await Reshuffle();
                    // don't take out the log line because Reshuffle doesn't log...
                    // put the state back regardless if they really reshuffled or not
                    await SetStateAsync(CurrentPlayer, GameState.WaitingForStart, true);
                    //await ProcessEnter(null, "");
                    break;
                case CatanAction.PlayedKnight:
                case CatanAction.AssignedBaron:
                case CatanAction.AssignedPirateShip:
                case CatanAction.RolledSeven:
                    await UndoMoveBaron(logLine);
                    break;
                case CatanAction.UpdatedRoadState:
                    LogRoadUpdate roadUpdate = logLine.Tag as LogRoadUpdate;
                    if (replayingLog)
                        await UpdateRoadState(roadUpdate.Road, roadUpdate.OldRoadState, roadUpdate.NewRoadState, LogType.Undo);
                    else
                        await UpdateRoadState(roadUpdate.Road, roadUpdate.NewRoadState, roadUpdate.OldRoadState, LogType.Undo);
                    SetLongestRoadFromLog();
                    break;
                case CatanAction.UpdateBuildingState:
                    LogBuildingUpdate buildingUpdate = logLine.Tag as LogBuildingUpdate;
                    if (replayingLog)
                    {
                        await buildingUpdate.Building.UpdateBuildingState(buildingUpdate.OldBuildingState, buildingUpdate.NewBuildingState, LogType.Undo);
                    }
                    else
                    {
                        await buildingUpdate.Building.UpdateBuildingState(buildingUpdate.NewBuildingState, buildingUpdate.OldBuildingState, LogType.Undo); // NOTE:  New and Old have been swapped                      
                    }

                    break;
                default:
                    break;
            }

            return logLine.StopProcessingUndo;


        }

        public async Task OnUndo()
        {
            if (_log.Count < SMALLEST_STATE_COUNT) // the games starts with 5 states that can't be undone
                return;


            try
            {
                for (int i = _log.Count - 1; i >= SMALLEST_STATE_COUNT - 1; i--)
                {
                    if (_log[i].LogType == LogType.Undo) continue; // if we have an undo action, skip it
                    if (_log[i].Undone == true) continue;   // we already undid this one

                    bool ret = await UndoLogLine(_log[i], false);
                    if (ret) break;
                }
            }
            finally
            {
                UpdateChart();
                UpdateUiForState(_log.Last().GameState);
            }
        }

        private void SetLongestRoadFromLog()
        {
            int longestRoad = 0;


            //
            //  find who has the longest road and how long it is
            foreach (PlayerData p in PlayingPlayers)
            {
                if (p.GameData.HasLongestRoad)
                {
                    longestRoad = p.GameData.LongestRoad;
                }
            }

            if (longestRoad == 0) // nobody has longest road
                return;

            //
            //  next see if there are any ties
            List<PlayerData> tiedPlayers = new List<PlayerData>();
            foreach (var p in PlayingPlayers)
            {
                if (p.GameData.LongestRoad == longestRoad)
                {
                    tiedPlayers.Add(p);
                }
            }

            if (tiedPlayers.Count == 1)
                return; // no ties, no work to do.

            //
            //  turn off longest road flag
            foreach (var p in tiedPlayers)
            {
                p.GameData.HasLongestRoad = false;
            }

            //
            //  go through the log to see who got there first
            int[] roadCount = new int[tiedPlayers.Count];

            foreach (LogEntry logline in _log)
            {
                if (logline.Action == CatanAction.UpdatedRoadState)
                {
                    LogRoadUpdate roadUpdate = logline.Tag as LogRoadUpdate;
                    if (roadUpdate.NewRoadState == RoadState.Road && roadUpdate.OldRoadState == RoadState.Unowned)
                    {
                        int idx = tiedPlayers.IndexOf(logline.PlayerData); // not the index of the playerdata...
                        if (idx == -1) continue; // a player other than the tied player has a log entry
                        roadCount[idx]++;
                        if (roadCount[idx] == longestRoad)
                        {
                            tiedPlayers[idx].GameData.HasLongestRoad = true;
                            return;
                        }
                    }
                    if (roadUpdate.NewRoadState == RoadState.Unowned)
                    {
                        int idx = tiedPlayers.IndexOf(logline.PlayerData);
                        if (idx != -1)
                        {
                            roadCount[idx]--;
                        }

                    }

                }
            }


        }

        private async Task UndoMoveBaron(LogEntry logLine)
        {

            LogBaronOrPirate undoObject = logLine.Tag as LogBaronOrPirate;
            await AssignBaronOrKnight(undoObject.TargetPlayer, undoObject.StartTile, undoObject.TargetWeapon, logLine.Action, LogType.Undo);


        }

        public async Task OnNewGame()
        {
            if (_log != null && _log.Count != 0)
            {
                if (State.GameState != GameState.WaitingForNewGame)
                {
                    if (await StaticHelpers.AskUserYesNoQuestion("Start a new game?", "Yes", "No") == false)
                        return;


                }
            }

            try
            {

                _gameView.Reset();


                if (AllPlayers.Count == 0)
                {
                    await AddDefaultUsers();
                    await LoadPlayerData();
                }
                NewGameDlg dlg = new NewGameDlg(AllPlayers, _gameView.Games);

                ContentDialogResult result = await dlg.ShowAsync();
                if ((dlg.GamePlayers.Count < 3 || dlg.GamePlayers.Count > 6) && result == ContentDialogResult.Primary)
                {
                    string content = String.Format($"You must pick at least 3 players and no more than 6 to play the game.");
                    MessageDialog msgDlg = new MessageDialog(content);
                    await msgDlg.ShowAsync();
                    return;
                }

                if (dlg.SelectedGame == null)
                {
                    string content = String.Format($"Pick a game!!");
                    MessageDialog msgDlg = new MessageDialog(content);
                    await msgDlg.ShowAsync();
                    return;
                }

                if (result != ContentDialogResult.Secondary)
                {
                    await this.Reset();
                    await SetStateAsync(null, GameState.WaitingForNewGame, true);
                    _gameView.CurrentGame = dlg.SelectedGame;

                    _log = new Log();
                    await _log.Init(dlg.SaveFileName);
                    SavedGames.Insert(0, _log);
                    await AddLogEntry(null, GameState.GamePicked, CatanAction.SelectGame, true, LogType.Normal, dlg.SelectedIndex);
                    await StartGame(dlg.PlayerDataList);
                }
            }
            finally
            {

            }
        }

        private async Task PromptForLostCards(PlayerData targetedPlayer, CatanAction action)
        {
            await Task.Delay(0);
            throw new NotImplementedException();
            //GameState currentState = _log.Last().GameState;
            //CatanPlayer ActivePlayerView = CurrentPlayer;
            //await AnimateToPlayer(targetedPlayer, currentState);

            //string input = await _gameTracker.ShowAndWait("Cards Lost", "0", true);
            //int cardsLost = 0;
            ////
            ////  if this fails, then we'll just move to the next player                
            //if (Int32.TryParse(input, out cardsLost))
            //{
            //    CurrentPlayer.SetCardCountForAction(action, cardsLost);
            //    AddLogEntry(CurrentPlayer, currentState, action, cardsLost);
            //    await OnSave();
            //}

            //await AnimateToPlayer(ActivePlayerView, currentState, false);
            //_gameTracker.SetState(currentState);

        }

        private Task DoIteration(bool skipActivePlayerView, CatanAction action)
        {

            //GameState lastState = _log.Last().GameState;
            //List<PlayerView> iterPlayers = new List<PlayerView>();
            //int i = 0;
            //if (skipActivePlayerView)
            //{
            //    await OnNext(1, LogType.Undo);
            //    i = 1;
            //}
            //while (i < PlayingPlayers.Count)
            //{
            //    string input = await ShowAndWait("Cards Lost", "0");
            //    int cardsLost = 0;
            //    //
            //    //  if this fails, then we'll just move to the next player                
            //    if (Int32.TryParse(input, out cardsLost))
            //    {
            //        CurrentPlayer.SetCardCountForAction(action, cardsLost);
            //        await AddLogEntry(CurrentPlayer, lastState, action, cardsLost);
            //    }

            //    await OnNext(1, LogType.Undo); // don't log - we dont undo the movement, just the counting
            //    i++;

            //}
            //
            //  put us back where we started            
            /// await SetStateAsync(CurrentPlayer, lastState, CatanAction.ChangedState, true);
            /// 

            throw new NotImplementedException();
        }


        /// <summary>
        ///     Somebody picked a menu item to change the color
        /// </summary>
        /// <param name="player"></param>
        public void CurrentPlayerColorChanged(PlayerData player)
        {
            foreach (RoadCtrl road in player.GameData.Roads)
            {
                road.Color = player.GameData.PlayerColor;

            }

            //
            //  TODO: delete this comment.  I don't think it is needed after we used databinding for the colors.

            //foreach (BuildingCtrl buildings in player.GameData.Buildings)
            //{
            //    buildings.Color = player.Background;

            //}

        }


        public bool CanBuild()
        {
            if (_log == null) return false;
            if (_log.Count == 0) return false;

            GameState state = State.GameState;

            if (state == GameState.WaitingForNext || // I can build after I roll               
                state == GameState.AllocateResourceForward || // I can build during the initial phase )
                state == GameState.AllocateResourceReverse || state == GameState.Supplemental)
                return true;

            return false;
        }


        //
        //   when a Knight is played
        //      1. Increment the target's TimesTargetted
        //      2. Move the ship or baron to the target tile
        //      3. set the flag that says a Knight has been played this turn or that the Robber has been moved because of a 7
        //      4. If Knight Played Increment the source player (which is always the current player) Knights played
        //      5. Log that it happened.
        //      6. check to see if we should update the Largest Army
        private async Task AssignBaronOrKnight(PlayerData targetPlayer, TileCtrl targetTile, TargetWeapon weapon, CatanAction action, LogType logType)
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

            TileCtrl startTile = null;

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


            await AddLogEntry(CurrentPlayer, GameState, action, true, logType, 1, new LogBaronOrPirate(_gameView.CurrentGame.Index, targetPlayer, CurrentPlayer, startTile, targetTile, weapon, action));

            if (GameState == GameState.MustMoveBaron && logType != LogType.Undo)
            {
                await SetStateAsync(CurrentPlayer, GameState.WaitingForNext, false, logType);
            }


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
            if (GameState != GameState.MustMoveBaron && GameState != GameState.WaitingForNext &&
                GameState != GameState.WaitingForRoll)
                return;

            //if (GameState != GameState.WaitingForNext &&
            //    GameState != GameState.WaitingForRoll)
            //    return;

            PlayerGameData playerGameData = CurrentPlayer.GameData;


            CatanAction action = CatanAction.None;
            TargetWeapon weapon = TargetWeapon.Baron;

            if (playerGameData.MovedBaronAfterRollingSeven != false && playerGameData.PlayedKnightThisTurn) // not eligible to move baron
                return;

            async void Baron_MenuClicked(object s, RoutedEventArgs e)
            {
                PlayerData player = (PlayerData)((MenuFlyoutItem)s).Tag;

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
                    if (location == RoadLocation.None) continue;
                    RoadCtrl r = _gameView.GetRoadAt(targetTile, location);
                    if (r.IsOwned && r.RoadState == RoadState.Ship)
                    {
                        roads.Add(r);
                    }
                }

                foreach (var road in roads)
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
                        return;

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
                        return;

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
                PlayerData largestArmyPlayer = null;
                foreach (var p in PlayingPlayers)
                {
                    if (p.GameData.LargestArmy)
                    {
                        largestArmyPlayer = p;
                        knightCount = p.GameData.KnightsPlayed;
                    }
                }

                if (CurrentPlayer.GameData.KnightsPlayed > knightCount)
                {
                    if (largestArmyPlayer != null) largestArmyPlayer.GameData.LargestArmy = false;
                    CurrentPlayer.GameData.LargestArmy = true;
                }
            }
            else
            {
                CurrentPlayer.GameData.LargestArmy = false;
            }
        }



        //
        //   when a user clicks on a Road, return the next state for it
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
                        nextState = RoadState.Ship;
                    else
                        nextState = RoadState.Unowned;
                    break;
                case RoadState.Ship:
                    nextState = RoadState.Unowned;
                    break;
                default:
                    break;
            }

            return nextState;
        }



        //
        //  why put this in a seperate function?  so you can find it with CTL+, w/o having to remember it is because of a PointerPressed event...
        ///
        private async Task UpdateRoadState(RoadCtrl road, RoadState oldState, RoadState newState, LogType logType)
        {
            road.RoadState = newState;
            switch (newState)
            {
                case RoadState.Unowned:
                    if (oldState == RoadState.Ship)
                        CurrentPlayer.GameData.Ships.Remove(road);
                    else
                        CurrentPlayer.GameData.Roads.Remove(road);

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



            await AddLogEntry(CurrentPlayer, GameState, CatanAction.UpdatedRoadState, true, logType, road.Number, new LogRoadUpdate(_gameView.CurrentGame.Index, road, oldState, road.RoadState));

            ObservableCollection<RoadCtrl> roads = new ObservableCollection<RoadCtrl>();
            roads.AddRange(CurrentPlayer.GameData.Roads);
            roads.AddRange(CurrentPlayer.GameData.Ships);

            CalculateAndSetLongestRoad(CurrentPlayer, roads);
            if (road.RoadState == RoadState.Unowned)
            {
                SetLongestRoadFromLog();
            }

        }

        private PlayerData MaxRoadPlayer
        {
            get
            {
                foreach (var player in PlayingPlayers)
                {
                    if (player.GameData.HasLongestRoad)
                        return player;
                }

                return null;
            }
        }



        private void CalculateAndSetLongestRoad(PlayerData pData, ObservableCollection<RoadCtrl> roads)
        {
            int longestRoad = CalculateLongestRoad(pData, roads);
            pData.GameData.LongestRoad = longestRoad;
            PlayerData maxRoadPlayer = this.MaxRoadPlayer;
            int maxRoads = 4;
            if (maxRoadPlayer != null)
            {
                maxRoads = maxRoadPlayer.GameData.LongestRoad;
                if (maxRoads < 5)
                {
                    maxRoadPlayer.GameData.HasLongestRoad = false; // happens if you undo the 5th road
                }
            }
            foreach (PlayerData player in PlayingPlayers)
            {

                if (player.GameData.LongestRoad > maxRoads) // ">" becuase it preserves ties
                {
                    if (maxRoadPlayer != null)
                    {
                        maxRoadPlayer.GameData.HasLongestRoad = false;
                    }

                    maxRoadPlayer = player;
                    maxRoads = player.GameData.LongestRoad;
                    maxRoadPlayer.GameData.HasLongestRoad = true;
                }


            }



        }



        private bool RoadAllowed(RoadCtrl road)
        {
            if (!ValidateBuilding) return true;

            //
            //  is there an adjacent Road

            foreach (RoadCtrl adjacentRoad in road.AdjacentRoads)
            {
                if (adjacentRoad.Color == road.Color && adjacentRoad.RoadState != RoadState.Unowned)
                {
                    return true;
                }
            }

            //
            //   is it next to a Settlement
            foreach (var s in road.AdjacentBuildings)
            {
                if (s.Owner == CurrentPlayer)
                    return true;
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
            foreach (var adjacent in settlement.AdjacentBuildings)
            {
                if (adjacent.BuildingState == BuildingState.City || adjacent.BuildingState == BuildingState.Settlement)
                {
                    return true;
                }
            }

            return false;
        }


        private void UpdateTileBuildingOwner(PlayerData player, BuildingCtrl building, BuildingState newState, BuildingState oldState)
        {

            foreach (var key in building.Clones)
            {
                if (key.Tile.ResourceType == ResourceType.Sea) continue;

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



        private void RecalcLongestRoadAfterBuildingChanges(TileCtrl tileCtrl, BuildingCtrl settlementCtrl, PlayerData player)
        {

            //
            //   turn off longest road...they'll get it back if they deserve it
            foreach (var pView in PlayingPlayers)
            {
                if (pView.GameData.HasLongestRoad)
                {
                    pView.GameData.HasLongestRoad = false;
                }
            }
            //
            //  do recalc
            foreach (PlayerData pData in PlayingPlayers)
            {
                ObservableCollection<RoadCtrl> roads = new ObservableCollection<RoadCtrl>();
                roads.AddRange(pData.GameData.Roads);
                roads.AddRange(pData.GameData.Ships);
                CalculateAndSetLongestRoad(pData, roads);
            }

        }

        int _roadSkipped = 0;
        //
        //  loop through all the players roads calculating the longest road from that point and then return the max found
        private int CalculateLongestRoad(PlayerData player, ObservableCollection<RoadCtrl> roads)
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
                            break;
                    }
                }
            }
            // this.TraceMessage($"RoadsSkipped:{_roadSkipped} count: {max} Road:{maxRoadStartedAt}");
            return max;

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
                                max = count;

                            ownedAdjacentNotCounted = next.OwnedAdjacentRoadsNotCounted(counted, blockedFork, out adjacentFork);
                            if (adjacentFork)
                            {

                                //ah...the loop
                                count++;
                                counted.Add(next); // we shouldn't have to do this more than once
                                if (count > max)
                                    max = count;

                                return max;

                            }
                        }
                        //
                        //  loop to the next road to see if it terminates, forks, or just continues...
                        break;
                    default:

                        //
                        //   general strategy:  for each fork in the road, pretend that all but one of the forks are already counted
                        //                      then count the remaining one.  after that, pick another to be couned                           
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
                                max = count + forkCount;

                            forks.Add(road); // put fork back so we can count that fork
                        }

                        return max;
                }
            } while (ownedAdjacentNotCounted.Count != 0);

            return max;

        }

        string[] _StateMessages = new string[] {
            "Uninitialized",        // 0    Uninitialized,                      
            "New Game",             // 1    WaitingForNewGame,                  
            "Starting...",          // 2    Starting,                           
            "Dealing",              // 3    Dealing,                            
            "Wait",                 // 4    WaitingForStart,                    
            "Next When Done",       // 5    AllocResourceForward,            
            "Next When Done",       // 6    AllocateResourceReverse,            
            "",                     // 7    DoneResourceAllocation,             
            "Enter Roll",           // 8    WaitingForRoll,                     
            "Targeted",             // 9    Targeted,                           
            "Cards Lost",           // 10    LostToCardsLikeMonopoly,            
            "Supplemental?",        // 11    Supplemental,                       
            "Done",                 // 12    DoneSupplemental,                   
            "NextWhenDone",         // 13    WaitingForNext,                     
            "Cards Lost",           // 14    LostCardsToSeven,                   
            "Cards Lost",           // 15    MissedOpportunity,                  
            "Pick Game",            // 16    GamePicked,                         
             "Move Baron or Ship"   // 17    MustMoveBaron                       


        };
        public void UpdateUiForState(GameState state)
        {
            StateDescription = _StateMessages[(int)state];
            _btnNextStep.IsEnabled = false;
            _btnUndo.IsEnabled = true;
            _btnWinner.IsEnabled = false;




            Menu_Undo.IsEnabled = false;
            Menu_Winner.IsEnabled = false;

            switch (state)
            {
                case GameState.Uninitialized:
                case GameState.Dealing:
                case GameState.MustMoveBaron:
                    break;
                case GameState.WaitingForNewGame:
                    _btnNextStep.IsEnabled = true;
                    break;
                case GameState.WaitingForStart:
                case GameState.WaitingForNext:
                    Menu_Undo.IsEnabled = true;
                    Menu_Winner.IsEnabled = true;
                    _btnNextStep.IsEnabled = true;
                    _btnUndo.IsEnabled = true;
                    Menu_Undo.IsEnabled = true;
                    break;
                case GameState.DoneSupplemental:
                case GameState.DoneResourceAllocation:
                case GameState.AllocateResourceForward:
                case GameState.AllocateResourceReverse:
                case GameState.Supplemental:
                    _btnNextStep.IsEnabled = true;
                    _btnUndo.IsEnabled = true;
                    Menu_Undo.IsEnabled = true;
                    break;
                case GameState.WaitingForRoll:
                    Menu_Undo.IsEnabled = true;
                    Menu_Winner.IsEnabled = true;
                    _btnNextStep.IsEnabled = false;
                    ShowNumberUi();
                    break;
                case GameState.Targeted:
                case GameState.LostCardsToSeven:
                case GameState.MissedOpportunity:
                case GameState.LostToCardsLikeMonopoly:
                    break;
                default:
                    break;
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

            if (GameState == GameState.AllocateResourceForward || GameState == GameState.AllocateResourceReverse)
            {

                await HideAllPipEllipses();
                _showPipGroupIndex = 0;

            }

            // tell all the Buildings that the CurrentPlayer has changed
            foreach (var building in _gameView.AllBuildings)
            {
                building.CurrentPlayer = CurrentPlayer;
            }

        }

        private void UpdateTurnFlag()
        {
            foreach (PlayerData pd in PlayingPlayers)
            {
                pd.GameData.IsCurrentPlayer = false; // views with Timers should turn off
            }

            CurrentPlayer.GameData.IsCurrentPlayer = true; // this should start the timer for this view
        }

        public void RoadEntered(RoadCtrl road, PointerRoutedEventArgs e)
        {

            if (!CanBuild()) return;
            if (road.IsOwned) return;

            road.Color = CurrentPlayer.GameData.PlayerColor;




            road.Show(true, RoadAllowed(road));

            //
            //   if you forgot, this is good for debugging the layout -- left here in case you change it...again.

            //foreach (var r in road.AdjacentRoads)
            //{
            //    r.Show(true);
            //}
            //foreach (var s in road.AdjacentSettlements)
            //{
            //    s.ShowBuildEllipse();
            //}

        }

        public void RoadExited(RoadCtrl road, PointerRoutedEventArgs e)
        {
            if (!CanBuild()) return;


            if (road.IsOwned) return;

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


            if (!CanBuild()) return;
            if (!RoadAllowed(road)) // clicked on a random road away from a settlement or road
                return;
            if (road.IsOwned)
            {
                if (road.Color != CurrentPlayer.GameData.PlayerColor) // this is not my road I'm clicking on -- bail
                    return;
            }

            await UpdateRoadState(road, road.RoadState, NextRoadState(road), LogType.Normal);
        }



        public Tuple<bool, bool> IsValidBuildingLocation(BuildingCtrl building)
        {
            bool ret = ValidateBuildingLocation(building, out bool showError);
            return new Tuple<bool, bool>(ret, showError);
        }

        //
        //  returns True if it is OK to build this settlement - this is basically a Road check
        bool ValidateBuildingLocation(BuildingCtrl building, out bool showErrorUI)
        {
            showErrorUI = true;
            if (GameState == GameState.WaitingForNewGame || GameState == GameState.WaitingForStart)
            {
                showErrorUI = false;
                return false;
            }

            if (!ValidateBuilding) return true;

            if (!CanBuild())
                return false;

            if (CurrentPlayer == null) // this happens if you move the mouse over the board before a new game is started
            {
                showErrorUI = false;
                return false;
            }


            bool allocationPhase = false;
            bool error = false;

            if (GameState == GameState.AllocateResourceForward || GameState == GameState.AllocateResourceReverse)
                allocationPhase = true;

            error = SettlementsWithinOneSpace(building);

            if (GameState == GameState.AllocateResourceForward || GameState == GameState.AllocateResourceReverse)
            {
                if (building.BuildingToTileDictionary.Count > 0)
                {
                    if (_gameView.GetIsland(building.BuildingToTileDictionary.First().Value) != null)
                    {

                        //  we are on an island - you can't build on an island when you are allocating resources
                        error = true;
                        return false;
                    }
                }
            }

            //
            //  make sure that we have at least one buildable tile
            bool buildableTile = false;
            foreach (var kvp in building.BuildingToTileDictionary)
            {
                if (kvp.Value.ResourceType != ResourceType.Sea)
                {
                    buildableTile = true;
                    break;
                }
            }

            if (!buildableTile)
            {
                showErrorUI = false;
                return false;
            }

            if (!allocationPhase && error == false)
            {
                error = true;
                //
                //   if the settlement is not next to another settlement and we are not in allocation phase, we have to be next to a road
                foreach (RoadCtrl road in building.AdjacentRoads)
                {
                    if (road.Color == CurrentPlayer.GameData.PlayerColor && road.RoadState != RoadState.Unowned)
                    {
                        error = false;
                        break;
                    }

                }

            }

            return !error;
        }


        /// <summary>
        ///     called after the settlement status has been updated.  the PlayerData has already been fixed to represent the new state
        ///     the Views bind directly to the PlayerData, so we don't do anything with the Score (or anything else with PlayerData)
        ///     This View knows how to Log and about the other Buildings and Roads, so put anything in here that is impacted by building something (or "unbuilding" it)
        ///     in this case, recalc the longest road (a buidling can "break" a road) and then log it.
        ///     we also clear all the Pipe ellipses if we are in the allocating phase
        /// </summary>
        public async Task BuildingStateChanged(BuildingCtrl building, BuildingState oldState, LogType logType)
        {
            PlayerData player = CurrentPlayer;
            RecalcLongestRoadAfterBuildingChanges(null, building, player);
            await AddLogEntry(CurrentPlayer, GameState, CatanAction.UpdateBuildingState, true, logType, building.Index, new LogBuildingUpdate(_gameView.CurrentGame.Index, null, building, oldState, building.BuildingState));

            //
            //  if we are in the allocation phase and we change the building state then hide all the Pip ellipses
            if (GameState == GameState.AllocateResourceForward || GameState == GameState.AllocateResourceReverse)
            {
                if (building.BuildingState != BuildingState.Pips) // but NOT if if is transitioning to the Pips state - only happens from the Menu "Show Highest Pip Count"
                {

                    await HideAllPipEllipses();
                    _showPipGroupIndex = 0;
                }
            }

            UpdateTileBuildingOwner(player, building, building.BuildingState, oldState);
        }

        /// <summary>
        ///     called by the BuildingCtrl during PointerPressed to see if it is ok to change the state of the building.
        ///     we can only do that if the state is WaitingForNext and the CurrentPlayer == the owner of the building
        /// </summary>
        /// <returns></returns>
        public bool BuildingStateChangedOk(BuildingCtrl building)
        {
            if (building.Owner != null)
            {
                if (building.Owner.ColorAsString != CurrentPlayer?.ColorAsString) // you can only click on your own stuff and when it is your turn
                {
                    return false;
                }
            }
            if (GameState == GameState.AllocateResourceForward || GameState == GameState.AllocateResourceReverse || GameState == GameState.Supplemental || GameState == GameState.WaitingForNext)
                return true;

            return false;
        }

        public TileCtrl GetTile(int tileIndex, int gameIndex)
        {
            return _gameView.GetTile(tileIndex, gameIndex);
        }

        public RoadCtrl GetRoad(int roadIndex, int gameIndex)
        {
            return _gameView.GetRoad(roadIndex, gameIndex);
        }

        public BuildingCtrl GetBuilding(int settlementIndex, int gameIndex)
        {
            return _gameView.GetSettlement(settlementIndex, gameIndex);
        }

        public PlayerData GetPlayerData(int playerIndex)
        {
            return AllPlayers[playerIndex];
        }


    }
}