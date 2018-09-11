using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
            {
                return;
            }

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
        private async Task<bool> UndoLogLine(LogEntry logLine)
        {
            //this.TraceMessage($"Replay:{replayingLog} Line:{logLine}");
            switch (logLine.Action)
            {
                case CatanAction.Rolled:
                    int roll = PopRoll();
                    break;
                case CatanAction.AddResourceCount:
                    LogResourceCount lrc = logLine.Tag as LogResourceCount;
                    if (logLine.PlayerData != null)
                    {
                        logLine.PlayerData.GameData.PlayerResourceData.AddResourceCount(lrc.ResourceType, -logLine.Number);
                    }
                    else
                    {
                        Ctrl_PlayerResourceCountCtrl.GameResourceData.AddResourceCount(lrc.ResourceType, -logLine.Number);
                    }

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
                    if (lst.OldState == GameState.WaitingForStart)
                    {
                        break;
                    }

                    await SetStateAsync(logLine.PlayerData, lst.OldState, logLine.StopProcessingUndo, LogType.Undo);
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
                    await UpdateRoadState(roadUpdate.Road, roadUpdate.NewRoadState, roadUpdate.OldRoadState, LogType.Undo);
                    await CalculateAndSetLongestRoad(LogType.Undo);
                    break;
                case CatanAction.UpdateBuildingState:
                    LogBuildingUpdate buildingUpdate = logLine.Tag as LogBuildingUpdate;
                    await buildingUpdate.Building.UpdateBuildingState(buildingUpdate.NewBuildingState, buildingUpdate.OldBuildingState); // NOTE:  New and Old have been swapped                      
                    await CalculateAndSetLongestRoad(LogType.Undo);  // this executes with the new tracking -- things may change since ties are different.
                    break;
                case CatanAction.RoadTrackingChanged:
                    LogRoadTrackingChanged lrtc = logLine.Tag as LogRoadTrackingChanged;
                    _raceTracking.Undo(lrtc.OldState, lrtc.NewState, logLine.PlayerData, this, this.GameState); // this will log the undo action to balance the log write
                    Debug.Assert(logLine.StopProcessingUndo == false, "this has no UI affect");
                    break;
                case CatanAction.ChangedPlayerProperty:
                    LogPropertyChanged lpc = logLine.Tag as LogPropertyChanged;
                    logLine.PlayerData.GameData.SetKeyValue<PlayerGameData>(lpc.PropertyName, lpc.OldVal);

                    break;

                default:
                    break;
            }

            return logLine.StopProcessingUndo;


        }

        public async Task OnUndo()
        {
            if (_log.Count < SMALLEST_STATE_COUNT) // the games starts with 5 states that can't be undone
            {
                return;
            }

            try
            {
                _log.State = LogState.Undo;
                for (int i = _log.Count - 1; i >= SMALLEST_STATE_COUNT - 1; i--)
                {
                    if (_log[i].LogType == LogType.Undo)
                    {
                        continue; // if we have an undo action, skip it
                    }

                    if (_log[i].Undone == true)
                    {
                        continue;   // we already undid this one
                    }

                    if (_log[i].LogType == LogType.DoNotUndo)
                    {
                        continue;
                    }

                    bool ret = await UndoLogLine(_log[i]);
                    //
                    //  if you undo and land on one of these states, then stop processing undo
                    bool stop = false;
                    //switch (_log[i].GameState)
                    //{
                    //    case GameState.MustMoveBaron:
                    //        stop = true;
                    //        break;
                    //    default:
                    //        break;
                    //}
                    //if (stop)
                    //{
                    //    this.TraceMessage($"Stopping because of GameState. Action:{_log[i].Action} State:{_log[i].GameState} Stop Undo: {stop}");
                    //    break;
                    //}

                    // if it was this action, we also stop the Unfo
                    switch (_log[i].Action)
                    {
                        case CatanAction.Rolled:
                        case CatanAction.ChangedPlayer:
                        case CatanAction.PlayedKnight:
                        case CatanAction.AssignedBaron:
                        case CatanAction.UpdatedRoadState:
                        case CatanAction.UpdateBuildingState:
                        case CatanAction.AssignedPirateShip:
                        case CatanAction.RolledSeven:
                            stop = true;
                            break;
                        default:
                            break;
                    }

                    //  this.TraceMessage($"Action:{_log[i].Action} State:{_log[i].GameState} Stop Undo: {stop}");

                    if (stop)
                    {
                        break;
                    }

                    // if (_log[i - 1].StopProcessingUndo) break;
                    //if (ret) break;
                }
            }
            finally
            {
                _log.State = LogState.Normal;
                UpdateUiForState(_log.Last().GameState);
            }

            await _log.WriteUnwrittenLinesToDisk();
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
                    {
                        return;
                    }
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
            if (_log == null)
            {
                return false;
            }

            if (_log.Count == 0)
            {
                return false;
            }

            GameState state = State.GameState;

            if (state == GameState.WaitingForNext || // I can build after I roll               
                state == GameState.AllocateResourceForward || // I can build during the initial phase )
                state == GameState.AllocateResourceReverse || state == GameState.Supplemental)
            {
                return true;
            }

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
            {
                return;
            }

            //if (GameState != GameState.WaitingForNext &&
            //    GameState != GameState.WaitingForRoll)
            //    return;

            PlayerGameData playerGameData = CurrentPlayer.GameData;


            CatanAction action = CatanAction.None;
            TargetWeapon weapon = TargetWeapon.Baron;

            if (playerGameData.MovedBaronAfterRollingSeven != false && playerGameData.PlayedKnightThisTurn) // not eligible to move baron
            {
                return;
            }

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
                foreach (PlayerData p in PlayingPlayers)
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



        //
        //  why put this in a seperate function?  so you can find it with CTL+, w/o having to remember it is because of a PointerPressed event...
        ///
        private async Task UpdateRoadState(RoadCtrl road, RoadState oldState, RoadState newState, LogType logType)
        {
            if (newState == oldState)
            {
                return;
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



            await AddLogEntry(CurrentPlayer, GameState, CatanAction.UpdatedRoadState, true, logType, road.Number, new LogRoadUpdate(_gameView.CurrentGame.Index, road, oldState, road.RoadState));
            await CalculateAndSetLongestRoad(logType);

        }

        private PlayerData MaxRoadPlayer
        {
            get
            {
                foreach (PlayerData player in PlayingPlayers)
                {
                    if (player.GameData.HasLongestRoad)
                    {
                        return player;
                    }
                }

                return null;
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        ///         this looks at the global state of all the roads and makes sure that it
        ///         1. keeps track of who gets to a road count >= 5 first
        ///         2. makes sure that the right player gets the longest road
        ///         3. works when an Undo action happens
        ///         5. works when a road is "broken"
        /// </summary>
        private async Task CalculateAndSetLongestRoad(LogType logType)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            //
            //  make the compiler error go away
            // await Task.Delay(0);

            PlayerData longestRoadPlayer = null;
            int maxRoads = -1;
            List<PlayerData> tiedPlayers = new List<PlayerData>();

            try
            {

                _raceTracking.BeginChanges();
                //
                //  first loop over the players and find the set of players that have the longest road
                //
                foreach (PlayerData p in PlayingPlayers)
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
                foreach (PlayerData p in tiedPlayers)
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
                _raceTracking.EndChanges(CurrentPlayer, GameState, logType);
            }

        }



        private bool RoadAllowed(RoadCtrl road)
        {
            if (!ValidateBuilding)
            {
                return true;
            }

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


        private void UpdateTileBuildingOwner(PlayerData player, BuildingCtrl building, BuildingState newState, BuildingState oldState)
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
                        {
                            break;
                        }
                    }
                }
            }
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
            try
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
            catch
            {
                this.TraceMessage($"You didn't add a description for the GameState {state}");
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
            foreach (BuildingCtrl building in _gameView.AllBuildings)
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

            if (!CanBuild())
            {
                return;
            }

            if (road.IsOwned)
            {
                return;
            }

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
            if (!CanBuild())
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


            if (!CanBuild())
            {
                return;
            }

            if (!RoadAllowed(road)) // clicked on a random road away from a settlement or road
            {
                return;
            }

            if (road.IsOwned)
            {
                if (road.Color != CurrentPlayer.GameData.PlayerColor) // this is not my road I'm clicking on -- bail
                {
                    return;
                }
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

            if (!ValidateBuilding)
            {
                return true;
            }

            if (!CanBuild())
            {
                return false;
            }

            if (CurrentPlayer == null) // this happens if you move the mouse over the board before a new game is started
            {
                showErrorUI = false;
                return false;
            }


            bool allocationPhase = false;
            bool error = false;

            if (GameState == GameState.AllocateResourceForward || GameState == GameState.AllocateResourceReverse)
            {
                allocationPhase = true;
            }

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
        ///     we also clear all the Pip ellipses if we are in the allocating phase
        /// </summary>
        public async Task BuildingStateChanged(BuildingCtrl building, BuildingState oldState, LogType logType)
        {
            PlayerData player = CurrentPlayer;

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

            //
            //  NOTE:  these have to be called in this order so that the undo works correctly
            await AddLogEntry(CurrentPlayer, GameState, CatanAction.UpdateBuildingState, true, logType, building.Index, new LogBuildingUpdate(_gameView.CurrentGame.Index, null, building, oldState, building.BuildingState));
            UpdateTileBuildingOwner(player, building, building.BuildingState, oldState);
            await CalculateAndSetLongestRoad(logType);

        }

        /// <summary>
        ///     called by the BuildingCtrl during PointerPressed to see if it is ok to change the state of the building.
        ///     we can only do that if the state is WaitingForNext and the CurrentPlayer == the owner of the building
        /// </summary>
        /// <returns></returns>
        public bool BuildingStateChangeOk(BuildingCtrl building)
        {
            if (building.Owner != null)
            {
                if (building.Owner.ColorAsString != CurrentPlayer?.ColorAsString) // you can only click on your own stuff and when it is your turn
                {
                    return false;
                }
            }
            if (GameState == GameState.AllocateResourceForward || GameState == GameState.AllocateResourceReverse || GameState == GameState.Supplemental || GameState == GameState.WaitingForNext)
            {
                return true;
            }

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