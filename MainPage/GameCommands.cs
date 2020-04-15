using System;

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using static Catan10.StaticHelpers;

namespace Catan10
{

    public class GridPosition
    {
        public string Name { get; set; } = "";
        public double TranslateX { get; set; } = 0.0;
        public double TranslateY { get; set; } = 0.0;
        public GridPosition(string n, double X, double Y)
        {
            Name = n;
            TranslateX = X;
            TranslateY = Y;
        }
        public GridPosition(string s)
        {
            Deserialize(s);
        }

        public override string ToString()
        {
            return Serialize();
        }

        public string Serialize()
        {
            return string.Format($"{Name}={TranslateX},{TranslateY}");
        }

        public void Deserialize(string s)
        {
            var kvp = StaticHelpers.GetKeyValue(s);
            Name = kvp.Key;
            string[] tokens = kvp.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            TranslateX = double.Parse(tokens[0]);
            TranslateY = double.Parse(tokens[1]);


        }
    }

    public sealed partial class MainPage : Page
    {
        private Dictionary<string, string> _defaultUsers = new Dictionary<string, string>()
        {
            {"Joe", "joe.jpg;Blue" },
            {"Dodgy", "Dodgy.jpg;Red" },
            {"Doug", "doug.jpg;White" },
            {"Robert", "robert.jpg;Black" },
            {"Chris", "chris.jpg;Yellow" },
            {"Cort", "cort.jpg;Green" },
            {"Craig", "craig.jpg;DarkGray" },
            {"John", "john.jpg;Brown" }

        };

        //
        //  this is the name of the grids in MainPage.xaml that we want to store and retrieve locations
        private string[] GridPositionName = new string[] { "RollGrid", "ControlGrid", "_savedGameGrid", "_gameView" };

        //
        //  this just creates our saved file
        private async Task AddDefaultUsers()
        {
            List<PlayerModel> list = new List<PlayerModel>();
            foreach (KeyValuePair<string, string> kvp in _defaultUsers)
            {
                PlayerModel p = new PlayerModel(this)
                {
                    PlayerName = kvp.Key

                };
                string[] tokens = kvp.Value.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                string bitmapPath = string.Format($"ms-appx:Assets/DefaultPlayers/{tokens[0]}");
                p.ColorAsString = tokens[1];
                p.ImageFileName = bitmapPath;
                await p.LoadImage();
                list.Add(p);


            }
            await MainPage.SavePlayers(list, MainPage.PlayerDataFile);

        }

