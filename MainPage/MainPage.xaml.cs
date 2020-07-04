using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Catan.Proxy;

using Catan10.Spy;

using Windows.Storage;
using Windows.Storage.Search;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class MainPage : Page
    {
        #region Delegates + Fields + Events + Enums

        public const string PlayerDataFile = "catansettings.json";

        // used to calculate longest road -- whoever gets their first wins LR, and it has to work if an Undo action ahppanes
        //  State for MainPage -- the thought was to move all save/load state into one place...but that work hasn't finished
        public static readonly DependencyProperty MainPageModelProperty = DependencyProperty.Register("MainPageModel", typeof(MainPageModel), typeof(MainPage), new PropertyMetadata(new MainPageModel(), MainPageModelChanged));

        public static readonly string SAVED_GAME_EXTENSION = ".log.json";
        public static readonly DependencyProperty TheHumanProperty = DependencyProperty.Register("TheHuman", typeof(PlayerModel), typeof(MainPage), new PropertyMetadata(null));
        private const int MAX_SAVE_FILES_RETAINED = 5;
        private const int SMALLEST_STATE_COUNT = 8;
        private readonly RoadRaceTracking _raceTracking = null;
        private readonly List<RandomBoardSettings> _randomBoardList = new List<RandomBoardSettings>();
        private bool _doDragDrop = false;
        private DateTime _dt = DateTime.Now;
        private int _randomBoardListIndex = 0;

        /// <summary>
        ///     This will go through all the buildings and find the ones that are
        ///        1. Buildable
        ///        2. in the "none" state (e.g. not already shown in some way)
        ///      and then have them show the PipGroup.
        ///      you have to do this every time because people might have built in locations that change the PipGroup
        /// </summary>
        private int _showPipGroupIndex = 0;

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        // this lets you double tap a map and then move it around
        // the index into PlayingPlayers that is the CurrentPlayer
        public static MainPage Current { get; private set; }

        public static IGameController GameController { get; private set; }

        public GameType GameType
        {
            get => _gameView.CurrentGame.GameType;
            set
            {
            }
        }

        public bool HasSupplementalBuild => GameType == GameType.SupplementalBuildPhase;

        public MainPageModel MainPageModel
        {
            get => (MainPageModel)GetValue(MainPageModelProperty);
            set => SetValue(MainPageModelProperty, value);
        }

        public StorageFolder SaveFolder { get; set; } = null;

        // a global for the game
        public PlayerModel TheHuman
        {
            get => (PlayerModel)GetValue(TheHumanProperty);
            set => SetValue(TheHumanProperty, value);
        }

        private DispatcherTimer SaveSettingsTimer { get; set; }

        #endregion Properties

        #region Constructors + Destructors

        public MainPage()
        {
          //
          //  ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
          //
            this.InitializeComponent();
            Current = this;
            GameController = this;
            this.DataContext = this;
            _raceTracking = new RoadRaceTracking(this);
        }

        #endregion Constructors + Destructors

        #region Methods

        private DispatcherTimer KeepAliveTimer { get; set; }

        public static double GetAnimationSpeed(AnimationSpeed speed)
        {
            double baseSpeed = 2;
            if (Current != null)
            {
                baseSpeed = Current.AnimationSpeedBase;
            }

            if (speed == AnimationSpeed.Ultra)
            {
                return (double)speed;
            }
            // AnimationSpeedFactor is a value of 1...4
            double d = (double)speed / (baseSpeed + 2);
            return d;
        }

        /// <summary>
        /// Update this because you did the sorting work in the dialog
        /// hide all positions and then loop through the array to make them visible
        ///
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<StorageFile>> GetSavedFilesInternal()
        {
            QueryOptions queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, new[] { $"{SAVED_GAME_EXTENSION}" })
            {
                FolderDepth = FolderDepth.Shallow
            };
            StorageFolder folder = await StaticHelpers.GetSaveFolder();
            StorageFileQueryResult query = folder.CreateFileQueryWithOptions(queryOptions);
            IReadOnlyList<StorageFile> files = await query.GetFilesAsync();

            return files;
        }

        public void UpdateBoardMeasurements()
        {
            PipCount = GetPipCount();
            List<BuildingCtrl> buildingsOrderedByPips = new List<BuildingCtrl>(_gameView.CurrentGame.HexPanel.Buildings);
            buildingsOrderedByPips.Sort((s1, s2) => s2.Pips - s1.Pips);
            Dictionary<int, int> pipCountDictionary = new Dictionary<int, int>();
            int[] pipCount = new int[4] { 0, 0, 0, 0 };

            foreach (var building in buildingsOrderedByPips)
            {
                if (13 - building.Pips > pipCount.Length - 1) break;
                pipCount[13 - building.Pips]++;
            }

            MainPageModel.SetPipCount(pipCount);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Ctrl_PlayerResourceCountCtrl.MainPage = this;

            var ignored = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                if (e.NavigationMode == NavigationMode.New)
                {
                    _progress.IsActive = true;
                    _progress.Visibility = Visibility.Visible;

                    _gameView.Init(this, this);
                    CreateMenuItems();
                }

                InitTest();

                await InitializeMainPageModel();

                SaveSettingsTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(5)
                };
                SaveSettingsTimer.Tick += SaveSettingsTimer_Tick;

                base.OnNavigatedTo(e);
            });
        }

        private static void MainPageModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as MainPage;
            var depPropValue = (MainPageModel)e.NewValue;
            depPropClass?.SetMainPageModel((MainPageModel)e.OldValue, (MainPageModel)e.NewValue);
        }

        private Task AddPlayer(PlayerModel pData, LogType logType)
        {
            MainPageModel.PlayingPlayers.Add(pData);

            AddPlayerMenu(pData);
            pData.Reset();
            //
            //  need to give the players some data about the game
            pData.GameData.MaxCities = _gameView.CurrentGame.GameData.MaxCities;
            pData.GameData.MaxRoads = _gameView.CurrentGame.GameData.MaxRoads;
            pData.GameData.MaxSettlements = _gameView.CurrentGame.GameData.MaxSettlements;
            pData.GameData.MaxShips = _gameView.CurrentGame.GameData.MaxShips;

            return Task.CompletedTask;
        }

        private void GameViewControlDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _doDragDrop = _doDragDrop ? false : true;
        }

        private async void GameViewControlPointerPressed(object sender, PointerRoutedEventArgs pRoutedEvents)
        {
            if (_doDragDrop)
            {
                await StaticHelpers.DragAsync((UIElement)sender, pRoutedEvents);
                OnGridPositionChanged("_gameView", new GridPosition(_transformGameView));
            }
        }

        /// <summary>
        ///     return a Dictionary of PipGroup to a List of Building that have that number of Pips
        /// </summary>
        /// <returns></returns>
        private Dictionary<int, List<BuildingCtrl>> GetBuildingByPips()
        {
            List<BuildingCtrl> buildingsOrderedByPips = new List<BuildingCtrl>(_gameView.CurrentGame.HexPanel.Buildings);
            buildingsOrderedByPips.Sort((s1, s2) => s2.Pips - s1.Pips);
            Dictionary<int, List<BuildingCtrl>> dictPipsToBuildings = new Dictionary<int, List<BuildingCtrl>>();
            //
            //  first a map of Pips -> building that have that #of Pips
            foreach (BuildingCtrl building in buildingsOrderedByPips)
            {
                if (!dictPipsToBuildings.TryGetValue(building.Pips, out List<BuildingCtrl> list))
                {
                    list = new List<BuildingCtrl>();
                    dictPipsToBuildings[building.Pips] = list;
                }
                list.Add(building);
            }
            //
            //  we now have a map of Pips to Buildings, but we want PipGroup to building
            Dictionary<int, List<BuildingCtrl>> pipGroupToBuildings = new Dictionary<int, List<BuildingCtrl>>();
            int pipGroup = 0;
            for (int i = 13; i > 0; i--) // can't have >13 pips in Catan
            {
                if (dictPipsToBuildings.TryGetValue(i, out List<BuildingCtrl> list))
                {
                    pipGroupToBuildings[pipGroup] = list;
                    pipGroup++;
                }
            }

            return pipGroupToBuildings;
        }

        private BuildingCtrl GetHighestPipsBuilding()
        {
            Dictionary<int, List<BuildingCtrl>> dictPipsToBuildings = GetBuildingByPips();
            for (int i = 0; i < 14; i++)
            {
                if (dictPipsToBuildings.TryGetValue(i, out var buildings) == true)
                {
                    foreach (var b in buildings)
                    {
                        if (b.BuildingState != BuildingState.Settlement && b.BuildingState != BuildingState.City)
                        {
                            var buildingState = ValidateBuildingLocation(b);
                            if ( buildingState == BuildingState.Pips || buildingState == BuildingState.Build)
                                return b;
                        }
                    }
                }
            }

            return null;
        }

        private TradeResources GetPipCount()
        {
            TradeResources tr = new TradeResources();
            foreach (var tile in _gameView.AllTiles)
            {
                switch (tile.ResourceType)
                {
                    case ResourceType.Sheep:
                        tr.Sheep += tile.Pips;
                        break;

                    case ResourceType.Wood:
                        tr.Wood += tile.Pips;
                        break;

                    case ResourceType.Ore:
                        tr.Ore += tile.Pips;
                        break;

                    case ResourceType.Wheat:
                        tr.Wheat += tile.Pips;
                        break;

                    case ResourceType.Brick:
                        tr.Brick += tile.Pips;
                        break;

                    case ResourceType.GoldMine:
                        break;

                    case ResourceType.Desert:
                        break;

                    case ResourceType.Back:
                        break;

                    case ResourceType.None:
                        break;

                    case ResourceType.Sea:
                        break;

                    default:
                        break;
                }
            }

            return tr;
        }

        private List<TileCtrl> GetTilesWithNumber(int number)
        {
            List<TileCtrl> tilesWithNumber = new List<TileCtrl>();
            foreach (TileCtrl t in _gameView.CurrentGame.Tiles)
            {
                if (t.Number == number)

                {
                    tilesWithNumber.Add(t);
                }
            }
            return tilesWithNumber;
        }

        private async Task GrowOrShrink(UIElement el)
        {
            CompositeTransform ct = el.RenderTransform as CompositeTransform;
            if (ct.ScaleX == .5)
            {
                ct.ScaleX = 1.0;
                ct.ScaleY = 1.0;
            }
            else
            {
                ct.ScaleX = .5;
                ct.ScaleY = .5;
            }

            await SaveGridLocations();
        }

        private async Task HideAllPipEllipses()
        {
            foreach (BuildingCtrl s in _gameView.CurrentGame.HexPanel.Buildings)
            {
                if (s.BuildingState == BuildingState.Pips)
                {
                    await s.UpdateBuildingState(CurrentPlayer, s.BuildingState, BuildingState.None);
                }
            }
        }

        private void KeepAliveTimer_Tick(object sender, object e)
        {
            if (MainPageModel.CatanService != null)
            {
                MainPageModel.CatanService.KeepAlive();
            }
        }

        private async Task LoadMainPageModel()
        {
            try
            {
                this.MainPageModel = await ReadMainPageModelOffDisk();
                if (MainPageModel == null || MainPageModel.AllPlayers == null || MainPageModel.AllPlayers.Count == 0)
                {
                    var list = await GetDefaultUsers();

                    MainPageModel = new MainPageModel()
                    {
                        AllPlayers = list,
                        Settings = new Settings(),
                        TheHuman = "",
                    };

                    await SaveGameState();
                    await ResetGridLayout();
                }

                foreach (var player in MainPageModel.AllPlayers)
                {
                    if (player.PlayerIdentifier == Guid.Empty)
                    {
                        player.PlayerIdentifier = Guid.NewGuid();
                    }

                    await player.LoadImage();

                    if (MainPageModel.TheHuman == player.PlayerName)
                    {
                        TheHuman = player;
                    }
                }

                await MainPageModel.Bank.LoadImage();

                if (MainPageModel.TheHuman == "")
                {
                    await PickDefaultUser();
                    CurrentPlayer = TheHuman; // this means each client will start with CurrentPlayer being themselves so that all UI binding to current player will give decent colors
                }
            }
            catch (FileLoadException fle)
            {
                this.TraceMessage($"FileLoadMessage exception: {fle}");
            }
            catch (Exception e)
            {
                this.TraceMessage($"Generic Exception: {e}");
            }
            finally
            {
            }
        }

        /// <summary>
        ///     Load Data that is global to the game
        ///     1. Players
        ///     2. Settings
        ///     3. Service settings
        /// </summary>
        /// <returns></returns>
        ///
        private async Task LoadSavedGames()
        {
            IReadOnlyList<StorageFile> files = await GetSavedFilesInternal();
            List<StorageFile> fList = new List<StorageFile>();
            fList.AddRange(files);

            IOrderedEnumerable<StorageFile> sort = from s in files orderby s.DateCreated.Ticks descending select s;
            //SavedGames.Clear();
            //foreach (StorageFile f in sort)
            //{
            //    DeprecatedLog log = new DeprecatedLog(f);
            //    SavedGames.Add(log);
            //}
        }

        private async void OnAssignNumbers(object sender, RoutedEventArgs e)
        {
            if (CurrentGameState != GameState.BeginResourceAllocation)
            {
                return;
            }

            bool ret = await StaticHelpers.AskUserYesNoQuestion(string.Format($"Are you sure?  This will likely offend some people and annoy others."), "Yes", "No");
            if (!ret)
            {
                return;
            }

            await _gameView.SetRandomCatanBoard(true);
            _randomBoardList.Add(_gameView.RandomBoardSettings);
        }

        private async void OnClearPips(object sender, RoutedEventArgs e)
        {
            await HideAllPipEllipses();
            _showPipGroupIndex = 0;
        }

        private void OnCloseSavedGames(object sender, RoutedEventArgs e)
        {
            _savedGameGrid.Visibility = Visibility.Collapsed;
        }

        private void OnGameViewControlTapped(object sender, TappedRoutedEventArgs e)
        {
            //   Pointer p = await StaticHelpers.DragAsync((UIElement)sender, e);
        }

        /// <summary>
        ///     The DraggableGridCtrl calls this whenever the window is moved or resized - the data should already
        ///     be up to date in the model via data binding.
        /// </summary>
        /// <param name="gridPosition"></param>
        private async void OnGridPositionChanged(string name, GridPosition gridPosition)
        {
            // this.TraceMessage($"name={name}={MainPageModel.Settings.GridPositions[name]}");
            if (!MainPageModel.Settings.GridPositions.TryGetValue(name, out GridPosition value))
            {
                MainPageModel.Settings.GridPositions[name] = gridPosition;
            }
            await SaveGameState();
        }

        private async void OnGrowOrShrinkControls(object sender, RoutedEventArgs e)
        {
            await GrowOrShrink(ControlGrid);
        }

        private async void OnManagePlayers(object sender, RoutedEventArgs e)
        {
            PlayerManagementDlg dlg = new PlayerManagementDlg(MainPageModel.AllPlayers);
            if (await dlg.ShowAsync() == ContentDialogResult.Primary)
            {
                MainPageModel.AllPlayers.Clear();
                MainPageModel.AllPlayers.AddRange(dlg.PlayerDataList);
                await SaveGameState();
            }
        }

        private void OnNumberTapped(object sender, TappedRoutedEventArgs e)
        {
            if (((Button)sender).Content is CatanNumber number)
            {
                List<RollModel> rolls = new List<RollModel>();
                for (int i = 0; i < 4; i++)
                {
                    var rollModel = new RollModel
                    {
                        DiceOne = number.Number / 2,
                        Selected = false
                    };
                    rollModel.DiceTwo = number.Number - rollModel.DiceOne;
                    rolls.Add(rollModel);
                }
                rolls[0].Selected = true;
                OnRolled(rolls);
            }
        }

        private async Task OnOpenSavedGame()
        {
            _stopWatchForTurn.StopTimer();
            OpenGameDlg dlg = new OpenGameDlg();
            await dlg.LoadGames();
            ContentDialogResult ret = await dlg.ShowAsync();

            if (ret == ContentDialogResult.Primary)
            {
                string savedGame = dlg.SavedGame;
                //  await this.ParseAndLoadGame(savedGame);
            }
        }

        private void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
           // ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
        }

        /// <summary>
        ///     When the user clicks on the PlayerRollCtrl, it comes here.  We need to call the appropriate Log object
        ///     based on the state to coordinate the UI updaes
        /// </summary>
        /// <param name="rolls"></param>
        private async void OnRolled(List<RollModel> rolls)
        {
            if (!MainPageModel.EnableRolls) return;

            switch (CurrentGameState)
            {
                case GameState.WaitingForRollForOrder:
                    await RollOrderLog.PostMessage(this, rolls);
                    break;

                case GameState.WaitingForRoll:
                    await WaitingForRollToWaitingForNext.PostRollMessage(this, rolls);
                    break;

                default:
                    break;
            }
        }

        private async void OnScrollMouseWheel(object sender, PointerRoutedEventArgs e)
        {
            DateTime dt = DateTime.Now;
            TimeSpan diff = DateTime.Now - _dt;
            if (diff.TotalSeconds < 1.0)
            {
                ElementSoundPlayer.State = ElementSoundPlayerState.On;
                ElementSoundPlayer.Play(ElementSoundKind.Show);
                ElementSoundPlayer.State = ElementSoundPlayerState.Off;
                return;
            }

            _dt = dt;

            if (MainPageModel.IsServiceGame && CurrentGameState == GameState.PickingBoard && e.KeyModifiers == VirtualKeyModifiers.None)
            {
                await ScrollMouseWheelInServiceGame(e);
                return;
            }

            int showPipGroupIndex = e.GetCurrentPoint(this).Properties.MouseWheelDelta;

            if (showPipGroupIndex >= 0)
            {
                showPipGroupIndex = 1;
            }
            else
            {
                showPipGroupIndex = -1;
            }

            _showPipGroupIndex += showPipGroupIndex;

            if (_showPipGroupIndex <= 0)
            {
                _showPipGroupIndex = 0;
            }
            Dictionary<int, List<BuildingCtrl>> dictPipsToBuildings = GetBuildingByPips();
            if (showPipGroupIndex < 0)
            {
                //
                //  if we went "down" turn off Pips on the last group we showed
                for (int hideIndex = _showPipGroupIndex; hideIndex < dictPipsToBuildings.Count; hideIndex++)
                {
                    List<BuildingCtrl> list = dictPipsToBuildings[hideIndex];
                    if (list.Count == 0)
                    {
                        Debug.WriteLine($"{hideIndex} doesn't have any buildings");
                    }
                    foreach (BuildingCtrl building in list)
                    {
                        if (building.BuildingState == BuildingState.Pips)
                        {
                            await building.UpdateBuildingState(CurrentPlayer, building.BuildingState, BuildingState.None);
                        }
                    }
                }

                return;
            }

            if (_showPipGroupIndex > dictPipsToBuildings.Count - 1)
            {
                _showPipGroupIndex = dictPipsToBuildings.Count - 1;
            }

            for (int i = 0; i < _showPipGroupIndex; i++)
            {
                List<BuildingCtrl> list = dictPipsToBuildings[i];
                if (list.Count == 0)
                {
                    Debug.WriteLine($"going up and no buildigs for {i}");
                }
                foreach (BuildingCtrl building in list)
                {
                    if (building.Pips == 0)  // throw out the ones that have no pips
                    {
                        Debug.WriteLine("Buildings.pips == 0");
                        building.PipGroup = -1;
                        continue; // outside the main map or a desert next to nothing
                    }
                    BuildingState bState = ValidateBuildingLocation(building);
                    if (bState == BuildingState.Error) // throw out the ones you can't build in
                    {
                        continue;
                    }

                    if (building.BuildingState != BuildingState.None)
                    {
                        continue;  // throw out the non-empty ones
                    }

                    building.PipGroup = i;
                    await building.UpdateBuildingState(CurrentPlayer, building.BuildingState, BuildingState.Pips);
                }
            }
        }

        private async void OnShowAllRolls(List<RollModel> rolls)
        {
            await ShowAllRollsLog.Post(this, rolls);
        }

        private void OnShowPips(object sender, RoutedEventArgs e)
        {
        }

        private async Task OnWin()
        {
            bool ret = await StaticHelpers.AskUserYesNoQuestion(string.Format($"Did {CurrentPlayer.PlayerName} really win?"), "Yes", "No");
            if (ret == true)
            {
                try
                {
                    await PlayerWon();
                    // await SetStateAsync(State.PlayerData, GameState.WaitingForNewGame, true);
                }
                catch (Exception e)
                {
                    MessageDialog dlg = new MessageDialog(string.Format($"Error in OnWin\n{e.Message}"));
                    await dlg.ShowAsync();
                }
            }
        }

        private async void PickAGoodBoard(PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.MouseWheelDelta >= 0)
            {
                _randomBoardListIndex++;
                if (_randomBoardListIndex >= _randomBoardList.Count)
                {
                    //
                    //  get new ones
                    await _gameView.SetRandomCatanBoard(true);

                    //
                    //  save the existing settings
                    _randomBoardList.Add(_gameView.RandomBoardSettings);
                    _randomBoardListIndex = _randomBoardList.Count - 1;
                }
            }
            else
            {
                //wants to see what we had before
                _randomBoardListIndex--;
                if (_randomBoardListIndex < 0)
                {
                    _randomBoardListIndex = 0;
                }
            }

            await _gameView.SetRandomCatanBoard(true, _randomBoardList[_randomBoardListIndex]);
        }

        public async Task PickSettlementsAndRoads()
        {
            if (CurrentGameState == GameState.AllocateResourceForward || CurrentGameState == GameState.AllocateResourceReverse)
            {
                await AutoSetBuildingAndRoad();
                await NextState();
               
            }
        }

        private async void PickSettlementsAndRoads(object sender, RoutedEventArgs e)
        {
            await PickSettlementsAndRoads();
        }

        private async Task<MainPageModel> ReadMainPageModelOffDisk()
        {
            if (SaveFolder == null)
            {
                SaveFolder = await StaticHelpers.GetSaveFolder();
            }

            string content = await StaticHelpers.ReadWholeFile(SaveFolder, PlayerDataFile);
            MainPageModel mainPageModel;

            if (String.IsNullOrEmpty(content))
            {
                return null;
            }
            try
            {
                mainPageModel = JsonSerializer.Deserialize<MainPageModel>(content);
                if (mainPageModel == null) mainPageModel = new MainPageModel();
                mainPageModel.GameController = this;
                mainPageModel.Log = new Log(this);

                if (mainPageModel.Settings.GridPositions.Count == 0)
                {
                    foreach (FrameworkElement child in ContentRoot.Children)
                    {
                        if (child.GetType() == typeof(DragableGridCtrl))
                        {
                            DragableGridCtrl ctrl = child as DragableGridCtrl;
                            mainPageModel.Settings.GridPositions[child.Name] = ctrl.GridPosition; // the default
                        }
                    }
                }

                return mainPageModel;
            }
            catch (JsonException j)
            {
                this.TraceMessage($"JSON error: {j}");
            }

            return null;
        }

        private async Task Reset()
        {
            if (MainPageModel?.Log != null)
            {
                MainPageModel.Log.Dispose();
            }
            while (MainPageModel.PlayingPlayers.Count > 0)
            {
                MainPageModel.PlayingPlayers.RemoveAt(0); // the clear doesn't trigger the unsubscribe because the NewItems and the OldItems are both null
            }

            MainPageModel.Log = new Log(this);

            await ResetTiles(true);

            foreach (PlayerModel player in MainPageModel.AllPlayers)
            {
                player.Reset();
            }

            _raceTracking.Reset();
        }

        private async Task ResetTiles(bool bMakeFaceDown)
        {
            //
            //  this should mean that we are launching for the first time
            if (_gameView.CurrentGame.Tiles[0].TileOrientation == TileOrientation.FaceDown)
            {
                return;
            }

            List<Task> tasks = new List<Task>();
            foreach (TileCtrl t in _gameView.CurrentGame.Tiles)
            {
                t.AnimateFadeAsync(1.0);
                t.Rotate(0, tasks, false);
            }
            await Task.WhenAll(tasks.ToArray());
            foreach (TileCtrl t in _gameView.CurrentGame.Tiles)
            {
                if (bMakeFaceDown)
                {
                    t.SetTileOrientation(TileOrientation.FaceDown, tasks, MainPage.GetAnimationSpeed(AnimationSpeed.SuperFast));
                }
            }
            await Task.WhenAll(tasks.ToArray());
        }

        private Task SaveGameState()
        {
            if (SaveSettingsTimer == null) return Task.CompletedTask;
            if (SaveSettingsTimer.IsEnabled) return Task.CompletedTask;
            SaveSettingsTimer.Start();
            return Task.CompletedTask;
        }

        /// <summary>
        ///     a Dispatch timer callback that saves the Game's players and settings.
        ///     The way it works is that whenever a setting changes, the Timer starts.
        ///     after 5 seconds, we write the file to the disk.  we only write every 5
        ///     seconds.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SaveSettingsTimer_Tick(object sender, object obj)
        {
            int count = 0;
            SaveSettingsTimer.Stop();
            try
            {

                //
                //  on occasion, i've had to rename grids in mainpage -- but their position might be in the saved file.
                //  this will remove the ones that don't exist anymore
                List<string> badGridNames = new List<string>();
                foreach (var name in MainPageModel.Settings.GridPositions.Keys)
                {
                    var ctrl = this.FindName(name);
                    if (ctrl == null)
                    {
                        badGridNames.Add(name);
                    }
                }
                badGridNames.ForEach((name) => MainPageModel.Settings.GridPositions.Remove(name));

                StorageFolder folder = await StaticHelpers.GetSaveFolder();
                var content = CatanSignalRClient.Serialize<MainPageModel>(MainPageModel, true);
                StorageFile file = await folder.CreateFileAsync(PlayerDataFile, CreationCollisionOption.ReplaceExisting);
                Debug.Assert(content.Length > 100);

                do
                {
                    count++;
                    await FileIO.WriteTextAsync(file, content);
                    await Task.Delay(50);
                    string verify = await FileIO.ReadTextAsync(file);
                    if (verify != content)
                    {
                        await Task.Delay(100);
                        if (count == 1)
                        {
                            Debugger.Break();
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                while (count < 5);
            }
            catch (Exception e)
            {
                this.TraceMessage($"eating exception: {e}");
            }
            finally
            {
                this.TraceMessage($"Saved Settings file. saved {count} times");
            }
        }

        /// <summary>
        ///     Unlike previsous implementations, the use the Action/Undo stacks in the log to store the random boards.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task ScrollMouseWheelInServiceGame(PointerRoutedEventArgs e)
        {
            if (TheHuman.PlayerName != CurrentPlayer.PlayerName) return;

            if (MainPageModel.Log.GameState == GameState.PickingBoard)
            {
                if (e.GetCurrentPoint(this).Properties.MouseWheelDelta >= 0)
                {
                    _gameView.AllBuildings.ForEach((b) => b.Reset()); // turn off pips
                    if (MainPageModel.Log.CanRedo && (MainPageModel.Log.PeekUndo.Action == CatanAction.RandomizeBoard))
                    {
                        await RedoAsync();
                    }
                    else
                    {
                        await RandomBoardLog.RandomizeBoard(this, 0);
                    }
                }
                else
                {
                    if (MainPageModel.Log.PeekAction.Action == CatanAction.RandomizeBoard)
                    {
                        await UndoAsync();
                    }
                }
            }
        }

        /// <summary>
        ///     Used for debugging/testing -- automatically pick a settlement and road
        /// </summary>
        /// <returns></returns>        
        private async Task AutoSetBuildingAndRoad()
        {
            // pick a tile with the highest pips and put a settlement on it
            var building = GetHighestPipsBuilding();
            await UpdateBuildingLog.UpdateBuildingState(this, building, BuildingState.Settlement);

            // pick a Random Road
            var road = building.AdjacentRoads[testRandom.Next(building.AdjacentRoads.Count)];

            await UpdateRoadLog.SetRoadState(this, road, RoadState.Road, _raceTracking);

        }

        private void SetMainPageModel(MainPageModel oldModel, MainPageModel newModel)
        {
            if (oldModel != null)
            {
                oldModel.Settings.PropertyChanged -= Settings_PropertyChanged;
            }
            if (newModel != null)
            {
                newModel.Settings.PropertyChanged += Settings_PropertyChanged;
            }
        }

        /// <summary>
        ///     When a setting changes, write the changes to disk
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await SaveGameState();

            if (e.PropertyName == "IsLocalGame")
            {

                if (MainPageModel.Settings.IsLocalGame && MainPageModel.CatanService != null)
                {
                    await MainPageModel.CatanService.DisposeAsync();
                    MainPageModel.CatanService = null;
                }
            }

            if (e.PropertyName == "IsServiceGame")
            {

                if (MainPageModel.Settings.IsServiceGame && MainPageModel.CatanService == null)
                {
                    await CreateAndConfigureProxy();
                }
            }

            if (e.PropertyName == "HostName")
            {
                await CreateAndConfigureProxy();
            }
        }

        private async Task VisualShuffle(RandomBoardSettings rbs = null)
        {
            await _gameView.VisualShuffle(rbs);
            _randomBoardList.Clear();
            _randomBoardList.Add(_gameView.RandomBoardSettings);
            _randomBoardListIndex = 0;
        }

        #endregion Methods

        public static readonly DependencyProperty SpyVisibleProperty = DependencyProperty.Register("SpyVisible", typeof(bool), typeof(MainPage), new PropertyMetadata(false));

        public static readonly DependencyProperty TurnedSpyOnProperty = DependencyProperty.Register("TurnedSpyOn", typeof(string), typeof(MainPage), new PropertyMetadata(""));

        private bool _spyWindowOpen = false;

        public bool SpyVisible
        {
            get => (bool)GetValue(SpyVisibleProperty);
            set => SetValue(SpyVisibleProperty, value);
        }

        public string TurnedSpyOn
        {
            get => (string)GetValue(TurnedSpyOnProperty);
            set => SetValue(TurnedSpyOnProperty, value);
        }

        private void Draggable_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = false; // bubble up
            //
            //  set everybody's zIndex
            foreach (var name in MainPageModel.Settings.GridPositions.Keys)
            {
                FrameworkElement ctrl = this.FindName(name) as FrameworkElement;
                if (ctrl != null)
                {
                    Canvas.SetZIndex(ctrl, 10);
                }
            }

            //
            //  boost the one clicked
            Canvas.SetZIndex(((FrameworkElement)sender), 11);
        }

        private async void OnJoinNetworkGame(object sender, RoutedEventArgs e)
        {
            if (MainPageModel.EnableNextButton == false) return;
            var games = await MainPageModel.CatanService.GetAllGames();

            var dlg = new JoinGameDlg(games) { MainPageModel = MainPageModel, Player = TheHuman };
            var ret = await dlg.ShowAsync();
            if (ret == ContentDialogResult.Primary)
            {
                GameInfo joinGame = dlg.GameSelected;
                await MainPageModel.CatanService.JoinGame(joinGame, TheHuman.PlayerName);
            }
        }

        private async void OnShowCatanSpy(object sender, RoutedEventArgs e)
        {
            if (_spyWindowOpen) return;
            _spyWindowOpen = true;
            AppWindow appWindow = await AppWindow.TryCreateAsync();
            Frame appWindowContentFrame = new Frame();
            appWindowContentFrame.Navigate(typeof(CatanSpyPage));
            await CatanSpyLog.SpyOnOff(this, true);
            // Get a reference to the page instance and assign the
            // newly created AppWindow to the MyAppWindow property.

            ElementCompositionPreview.SetAppWindowContent(appWindow, appWindowContentFrame);
            appWindow.Closed += delegate
            {
                _spyWindowOpen = false;
                appWindowContentFrame.Content = null;
                appWindow = null;
            };
            await appWindow.TryShowAsync();
        }

        private bool EnableNextButton(bool enableNextButton, GameState gameState, int unprocessedMessages)
        {
         //   this.TraceMessage($"State={gameState}|Unprocesed={unprocessedMessages}|enableNextButton={enableNextButton}");
            if (unprocessedMessages != 0)
            {
             //   this.TraceMessage($"disabling Next button because UnprocessedMessages={unprocessedMessages}");
                return false;
            }

            bool enable = MainPageModel.EnableNextButton;
            Debug.Assert(enable == enableNextButton);
            return enable;

        }

        private string BuildCaption(string begin, string end)
        {
            if (String.IsNullOrEmpty(end)) return begin;

            return $"{begin} -  {end}";
        }

        private string BuildTurnCaption(string name)
        {
            return $"{name}'s Turn";
        }

        private async void OnNewLocalGame(object sender, RoutedEventArgs e)
        {
            MainPageModel.Settings.IsLocalGame = true;


            if (MainPageModel.GameState != GameState.WaitingForNewGame)
            {
                if (await StaticHelpers.AskUserYesNoQuestion("Start a new game?", "Yes", "No") == false)
                {
                    return;
                }
            }



            NewGameDlg dlg = new NewGameDlg(MainPageModel.AllPlayers, _gameView.Games);

            ContentDialogResult result = await dlg.ShowAsync();
            if ((dlg.PlayingPlayers.Count < 3 || dlg.PlayingPlayers.Count > 6) && result == ContentDialogResult.Primary)
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
                _gameView.Reset();
                await this.Reset();


                _gameView.CurrentGame = dlg.SelectedGame;
                MainPageModel.PlayingPlayers.Clear();
                GameInfo info = new GameInfo()
                {
                    Creator = "Joe",
                    Id = Guid.NewGuid(),
                    Started = false
                };
                await NewGameLog.JoinOrCreateGame(this, info, CatanAction.GameCreated);

                MainPageModel.PlayingPlayers.Clear();

                dlg.PlayingPlayers.ForEach(async (p) =>
              {
                  await AddPlayerLog.AddPlayer(this, p.PlayerName);
              });



            }


        }
    }
}