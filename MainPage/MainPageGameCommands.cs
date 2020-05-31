using System;

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Catan10
{
    public sealed partial class MainPage : Page
    {
        private readonly List<PlayerModel> defaultPlayers = new List<PlayerModel>()
        {
            new PlayerModel() {PlayerName = "Joe", ImageFileName = "ms-appx:Assets/DefaultPlayers/joe.jpg", ForegroundColor=Colors.White, PrimaryBackgroundColor=Colors.SlateBlue, SecondaryBackgroundColor = Colors.Black,PlayerIdentifier = Guid.Parse("{2B685447-31D9-4DCA-B29F-6FEC870E3AC5}")},
            new PlayerModel() {PlayerName = "Dodgy", ImageFileName = "ms-appx:Assets/DefaultPlayers/dodgy.jpg", ForegroundColor=Colors.White, PrimaryBackgroundColor=Colors.Red, SecondaryBackgroundColor = Colors.Black , PlayerIdentifier = Guid.Parse("{2B685447-31D9-4DCA-B29F-6FEC870E3AC6}")},
            new PlayerModel() {PlayerName = "Doug", ImageFileName = "ms-appx:Assets/DefaultPlayers/doug.jpg", ForegroundColor=Colors.Black, PrimaryBackgroundColor=Colors.DarkGray, SecondaryBackgroundColor = Colors.LightGray, PlayerIdentifier = Guid.Parse("{2B685447-31D9-4DCA-B29F-6FEC870E3AC7}") },
            new PlayerModel() {PlayerName = "Robert", ImageFileName = "ms-appx:Assets/DefaultPlayers/robert.jpg", ForegroundColor=Colors.White, PrimaryBackgroundColor=Colors.Black, SecondaryBackgroundColor = Colors.DarkGray, PlayerIdentifier = Guid.Parse("{2B685447-31D9-4DCA-B29F-6FEC870E3AC8}")},
            new PlayerModel() {PlayerName = "Chris", ImageFileName = "ms-appx:Assets/DefaultPlayers/chris.jpg", ForegroundColor=Colors.White, PrimaryBackgroundColor=Colors.Teal, SecondaryBackgroundColor = Colors.Black, PlayerIdentifier = Guid.Parse("{2B685447-31D9-4DCA-B29F-6FEC870E3AC9}") },
            new PlayerModel() {PlayerName = "Cort", ImageFileName = "ms-appx:Assets/DefaultPlayers/cort.jpg", ForegroundColor=Colors.White, PrimaryBackgroundColor=Colors.Green, SecondaryBackgroundColor = Colors.Black, PlayerIdentifier = Guid.Parse("{2B685447-31D9-4DCA-B29F-6FEC870E3ACA}") },
            new PlayerModel() {PlayerName = "Adrian", ImageFileName = "ms-appx:Assets/DefaultPlayers/adrian.jpg", ForegroundColor=Colors.White, PrimaryBackgroundColor=Colors.Purple, SecondaryBackgroundColor = Colors.Black, PlayerIdentifier = Guid.Parse("{2B685447-31D9-4DCA-B29F-6FEC870E3ACB}") },
        };

        private void CreateMenuItems()
        {
            if (StaticHelpers.IsInVisualStudioDesignMode)
            {
                return;
            }

            Menu_Games.Items.Clear();

            //
            //  The Games Menu has the Description as the menu text ("Regular" should be checked)
            //  and the CatanGameCtrl object in the tag.  this is used to manipulate CurrentGame in the GameContainer
            //
            List<CatanGameCtrl> availableGames = _gameView.Games;
            foreach (CatanGameCtrl game in availableGames)
            {
                ToggleMenuFlyoutItem item = new ToggleMenuFlyoutItem
                {
                    Text = game.Description,
                    Tag = game
                };
                if (item.Text == "Regular")
                {
                    item.IsChecked = true;
                    //
                    //   current game is set to Regular in GameViewCainer.Init() calledi in the Page.NavigatedTo method
                }

                item.Click += Menu_SelectGame;
                Menu_Games.Items.Add(item);
            }

            foreach (var kvp in CatanColors.NameToColorDictionary)
            {
                ToggleMenuFlyoutItem item = new ToggleMenuFlyoutItem
                {
                    Text = kvp.Key,
                    Tag = kvp.Value
                };

                item.Click += PlayerPrimaryColor_Clicked;
                Menu_Primary_Color.Items.Add(item);

                item = new ToggleMenuFlyoutItem
                {
                    Text = kvp.Key,
                    Tag = kvp.Value
                };
                item.Click += PlayerSecondaryColor_Clicked;
                Menu_Secondary_Color.Items.Add(item);
            }
        }

        private async Task DoRedo()
        {
            if (CurrentGameState == GameState.WaitingForNewGame || !MainPageModel.EnableUiInteraction)
            {
                return;
            }
            try
            {
                MainPageModel.EnableUiInteraction = false;
                this.TraceMessage("starting redo");
                bool ret = await RedoAsync();
                this.TraceMessage("done redo");
            }
            finally
            {
                MainPageModel.EnableUiInteraction = true;
            }
        }

        private Target FindTarget(List<Target> targetList, ResourceType resourceType, int minPips)
        {
            targetList.Sort((s1, s2) => s2.ResourcePotential - s1.ResourcePotential);
            foreach (var option in targetList)
            {
                if (option.Tile.ResourceType == resourceType)
                {
                    if (option.ResourcePotential >= minPips)
                    {
                        return option;
                    }
                }
            }

            return null;
        }

        private async Task<List<PlayerModel>> GetDefaultUsers()
        {
            foreach (var player in defaultPlayers)
            {
                await player.LoadImage();
            };

            return defaultPlayers;
        }

        private async void Menu_OnNewGame(object sender, RoutedEventArgs e)
        {
            // await OnNewGame?.Invoke(this, EventArgs.Empty);
            await OnNewGame();
        }

        private async void Menu_SelectGame(object sender, RoutedEventArgs e)
        {
            ToggleMenuFlyoutItem item = sender as ToggleMenuFlyoutItem;

            foreach (ToggleMenuFlyoutItem subItem in Menu_Games.Items)
            {
                if (subItem != item)
                {
                    subItem.IsChecked = false;
                }
            }

            await ChangeGame(item.Tag as CatanGameCtrl);
        }

        private void Menu_ShowSavedGames(object sender, RoutedEventArgs e)
        {
            //await LoadSavedGames(); // this will load the saved games into the UI control
            //_savedGameGrid.Visibility = Visibility.Visible;

            //var folder = await StaticHelpers.GetSaveFolder();
            //var picker = new FileOpenPicker()
            //{
            //    ViewMode = PickerViewMode.List,
            //    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            //};
            //picker.FileTypeFilter.Add(".SavedCatanGame");
            //var file = await picker.PickSingleFileAsync();
            //if (file != null)
            //{
            //   Log newLog = new Log();
            //   newLog =  await newLog.LoadLog(file.Name, this);
            //   _log = await ReplayLog(newLog);

            //    UpdateUiForState(_log.Last().GameState);
            //}

            //OpenGameDlg dlg = new OpenGameDlg();
            //await dlg.LoadGames();
            //await dlg.ShowAsync();
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            _contextMenu.Hide();
        }

        private async void OnChangeRandomGoldTileCount(object sender, RoutedEventArgs e)
        {
            var goldTileCount = await StaticHelpers.GetUserString("How Many Gold Tiles?", RandomGoldTileCount.ToString());
            if (Int32.TryParse(goldTileCount, out int newVal))
            {
                RandomGoldTileCount = newVal;
                // not setting the gold tiles to be the new value for now becaues of Undo problems...instead hit Next and then Undo
            }
        }

        private void OnMenuClosed(object sender, object e)
        {
        }

        private void OnMenuOpened(object sender, object e)
        {
        }

        private async void OnNewGame(object sender, RoutedEventArgs e)
        {
            await OnNewGame();
        }

        /// <summary>
        ///     this function is executed when the user clicks on the Next button in the UI. there are some "challenges" here with the user clicking Next multiple times before
        ///     the functions complete.  This is becuase the function returns void, so control is returned to the user before any Tasks are completed.  to guard against this,
        ///     we do two things
        ///
        ///     1. disable the button on entry, and when the top level Task oriented call is completed, we enable the button
        ///     2. you'd think this would be enough, but I found through debugging that I could click fast enough to get two calls at the same time, so I also protect the function
        ///        with a member variable (_insideNextFunction) and reject more than one call into the function.  As we are single threaded, this should be safe.
        ///
        ///
        /// </summary>
        private async void OnNextStep(object sender, RoutedEventArgs e)
        {
            try
            {
                //  this.TraceMessage("NextStep");
                if (!MainPageModel.EnableUiInteraction)
                {
                    //   this.TraceMessage("CallRejected");
                    return;
                }
                await NextState();
                // this.TraceMessage("Finished NextState");
            }
            finally
            {
                //   this.TraceMessage("leaving NextStep");
            }
        }

        private async void OnPickOptimalBaron(object sender, RoutedEventArgs e)
        {
            if (CurrentGameState != GameState.WaitingForRoll && CurrentGameState != GameState.WaitingForNext && CurrentGameState != GameState.MustMoveBaron)
            {
                return;
            }

            PlayerGameModel playerGameData = CurrentPlayer.GameData;
            if (playerGameData.MovedBaronAfterRollingSeven != false && playerGameData.PlayedKnightThisTurn) // not eligible to move baron
            {
                return;
            }

            List<Target> targetList = PickBaronVictim(true);
            if (targetList.Count == 0)
            {
                MessageDialog dlg = new MessageDialog("I can't seem to be able to find a good candidate.\nFigure it out yourself.");
                await dlg.ShowAsync();
                return;
            }

            //
            //  Get the high score
            //  we assume that if the high score is < 6 then we are in the expansion phase of the game

            int highScore = 0;
            foreach (var p in MainPageModel.AllPlayers)
            {
                if (p.GameData.Score > highScore)
                {
                    highScore = p.GameData.Score;
                }
            }

            targetList.Sort((s1, s2) => s2.ResourcePotential - s1.ResourcePotential);
            Target target = null;
            int most = targetList[0].ResourcePotential;
            ResourceType[] orderedListOfResources = null;
            if (highScore < 6)
            {
                orderedListOfResources = new ResourceType[] { ResourceType.Brick, ResourceType.Wood, ResourceType.Wheat, ResourceType.Sheep, ResourceType.Ore };
            }
            else
            {
                orderedListOfResources = new ResourceType[] { ResourceType.Ore, ResourceType.Wheat, ResourceType.Sheep, ResourceType.Brick, ResourceType.Wood };
            }

            //
            //   if somebody has 9 points, remove all options with player score < 9

            if (highScore == 9)
            {
                for (int i = targetList.Count - 1; i >= 0; i--)
                {
                    if (targetList[i].Player.GameData.Score < 9)
                    {
                        targetList.RemoveAt(i);
                    }
                }
            }

            if (targetList.Count == 0)
            {
                MessageDialog dlg = new MessageDialog("I can't seem to be able to find a good candidate.\nFigure it out yourself.");
                await dlg.ShowAsync();
                return;
            }
            List<Target> topList = new List<Target>();
            int mostPotential = targetList[0].ResourcePotential;
            foreach (var t in targetList)
            {
                if (t.ResourcePotential > mostPotential - .1)
                {
                    topList.Add(t);
                }
                else
                {
                    break; // it is an ordered list
                }
            }

            Random rand = new Random((int)DateTime.Now.Ticks);

            target = topList[rand.Next(topList.Count)];

            CatanAction action = CatanAction.PlayedKnight;
            TargetWeapon weapon = TargetWeapon.Baron;
            if (CurrentGameState == GameState.MustMoveBaron)
            {
                action = CatanAction.AssignedBaron;
            }

            await AssignBaronOrKnight(target.Player, target.Tile, weapon, action, LogType.Normal);
        }

        /// <summary>
        ///     tries to be smart about where to place the baron
        ///     1. if Dodgy is playing (since he's always Red...) be sure and put it on him
        ///     2. pick the one with the most resource generating potential
        ///     3. if the highscore less than 5, try to block brick
        ///     5. if the highscore >=5, try to block Ore
        /// </summary>
        /// <summary>
        ///     Picks a random place for the Baron that
        ///     1. doesn't impact the current player
        ///     2. does impact another player
        ///     3. twice as likely to Block a City vs. Block a settlement
        /// </summary>
        private async void OnPickRandomBaron(object sender, RoutedEventArgs e)
        {
            if (CurrentGameState != GameState.WaitingForRoll && CurrentGameState != GameState.WaitingForNext && CurrentGameState != GameState.MustMoveBaron)
            {
                return;
            }

            PlayerGameModel playerGameData = CurrentPlayer.GameData;
            if (playerGameData.MovedBaronAfterRollingSeven != false && playerGameData.PlayedKnightThisTurn) // not eligible to move baron
            {
                return;
            }

            List<Target> targetList = PickBaronVictim(true);
            if (targetList.Count == 0)
            {
                MessageDialog dlg = new MessageDialog("I can't seem to be able to find a good candidate.\nFigure it out yourself.");
                await dlg.ShowAsync();
                return;
            }
            Random rand = new Random((int)DateTime.Now.Ticks);
            int index = rand.Next(targetList.Count);
            Target target = targetList[index];
            CatanAction action = CatanAction.PlayedKnight;
            TargetWeapon weapon = TargetWeapon.Baron;
            if (CurrentGameState == GameState.MustMoveBaron)
            {
                action = CatanAction.AssignedBaron;
            }

            await AssignBaronOrKnight(target.Player, target.Tile, weapon, action, LogType.Normal);
        }

        private async void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            UIElement uiElement = sender as UIElement;
            int zIndex = Canvas.GetZIndex(uiElement);
            if (e.GetCurrentPoint(uiElement).Position.Y < 30)
            {
                Canvas.SetZIndex(uiElement, zIndex + 1000);

                if (sender.GetType() == typeof(Grid))
                {
                    Grid grid = sender as Grid;
                    await StaticHelpers.DragAsync(grid, e);
                }

                await SaveGridLocations();
                Canvas.SetZIndex(uiElement, zIndex);
            }
        }

        // DO NOT call button.IsEnabled = true; -- this is set via notification
        private async void OnRedo(object sender, RoutedEventArgs e)
        {
            if (!MainPageModel.EnableUiInteraction) return;
            await DoRedo();
        }

        private async void OnSaveSettings(object sender, RoutedEventArgs e)
        {
            await SaveGameState();
        }

        private async void OnSetDefaultState(object sender, RoutedEventArgs e)
        {
            if (SaveFolder == null)
            {
                SaveFolder = await StaticHelpers.GetSaveFolder();
            }
            StorageFile file = await SaveFolder.GetFileAsync(PlayerDataFile);
            await file.DeleteAsync(StorageDeleteOption.Default);
            await LoadMainPageModel();
        }

        private async void OnSetUser(object sender, RoutedEventArgs e)
        {
            await PickDefaultUser();
        }


        // DO NOT call button.IsEnabled = true; -- this is set via notification
        private async void OnUndo(object sender, RoutedEventArgs e)
        {
            if (!MainPageModel.EnableUiInteraction) return;
            await DoUndo();
        }

        private async void OnViewSettings(object sender, RoutedEventArgs e)
        {
            _initializeSettings = true;
            if (TheHuman.PlayerName == "")
            {
                await PickDefaultUser();
            }
            SettingsDlg dlg = new SettingsDlg(MainPageModel.Settings, TheHuman);
            _initializeSettings = false;
            await dlg.ShowAsync();
        }

        private async void OnWebSocketConnect(object sender, RoutedEventArgs e)
        {
            await WsConnect();
        }



        /// <summary>
        ///     Go through all the tiles and decide if it is a potential baron victim.
        ///     rules:
        ///         1. can't have any of CurrentPlayer's building on it
        ///         2. can't be empty
        /// </summary>
        /// <returns> a List that has the player/tile options</returns>
        private List<Target> PickBaronVictim(bool weighCities)
        {
            var r = new Random((int)DateTime.Now.Ticks);
            int tileCount = _gameView.TilesInIndexOrder.Length;
            List<Target> targetList = new List<Target>();
            foreach (var targetTile in _gameView.TilesInIndexOrder)
            {
                if (targetTile.HasBaron)
                {
                    continue;
                }

                bool impactsCurrentPlayer = false;
                foreach (var building in targetTile.OwnedBuilding)
                {
                    //  got to go through all of them because you don't want
                    //  to pick a tile that impacts the CurrentPlayer

                    if (building.Owner == CurrentPlayer)
                    {
                        impactsCurrentPlayer = true;
                        break;
                    }
                }

                if (!impactsCurrentPlayer && targetTile.OwnedBuilding.Count > 0)
                {
                    //
                    //  now go through and add them to the list
                    foreach (var building in targetTile.OwnedBuilding)
                    {
                        targetList.Add(new Target(building.Owner, targetTile));
                        if (weighCities && building.BuildingState == BuildingState.City)
                        {
                            //
                            //  cities are worth twice as much, so put them in twice to make them more likely to be picked
                            targetList.Add(new Target(building.Owner, targetTile));
                        }
                    }
                }
            }

            return targetList;
        }

        private async Task PickDefaultUser()
        {
            var picker = new PlayerPickerDlg(MainPageModel.AllPlayers);
            _ = await picker.ShowAsync();
            if (picker.Player == null)
            {
                await StaticHelpers.ShowErrorText("You have to pick a player!  Stop messing around Dodgy!");
                return;
            }

            TheHuman = picker.Player;
            if (!ValidateBuilding && MainPageModel.PlayingPlayers.Count == 1)
            {
                CurrentPlayer = TheHuman;  //  this is useful for debugging
            }
            MainPageModel.TheHuman = TheHuman.PlayerName;
            MainPageModel.DefaultUser = TheHuman.PlayerName;
            await SaveGameState();
        }

        /// <summary>
        /// this is called when the color menu item is clicked to change the color of the current player
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PlayerPrimaryColor_Clicked(object sender, RoutedEventArgs e)
        {
            if (CurrentPlayer == null)
            {
                return;
            }

            ToggleMenuFlyoutItem item = sender as ToggleMenuFlyoutItem;
            foreach (ToggleMenuFlyoutItem subItem in Menu_Primary_Color.Items)
            {
                if (subItem != item)
                {
                    subItem.IsChecked = false;
                }
            }

            CurrentPlayer.PrimaryBackgroundColor = (Color)item.Tag;

            await SaveGameState();
        }

        private async void PlayerSecondaryColor_Clicked(object sender, RoutedEventArgs e)
        {
            if (CurrentPlayer == null)
            {
                return;
            }

            ToggleMenuFlyoutItem item = sender as ToggleMenuFlyoutItem;
            foreach (ToggleMenuFlyoutItem subItem in Menu_Secondary_Color.Items)
            {
                if (subItem != item)
                {
                    subItem.IsChecked = false;
                }
            }

            CurrentPlayer.SecondaryBackgroundColor = (Color)item.Tag;

            await SaveGameState();
        }

        //
        //  save all the grids.  since I add/delete them frequently, this can throw if/when a grid is removed after it has been saved.
        //  just swallow the exception.
        //
        private async Task SaveGridLocations()
        {
            try
            {
                await SaveGameState();
            }
            catch (Exception e)
            {
                this.TraceMessage($"caught the exception: {e}");
            }
        }

        private void ToggleShowTile(object sender, RoutedEventArgs e)
        {
            ToggleMenuFlyoutItem menu = sender as ToggleMenuFlyoutItem;

            foreach (var tile in _gameView.TilesInIndexOrder)
            {
                tile.ShowIndex = menu.IsChecked;
            }
        }

        private void UpdateGridLocations()
        {
            try
            {
                foreach (var kvp in MainPageModel.Settings.GridPositions)
                {
                    GridPosition pos = kvp.Value;
                    string name = kvp.Key;
                    var ctrl = this.FindName(name);
                    if (ctrl == null) continue; // the dev renamed the control!!
                    if (ctrl.GetType().FullName == "Catan10.GameContainerCtrl") continue;

                    DragableGridCtrl dGrid = ctrl as DragableGridCtrl;
                    if (dGrid != null) dGrid.GridPosition = pos;
                }
            }
            catch (Exception e)
            {
                this.TraceMessage($"Exception: {e}");
            }
        }

        internal void AddPlayerMenu(PlayerModel player)
        {
        }

        public async Task DoUndo()
        {
            if ((CurrentGameState == GameState.WaitingForNewGame || !MainPageModel.EnableUiInteraction) && ValidateBuilding)
            {
                return;
            }
            try
            {
                MainPageModel.EnableUiInteraction = false;
                bool ret = await UndoAsync();
            }
            finally
            {
                MainPageModel.EnableUiInteraction = true;
            }
        }

        /// <summary>
        ///     This is called when the user clicks on the "Next" button.
        ///     its job is to do all of the updates nesessary to move to the next state
        ///
        ///     We need to be identical here and in the IGameController::SetState(SetStateLog log) function in that hitting "next" on this machine
        ///     does the exact same thing to all the other machines in the game.
        ///
        ///     intuition says that this switch should just call a LogHeader static to initiate the action.
        ///
        /// </summary>
        /// <returns></returns>
        public async Task<bool> NextState()
        {
            if (CurrentPlayer == null)
            {
                await OnNewGame();
                return false;
            }
            try
            {
                if (CurrentPlayer.PlayerIdentifier != TheHuman.PlayerIdentifier) return false;

                if (CurrentPlayer.GameData.Resources.UnspentEntitlements.Count > 0) return false;

                // this.TraceMessage("starting NextStep");
                MainPageModel.EnableUiInteraction = false;
                switch (CurrentGameState)
                {
                    case GameState.WaitingForNewGame:
                        OnStartDefaultNetworkGame(null, null);
                        break;

                    case GameState.WaitingForPlayers: // while you are waiting for players you can also select the board
                        await WaitingForPlayersToPickingBoard.PostLog(this);
                        break;

                    case GameState.PickingBoard:  // you get here by clicking the "=>" button
                        await PickingBoardToWaitingForRollOrder.PostLog(this);
                        break;

                    case GameState.WaitingForRollForOrder: // you get here by clicking the "=>" button
                        await WaitingForRollOrderToBeginResourceAllocation.PostLog(this);
                        break;

                    case GameState.BeginResourceAllocation:
                        await BeginAllocationToAllocateResourcesForward.PostLog(this);
                        break;

                    case GameState.AllocateResourceForward:
                        if (MainPageModel.PlayingPlayers.Last().GameData.Score == 1) 
                        {
                            await AllocateResourcesForwardToAllocateResourcesReverse.PostLog(this);
                        }
                        else
                        {
                            await AllocateResourcesForwardToAllocateResourcesForward.PostLog(this);
                        }
                        break;

                    case GameState.AllocateResourceReverse:

                        int players = MainPageModel.PlayingPlayers.IndexOf(CurrentPlayer) - 1;

                        if (MainPageModel.PlayingPlayers[0].GameData.Score == 2)
                        {
                            await AllocateResourcesReverseToDoneAllocResources.PostLog(this);
                        }
                        else
                        {
                            await AllocateResourcesReverseToAllocateResourcesReverse.PostLog(this);
                        }

                        break;

                    case GameState.DoneResourceAllocation:
                        await DoneAllocResourcesToWaitingForRoll.PostLog(this);
                        break;

                    case GameState.WaitingForRoll:
                        //
                        //  this is called in MainPage.OnRolled
                        break;

                    case GameState.Targeted:
                        break;

                    case GameState.LostToCardsLikeMonopoly:
                        break;

                    case GameState.Supplemental:
                        break;

                    case GameState.DoneSupplemental:
                        break;

                    case GameState.WaitingForNext:
                        break;

                    case GameState.LostCardsToSeven:
                        break;

                    case GameState.MissedOpportunity:
                        break;

                    case GameState.GamePicked:
                        break;

                    case GameState.MustMoveBaron:
                        break;

                    case GameState.Unknown:
                        break;

                    //
                    //  these don't ever get called when Next is hit
                    case GameState.Uninitialized:
                    default:
                        Contract.Assert(false, "Next should not have been anabled for this state!");
                        break;
                }

                return true;
            }
            finally
            {
                MainPageModel.EnableUiInteraction = true;
            }
        }
       
    }
}