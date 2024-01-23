﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Catan10.Logging.StateTransitions;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;

namespace Catan10
{
    public sealed partial class MainPage : Page, IGameCallback, ITileControlCallback
    {
        #region Delegates + Fields + Events + Enums

        private int _roadSkipped = 0;

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public GameContainerCtrl GameContainer
        {
            get
            {
                return CTRL_GameView;
            }
        }

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

        #endregion Properties

        #region Methods

        public async Task DestroyCity(BuildingCtrl building)
        {

            Debug.Assert(CurrentGameState == GameState.MustDestroyCity);
            Debug.Assert(CurrentPlayer.GameData.Resources.UnspentEntitlements.Contains(Entitlement.DestroyCity));
            Debug.Assert(building.Owner == CurrentPlayer);

            if (building.Owner != CurrentPlayer) return;


            if (building.City.HasWall && this.MainPageModel.Settings.HouseRules.WallProtectsCity)
            {
                await DestroyWall.PostDestroyWall(this, building);
            }
            else
            {
                await UpdateBuildingLog.UpdateBuildingState(this, building, BuildingState.Settlement, CurrentGameState);
            }

            await DestroyCity_Next.PostLog(this);

        }

    
        /// <summary>
        ///     called by the BuildingCtrl during PointerPressed to see if it is ok to change the state of the building.
        ///     we can only do that if the state is WaitingForNext and the CurrentPlayer == the owner of the building
        /// </summary>
        /// <returns></returns>
        public bool BuildingStateChangeOk(BuildingCtrl building)
        {
            if (!ValidateBuilding) return true;
            if (MainPageModel.Settings.IsServiceGame)
            {
                if (CurrentPlayer.PlayerIdentifier != TheHuman.PlayerIdentifier) return false;
                Contract.Assert(CurrentPlayer == TheHuman);
            }

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
            if (CurrentGameState == GameState.AllocateResourceForward ||
                CurrentGameState == GameState.AllocateResourceReverse ||
                CurrentGameState == GameState.Supplemental ||
                CurrentGameState == GameState.WaitingForNext)
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
                    Debug.Assert(CurrentPlayer.GameData.Resources.HasEntitlement(Entitlement.Settlement));
                    return true;
                }
            }
            if (building.BuildingState == BuildingState.NoEntitlement) return false;
            //
            //  12/23/2023: if the state is a BuyOrUpgradeKnight but nobody owns it, you can build a knight there.
            if (building.BuildingState == BuildingState.Knight && building.Owner == null) return true;
            return false;
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
            if (game == CTRL_GameView.CurrentGame)
            {
                return;
            }