        private async void OnAddDefaultUsers(object sender, RoutedEventArgs e)
        {
            await AddDefaultUsers();
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



        //
        //  save all the grids.  since I add/delete them frequently, this can throw if/when a grid is removed after it has been saved.
        //  just swallow the exception.
        //
        private async Task SaveGridLocations()
        {
            try
            {
                _settings.GridPositions.Clear();
                foreach (string name in GridPositionName)
                {
                    UIElement el = (UIElement)this.FindName(name);
                    CompositeTransform ct = (CompositeTransform)el.RenderTransform;
                    GridPosition pos = new GridPosition(name, ct.TranslateX, ct.TranslateY);
                    _settings.GridPositions.Add(pos);
                }

                await _settings.SaveSettings(_settingsFileName);
            }
            catch (Exception e)
            {
                this.TraceMessage($"caught the exception: {e}");
            }
        }

        private void UpdateGridLocations()
        {
            try
            {


                foreach (GridPosition pos in _settings.GridPositions)
                {
                    UIElement el = (UIElement)this.FindName(pos.Name);
                    CompositeTransform ct = (CompositeTransform)el.RenderTransform;
                    ct.TranslateX = pos.TranslateX;
                    ct.TranslateY = pos.TranslateY;
                }
            }
            catch (Exception e)
            {
                this.TraceMessage($"Exception: {e}");
            }
        }

        private void Border_PointerEntered(object sender, PointerRoutedEventArgs e)
        {

            Border border = sender as Border;

            Point position = e.GetCurrentPoint(border).Position;
            double grabSize = 5;
            bool mouseCaptured = false;
            PointerEventHandler pointerMoved = null;
            PointerEventHandler pointerExited = null;
            PointerEventHandler pointerPressed = null;
            PointerEventHandler pointerReleased = null;
            bool sizeableX = false;
            bool sizeableY = false;
            double originalWidth = border.Width;
            double originalHeight = border.Height;
            Point pointMouseDown = position;
            CompositeTransform ct = border.RenderTransform as CompositeTransform;


            pointerPressed = (object s, PointerRoutedEventArgs eMove) =>
            {
                this.TraceMessage("PointerPressed");
                border.PointerReleased += pointerReleased;
                if (sizeableX == true)
                {
                    mouseCaptured = true;
                    border.CapturePointer(e.Pointer);
                    this.TraceMessage("resize X");
                }
                if (sizeableY == true)
                {
                    mouseCaptured = true;
                    border.CapturePointer(e.Pointer);
                    this.TraceMessage("resize Y");
                }
            };

            pointerReleased = (object s, PointerRoutedEventArgs eMove) =>
            {
                this.TraceMessage($"PointerReleased");
                border.PointerPressed -= pointerPressed;
                border.PointerReleased -= pointerReleased;
                border.PointerMoved -= pointerMoved;
                if (mouseCaptured)
                {
                    border.ReleasePointerCapture(e.Pointer);
                }
            };

            pointerMoved = (object s, PointerRoutedEventArgs eMove) =>
            {

                position = eMove.GetCurrentPoint(border).Position;
                if (mouseCaptured)
                {
                    double ratioX = position.X / originalWidth;
                    double ratioY = position.Y / originalHeight;
                    this.TraceMessage($"pointerMoved: {position} RatioX:{ratioX} RatioY:{ratioY}");

                    //
                    //  find how much the mouse has moved and resize the window as appropriate..I think this should be a trasnform, not a width

                    //
                    //  this is the money clause -- resize the window!
                    if (sizeableX)
                    {
                        if (ratioX > .5)
                        {
                            ct.ScaleX = ratioX;
                        }
                    }
                    if (sizeableY)
                    {
                        if (ratioY > .5)
                        {
                            ct.ScaleY = ratioY;
                        }
                    }

                    return;
                }

                if (position.Y < grabSize || position.Y > border.Height - grabSize)
                {
                    sizeableY = true;
                    sizeableX = false;
                    Window.Current.CoreWindow.PointerCursor =
                        new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.SizeNorthSouth, 1);
                }
                else if (position.X < grabSize || position.X > border.Width - grabSize)
                {
                    sizeableX = true;
                    sizeableY = false;
                    Window.Current.CoreWindow.PointerCursor =
                        new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.SizeWestEast, 1);
                }

                else
                {
                    sizeableX = false;
                    sizeableY = false;
                    Window.Current.CoreWindow.PointerCursor =
                                  new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
                }


            };

            pointerExited = (s, eExit) =>
            {
                this.TraceMessage($"pointerMoved");
                Window.Current.CoreWindow.PointerCursor =
                    new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 1);
                border.PointerMoved -= pointerMoved;
                border.PointerExited -= pointerExited;
                border.PointerPressed -= pointerPressed;
            };