            if (CurrentGameState == GameState.WaitingForNewGame)
            {
                CTRL_GameView.CurrentGame = game;

                return;
            }
            if (await StaticHelpers.AskUserYesNoQuestion("Are you sure you want to change the game?  The board will reshuffle and you won't be able to get back to this game.", "Yes", "No"))
            {
                CTRL_GameView.CurrentGame = game;

                await VisualShuffle();
            }
        }

        public async Task CurrentPlayerChanged()
        {

            UpdateTurnFlag();

            _stopWatchForTurn.TotalTime = TimeSpan.FromSeconds(0);
            _stopWatchForTurn.StartTimer();

            if (CurrentGameState == GameState.AllocateResourceForward || CurrentGameState == GameState.AllocateResourceReverse)
            {
                await HideAllPipEllipses();
                _showPipGroupIndex = 0;
            }

            // tell all the Buildings that the CurrentPlayer has changed
            foreach (BuildingCtrl building in CTRL_GameView.AllBuildings)
            {
                building.CurrentPlayer = CurrentPlayer;
            }
        }

        public BuildingCtrl GetBuilding(int settlementIndex)
        {
            return CTRL_GameView.GetBuilding(settlementIndex);
        }

        public PlayerModel GetPlayerData(int playerIndex)
        {
            return MainPageModel.AllPlayers[playerIndex];
        }

        public RoadCtrl GetRoad(int roadIndex)
        {
            return CTRL_GameView.GetRoad(roadIndex);
        }

        public TileCtrl GetTile(int tileIndex)
        {
            return CTRL_GameView.GetTile(tileIndex);
        }

        public async Task OnNewGame()
        {
            MainPageModel.Settings.IsLocalGame = true;

            if (MainPageModel.GameState != GameState.WaitingForNewGame)
            {
                if (await StaticHelpers.AskUserYesNoQuestion("Start a new game?", "Yes", "No") == false)
                {
                    return;
                }
            }

            NewGameDlg dlg = new NewGameDlg(MainPageModel.AllPlayers, CTRL_GameView.Games);

            ContentDialogResult result = await dlg.ShowAsync();
            if (( dlg.PlayingPlayers.Count < 3 || dlg.PlayingPlayers.Count > 6 ) && result == ContentDialogResult.Primary)
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
                CTRL_GameView.Reset();
                await this.Reset();

                CTRL_GameView.CurrentGame = dlg.SelectedGame;
                
                MainPageModel.PlayingPlayers.Clear();
                GameInfo info = new GameInfo()
                {
                    Creator = TheHuman.PlayerName,
                    GameIndex = dlg.SelectedIndex,
                    Id = Guid.NewGuid(),
                    Started = false,
                    CitiesAndKnights = dlg.CitiesAndKnights
                };
                await NewGameLog.CreateGame(this, info, CatanAction.GameCreated);
                GameContainer.CurrentGame.HexPanel.CitiesAndKnights = dlg.CitiesAndKnights;
                MainPageModel.PlayingPlayers.Clear();
                foreach (var player in dlg.PlayingPlayers)
                {
                    await AddPlayerLog.AddPlayer(this, player.PlayerName);
                }

                CTRL_PlayerTrackerCtrl.UpdateLayout();
            }
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
            if (CurrentGameState == GameState.DestroyRoad)
            {
                await DiplomatLog.PostLogEntry(this, road);
                return;
            }

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
            await UpdateRoadLog.PostLogEntry(this, road, NextRoadState(road));
            //
            //  UpdateRoad state will be done in the IGameController
        }

        //
        //  this is where the CurrentPlayer can pick somebody to target for the Baron.  this is always called from the tile, so first
        //  make sure that the user is elligible to move baron/ship
        //
        //  it can happen because the user rolled 7 or they played a BuyOrUpgradeKnight, or they could do both in the same turn
        //
        //  if there is a knight entitlement, then they played a knight
        //  if there is not, then they rolled 7
        //
        //
        public void TileRightTapped(TileCtrl targetTile, RightTappedRoutedEventArgs rte)
        {
            //   this.TraceMessage($"Tile={targetTile} CurrentPlayer={CurrentPlayer.PlayerName} Owners={targetTile.OwnedBuildings.FlattenProperty("Owner", ",")}" );

            if (!CurrentPlayer.GameData.Resources.UnspentEntitlements.Contains(Entitlement.MoveBaron)) return;
            //
            //  need the reason for moving the baron for booking and UI message
            MoveBaronReason reason = MoveBaronReason.Rolled7;
            MustMoveBaronLog log = MainPageModel.Log.PeekAction as MustMoveBaronLog;
            if (log != null) reason = log.Reason;

            this.TraceMessage($"Reason = {reason}");

            PlayerGameModel playerGameData = CurrentPlayer.GameData;

            TargetWeapon weapon = TargetWeapon.Baron;

            //
            //  I made this a local function to capture the stack variables.
            async void Baron_MenuClicked(object s, RoutedEventArgs e)
            {
                //
                //  pop the dialog to pick a card
                //
                var victims = (List<string>)((MenuFlyoutItem)s).Tag;
                ResourceType stolenResource = ResourceType.None;

                //
                //  log to tell the other clients what we did
                await MovedBaronLog.PostLog(this,
                                            victims,
                                            targetTile.Index,
                                            weapon == TargetWeapon.Baron ? CTRL_GameView.BaronTile.Index : CTRL_GameView.PirateShipTile.Index,  // the previous index
                                            weapon,
                                            reason,
                                            stolenResource);
            }

            _menuBaron.Items.Clear();
            MenuFlyoutItem item = null;
            if (targetTile.ResourceType == ResourceType.Sea) // we move pirate instead of Baron
            { // this means we are moving the pirate ship
                Debug.Assert(false, "I didn't try to make this work yet.");
                weapon = TargetWeapon.PirateShip;
                List<RoadCtrl> roads = new List<RoadCtrl>();
                foreach (RoadLocation location in Enum.GetValues(typeof(RoadLocation)))
                {
                    if (location == RoadLocation.None)
                    {
                        continue;
                    }

                    RoadCtrl r = CTRL_GameView.GetRoadAt(targetTile, location);
                    if (r.IsOwned && r.RoadState == RoadState.Ship)
                    {
                        roads.Add(r);
                    }
                }

                foreach (RoadCtrl road in roads)
                {
                    string s = "";

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
                        if (reason == MoveBaronReason.PlayedDevCard)
                        {
                            s = "Playing Knight to Target: " + s;
                        }
                        else
                        {
                            s = "Targetting " + s;
                        }

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

                var targets = PotentialTargets(targetTile, CurrentPlayer);
                if (targets.Count > 0)
                {
                    if (reason == MoveBaronReason.Bishop)
                    {
                        // when a Bishop is played, you collect from everybody

                        string message = "Collect one card from ";
                        foreach (var target in targets)
                        {
                            message += target + ", ";
                        }
                        message = message.Substring(0, message.Length - 1) + ".";
                        item = new MenuFlyoutItem()
                        {
                            Text = message,
                            Tag = targets

                        };
                        item.Click += Baron_MenuClicked;
                        _menuBaron.Items.Add(item);

                    }
                    else

                    {
                        string message;
                        foreach (var target in targets)
                        {
                            if (reason == MoveBaronReason.PlayedDevCard)
                            {
                                message = "Knight Targets: " + target;
                            }
                            else
                            {
                                message = "Targetting " + target;
                            }
                            item = new MenuFlyoutItem()
                            {
                                Text = message,
                                Tag = new List<string>
                                {
                                    target
                                }
                            };
                            item.Click += Baron_MenuClicked;
                            _menuBaron.Items.Add(item);
                        }
                    }
                }
                else // No targets!
                {
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

        private List<string> PotentialTargets(TileCtrl tile, PlayerModel owner)
        {
            HashSet<string> targets = new HashSet<string>();

            foreach (BuildingCtrl settlement in tile.OwnedBuildings)
            {
                if (settlement.Owner != owner)
                {
                    targets.Add(settlement.Owner.PlayerName);
                }
            }

            return new List<string>(targets);
        }

        public async Task UpgradeKnight(BuildingCtrl building)
        {
            if (!CurrentPlayer.GameData.Resources.HasEntitlement(Entitlement.UpgradeKnight))
            {
                
                return;
            }
            int newRank = (int) building.Knight.KnightRank + 1;
            await KnightStateChangeLog.ToggleActiveState(this, building.Index, building.Knight, ( KnightRank )newRank, building.Knight.Activated);
            await Task.Delay(0); 
        }
        public async Task ActivateKnight(BuildingCtrl building, bool activated)
        {
            if (!CurrentPlayer.GameData.Resources.HasEntitlement(Entitlement.ActivateKnight))
            {
                await Task.Delay(0); //ignoring the request
            }

            await KnightStateChangeLog.ToggleActiveState(this, building.Index, building.Knight, building.Knight.KnightRank, activated);
        }

        

        public bool HasEntitlement(Entitlement entitlement)
        {
            return CurrentPlayer.GameData.Resources.HasEntitlement(entitlement);
        }
        public BuildingState ValidateBuildingLocation(BuildingCtrl building)
        {
            if (( CurrentGameState == GameState.WaitingForNewGame || CurrentGameState == GameState.BeginResourceAllocation ) && ValidateBuilding)
            {
                return BuildingState.None;
            }

            if (!ValidateBuilding) // if i'm not validating building, build anywhere
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

            if (CurrentGameState == GameState.AllocateResourceForward || CurrentGameState == GameState.AllocateResourceReverse)
            {
                if (building.BuildingToTileDictionary.Count > 0)
                {
                    if (CTRL_GameView.GetIsland(building.BuildingToTileDictionary.First().Value) != null)
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

            // 12/24/2023
            //              You can build a BuyKnight if:
            //              1. you are running CitiesAndKnights
            //              2. you have the entitlement
            //              3. you are next to a road
            //              4. (1/5/2024):  You don't have the BuyKnight entitlement, but you are in the PlaceDeserterKnight GameState
            //
            //     this logic means you *must* spend your knights before you build any other kind of settlements.
            if (MainPageModel.GameInfo.CitiesAndKnights && ( CurrentPlayer.GameData.Resources.HasEntitlement(Entitlement.BuyKnight) || CurrentGameState == GameState.PlaceDeserterKnight )
                && OwnedAdjacentRoad(CurrentPlayer, building) && building.BuildingState == BuildingState.None)
            {
                return BuildingState.Knight;
            }

            if (SettlementsWithinOneSpace(building))
            {

                return BuildingState.Error;
            }
            if (!OwnedAdjacentRoad(CurrentPlayer, building))
            {
                // you can't build within one space of another settlement and you have to have a road next to a building...unless you are allocating

                if (!allocationPhase)
                {
                    return BuildingState.Error;
                }
            }

            //
            //  if we get here, we have a valid place to build (or we've bypassed the business logic)...make sure there is an entitlement

            if (CurrentPlayer.GameData.Resources.HasEntitlement(Entitlement.Settlement))
            {
                return BuildingState.Build;
            }
            else
            {
                return BuildingState.NoEntitlement;
            }

        }

        private bool OwnedAdjacentRoad(PlayerModel player, BuildingCtrl building)
        {
            foreach (RoadCtrl road in building.AdjacentRoads)
            {
                if (road.Owner == player && road.RoadState != RoadState.Unowned)
                {
                    return true;
                }
            }
            return false;
        }

        internal PlayerModel PlayerNameToPlayer(string name, ICollection<PlayerModel> players)
        {
            foreach (var player in players)
            {
                if (player.PlayerName == name)
                    return player;
            }
            throw new Exception("bad targetList passed PlayerNameToPlayer");
        }

        //
        //   when a BuyOrUpgradeKnight is played
        //      1. Increment the target's TimesTargetted
        //      2. Move the ship or baron to the target tile
        //      3. set the flag that says a BuyOrUpgradeKnight has been played this turn or that the Robber has been moved because of a 7
        //      4. If BuyOrUpgradeKnight Played Increment the source player (which is always the current player) CK_Knights played
        //      5. Log that it happened.
        //      6. check to see if we should update the Largest Army
        private async Task AssignBaronOrKnight(PlayerModel targetPlayer, TileCtrl targetTile, TargetWeapon weapon, CatanAction action, LogType logType)
        {
            int inc = 1;
            if (logType == LogType.Undo)
            {
                //
                //   if this is an undo action, decrement the counter and set the flag to false so the player can do it again
                inc = -1;
            }

            if (targetPlayer != null)
            {
                targetPlayer.GameData.TimesTargeted += inc;
            }

            TileCtrl startTile;

            if (weapon == TargetWeapon.PirateShip)
            {
                startTile = CTRL_GameView.PirateShipTile;
                CTRL_GameView.PirateShipTile = targetTile;
            }
            else
            {
                startTile = CTRL_GameView.BaronTile;
                CTRL_GameView.BaronTile = targetTile;
            }

            if (action == CatanAction.PlayedKnight)
            {

                CurrentPlayer.GameData.Resources.KnightsPlayed += inc;
                AssignLargestArmy();
            }

            //  await AddLogEntry(CurrentPlayer, CurrentGameState, action, true, logType, 1, new LogBaronOrPirate(CTRL_GameView.CurrentGame.Index, targetPlayer, CurrentPlayer, startTile, targetTile, weapon, action));

            if (CurrentGameState == GameState.MustMoveBaron && logType != LogType.Undo)
            {
                // await SetStateAsync(CurrentPlayer, GameState.WaitingForNext, false, logType);
            }

            if (CurrentGameState == GameState.WaitingForRoll)
            {
                if (logType == LogType.Undo)
                {
                    MustMoveBaron = true;
                }
                else if (logType == LogType.Normal)
                {
                    //
                    //  we assigned the baron before rolling --

                    Debug.Assert(this.MustMoveBaron, "To hit this condition, the Baron Button needed to be checked");
                    MustMoveBaron = false;
                }
            }
            await Task.Delay(0);
        }

        //
        //  if the current player ends up with more than 3 knights, see if they have the largest army by looking
        //  at everybody and check to see who has the most knights played.
        //
        //  since this is called from Undo, you have to set it to false if it is less than 2 in case you undid the one that made you the larget army
        //
        public void AssignLargestArmy()
        {
            if (CurrentPlayer.GameData.Resources.KnightsPlayed > 2)
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
                        knightCount = p.GameData.Resources.KnightsPlayed;
                    }
                }

                if (CurrentPlayer.GameData.Resources.KnightsPlayed > knightCount)
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
            if (road.Keys.Count == 1 && CTRL_GameView.CurrentGame.GameData.MaxShips > 0)
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
                    key.Tile.OwnedBuildings.Remove(building);
                    if (CTRL_GameView.HasIslands)
                    {
                        Island island = CTRL_GameView.GetIsland(key.Tile);
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
                    if (key.Tile.OwnedBuildings.Contains(building) == false && building.BuildingState != BuildingState.Knight) // 12/23/2023: don't count BuyOrUpgradeKnight as a TileOwner so that resource allocation works
                    {
                        key.Tile.OwnedBuildings.Add(building);
                    }
                    if (CTRL_GameView.HasIslands)
                    {
                        Island island = CTRL_GameView.GetIsland(key.Tile);
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

        public async Task KnightLeftPointerPressed(BuildingCtrl building)
        {


            if (building == null) return;

            Debug.Assert(building.BuildingState == BuildingState.None || building.BuildingState == BuildingState.Knight);


            if (building.Owner == null && HasEntitlement(Entitlement.BuyKnight))
            {
                building.ResetTempBuildingState(); // this was set "out of band" in mouse enter, putting it back.

                await UpdateBuildingLog.UpdateBuildingState(this, building, BuildingState.Knight, CurrentGameState);
                return;

            }
   

            if (building.Owner == CurrentPlayer && HasEntitlement(Entitlement.MoveBaronWithKnight) && building.Knight.Activated)
            {

                // make sure that this knight is next to the 
                foreach (var tile in building.BuildingToTileDictionary.Values)
                {
                    if (tile.HasBaron)
                    {
                        //
                        // this entitlement allows the user to click on a knight that will transition to 
                        // MustMoveBaron.  
                        await MoveBaronWithKnightLog.PostLog(this, building);
                        break;
                    }
                }
            }

            if (CurrentGameState == GameState.PlaceDeserterKnight)
            {
                await DeserterLog.PlaceDeserterLog(this, building);
                return;
            }


            if (CurrentGameState == GameState.PickDeserter)
            {

                if (building.Owner == CurrentPlayer) return; // don't destroy your own knight

                await DeserterLog.PickDeserterLog(this, building);
                return;
            }



            if (building.IsKnight && building.Knight.Activated == false && HasEntitlement(Entitlement.ActivateKnight))
            {
                await ActivateKnight(building, true);
                return;
            }

            if (HasEntitlement(Entitlement.UpgradeKnight) && building.Knight.KnightRank < KnightRank.Mighty && building.Owner == CurrentPlayer)
            {
                await UpgradeKnight(building);
            }
            //
            // last check -- is there a knight they are trying to kick out?
            if (CurrentGameState == GameState.WaitingForRoll && building.IsKnight && building.Knight.Activated && MainPageModel.Settings.HouseRules.MoveBaronBeforeRoll)
            {
                foreach (var tile in building.BuildingToTileDictionary.Values)
                {
                    if (tile == CTRL_GameView.BaronTile)
                    {
                        CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.MoveBaronWithKnight);
                        await MoveBaronWithKnightLog.PostLog(this, building);
                        return;
                    }
                }
            }
        }

        #endregion Methods
    }
}