            border.PointerMoved += pointerMoved;
            border.PointerExited += pointerExited;
            border.PointerPressed += pointerPressed;

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
        private void OnNextStep(object sender, RoutedEventArgs e)
        {
            
            if (!MainPageModel.EnableUiInteraction)
            {
                this.TraceMessage("rejecting call to OnNextStep");
                return;
            }
            MainPageModel.EnableUiInteraction = false;            
            NextState().ContinueWith((b) =>
               {                   
                       //
                       //   need to switch back to the UI thread
                       _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                     {


                         MainPageModel.EnableUiInteraction = true;

                     });

               });
        }

        public async Task<bool> NextState()
        {
            if (CurrentPlayer == null)
            {
                await OnNewGame();
                return false;
            }

            await ProcessEnter(CurrentPlayer, "");
            return true;
        }

        
        private void OnUndo(object sender, RoutedEventArgs e)
        {
           if (GameState == GameState.WaitingForNewGame || !MainPageModel.EnableUiInteraction)
            {
                return;
            }
            MainPageModel.EnableUiInteraction = false;
            
            OnUndo(true).ContinueWith( (b) =>
            {
                _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    MainPageModel.EnableUiInteraction = true;

                });
            });
            
        }
        private void OnRedo(object sender, RoutedEventArgs e)
        {
            if (GameState == GameState.WaitingForNewGame || !MainPageModel.EnableUiInteraction)
            {
                return;
            }
            MainPageModel.EnableUiInteraction = false;
           
            OnRedo(true).ContinueWith((o) =>
            {
                _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // DO NOT call button.IsEnabled = true; -- this is set
                    // via notification from the log on the undo stack depth

                    MainPageModel.EnableUiInteraction = true;

                });
            });

        }

        private async void OnNewGame(object sender, RoutedEventArgs e)
        {
            await OnNewGame();
        }

        private async void OnViewSettings(object sender, RoutedEventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            _initializeSettings = true;
            SettingsDlg dlg = new SettingsDlg(this, _settings);
            _initializeSettings = false;
            await dlg.ShowAsync();
            ((Button)sender).IsEnabled = true;
        }

        /// <summary>
        ///     we stopped tracking Winner as that drove conflict when everybody forgot to set the winner...
        /// </summary
        private async void OnWinnerClicked(object sender, RoutedEventArgs e)
        {
            await OnWin();
        }

        private async void OnNumberTapped(object sender, TappedRoutedEventArgs e)
        {
            Button button = sender as Button;
            CatanNumber number = button.Content as CatanNumber;

            if (this.State.GameState != GameState.WaitingForRoll)
            {
                HideNumberUi();
                return;
            }

            await ProcessRoll(number.Number);


        }


        private void OnShowMenu(object sender, RoutedEventArgs e)
        {

        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            _contextMenu.Hide();
        }
        private void OnMenuOpened(object sender, object e)
        {

        }

        private void OnMenuClosed(object sender, object e)
        {

        }



        private async void Menu_OnNewGame(object sender, RoutedEventArgs e)
        {
            // await OnNewGame?.Invoke(this, EventArgs.Empty);
            await OnNewGame();
        }

        private async void Menu_ShowSavedGames(object sender, RoutedEventArgs e)
        {
            await LoadSavedGames(); // this will load the saved games into the UI control
            _savedGameGrid.Visibility = Visibility.Visible;

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

            await ChangeGame(item.Tag as CatanGame);

        }

        private async void OnUndoClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                ((MenuFlyoutItem)sender).IsEnabled = false;
                MenuFlyoutItem itm = sender as MenuFlyoutItem;
                await OnUndo();
            }
            finally
            {
                ((MenuFlyoutItem)sender).IsEnabled = true;
            }
        }




        private void CreateMenuItems()
        {
            if (StaticHelpers.IsInVisualStudioDesignMode)
            {
                return;
            }

            Menu_Games.Items.Clear();




            //
            //  The Games Menu has the Description as the menu text ("Regular" should be checked)
            //  and the CatanGame object in the tag.  this is used to manipulate CurrentGame in the GameContainer
            //
            List<CatanGame> availableGames = _gameView.Games;
            foreach (CatanGame game in availableGames)
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


            foreach (KeyValuePair<string, Windows.UI.Color> kvp in StaticHelpers.StringToColorDictionary)
            {
                ToggleMenuFlyoutItem item = new ToggleMenuFlyoutItem
                {
                    Text = kvp.Key,
                    Tag = kvp.Value
                };
                item.Click += PlayerColor_Clicked;
                Menu_Colors.Items.Add(item);
            }

        }

        /// <summary>
        /// this is called when the color menu item is clicked to change the color of the current player
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PlayerColor_Clicked(object sender, RoutedEventArgs e)
        {
            if (CurrentPlayer == null)
            {
                return;
            }

            ToggleMenuFlyoutItem item = sender as ToggleMenuFlyoutItem;
            foreach (ToggleMenuFlyoutItem subItem in Menu_Colors.Items)
            {
                if (subItem != item)
                {
                    subItem.IsChecked = false;
                }
            }

            CurrentPlayer.ColorAsString = item.Text;

            //
            //  this is only needed because Roads don't do proper data binding yet.
            CurrentPlayerColorChanged(CurrentPlayer);

            await SavePlayers(AllPlayers, PlayerDataFile); 
        }

        internal void AddPlayerMenu(PlayerModel player)
        {



        }

        private void ToggleShowTile(object sender, RoutedEventArgs e)
        {
            ToggleMenuFlyoutItem menu = sender as ToggleMenuFlyoutItem;


            foreach (var tile in _gameView.TilesInIndexOrder)
            {
                tile.ShowIndex = menu.IsChecked;
            }
        }

        /// <summary>
        ///     tries to be smart about where to place the baron
        ///     1. if Dodgy is playing (since he's always Red...) be sure and put it on him
        ///     2. pick the one with the most resource generating potential
        ///     3. if the highscore less than 5, try to block brick
        ///     5. if the highscore >=5, try to block Ore
        /// </summary>

        private async void OnPickOptimalBaron(object sender, RoutedEventArgs e)
        {

            if (GameState != GameState.WaitingForRoll && GameState != GameState.WaitingForNext && GameState != GameState.MustMoveBaron)
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
            foreach (var p in AllPlayers)
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
            if (GameState == GameState.MustMoveBaron)
            {
                action = CatanAction.AssignedBaron;
            }

            await AssignBaronOrKnight(target.Player, target.Tile, weapon, action, LogType.Normal);
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
        private async void OnChangeRandomGoldTileCount(object sender, RoutedEventArgs e)
        {
            var goldTileCount = await StaticHelpers.GetUserString("How Many Gold Tiles?", RandomGoldTileCount.ToString());
            if (Int32.TryParse(goldTileCount, out int newVal))
            {
                RandomGoldTileCount = newVal;
                // not setting the gold tiles to be the new value for now becaues of Undo problems...instead hit Next and then Undo
            }
        }
        private void OnShowRolls(object sender, RoutedEventArgs e)
        {
            ToggleNumberUi();
        }

        /// <summary>
        ///     Picks a random place for the Baron that
        ///     1. doesn't impact the current player
        ///     2. does impact another player
        ///     3. twice as likely to Block a City vs. Block a settlement
        /// </summary>
        private async void OnPickRandomBaron(object sender, RoutedEventArgs e)
        {
            if (GameState != GameState.WaitingForRoll && GameState != GameState.WaitingForNext && GameState != GameState.MustMoveBaron)
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
            if (GameState == GameState.MustMoveBaron)
            {
                action = CatanAction.AssignedBaron;
            }

            await AssignBaronOrKnight(target.Player, target.Tile, weapon, action, LogType.Normal);


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
    }

    public class Target
    {
        public PlayerModel Player { get; private set; } = null;
        public TileCtrl Tile { get; private set; } = null;
        public int ResourcePotential { get; private set; } = 0;
        public override string ToString()
        {
            return $"{Player,-15} | {Tile,-15} | {ResourcePotential}";
        }
        public Target(PlayerModel p, TileCtrl t)
        {
            Player = p;
            Tile = t;
            foreach (var building in Tile.OwnedBuilding)
            {
                if (building.BuildingState == BuildingState.Settlement)
                {
                    ResourcePotential++;
                }
                else if (building.BuildingState == BuildingState.City)
                {
                    ResourcePotential += 2;
                }
                else
                {
                    throw new Exception("This building shouldn't be owned");
                }

            }

            ResourcePotential *= Tile.Pips;

            if (ResourcePotential > 0)
            {
                //
                //  check to see if this player has 2:1 in a resource from this tile

                foreach (var harbor in Player.GameData.OwnedHarbors)
                {
                    if (StaticHelpers.HarborTypeToResourceType(harbor.HarborType) == Tile.ResourceType)
                    {
                        ResourcePotential *= 2;
                    }
                }
            }




        }


    }
}
