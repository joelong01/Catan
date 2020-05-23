using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading.Tasks;

using Catan.Proxy;

using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class MainPage : Page, ILog
    {
        private const int MAX_SAVE_FILES_RETAINED = 5;
        private const int SMALLEST_STATE_COUNT = 8;
        private readonly RoadRaceTracking _raceTracking = null;
        private readonly List<RandomBoardSettings> _randomBoardList = new List<RandomBoardSettings>();
        private int _currentPlayerIndex = 0;
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

        private int _supplementalStartIndex = -1;
        private bool _undoingCardLostHack = false;
        private TaskCompletionSource<object> fileGuard = null;
        public const string PlayerDataFile = "catansettings.json";

        // used to calculate longest road -- whoever gets their first wins LR, and it has to work if an Undo action ahppanes
        //  State for MainPage -- the thought was to move all save/load state into one place...but that work hasn't finished
        public static readonly DependencyProperty MainPageModelProperty = DependencyProperty.Register("MainPageModel", typeof(MainPageModel), typeof(MainPage), new PropertyMetadata(new MainPageModel(), MainPageModelChanged));

        private static void MainPageModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as MainPage;
            var depPropValue = (MainPageModel)e.NewValue;
            depPropClass?.SetMainPageModel((MainPageModel)e.OldValue, (MainPageModel)e.NewValue);
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
        }

        public static readonly string SAVED_GAME_EXTENSION = ".log";
        public static readonly DependencyProperty TheHumanProperty = DependencyProperty.Register("TheHuman", typeof(PlayerModel), typeof(MainPage), new PropertyMetadata(null));

        public MainPage()
        {
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            this.InitializeComponent();
            Current = this;
            this.DataContext = this;
            _raceTracking = new RoadRaceTracking(this);
            Ctrl_PlayerResourceCountCtrl.Log = this;
        }

        // this lets you double tap a map and then move it around
        // the index into PlayingPlayers that is the CurrentPlayer
        public static MainPage Current { get; private set; }

        public GameState GameStateFromOldLog => CurrentGameState;

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

        public ObservableCollection<DeprecatedLog> SavedGames { get; set; } = new ObservableCollection<DeprecatedLog>();

        public StorageFolder SaveFolder { get; set; } = null;

        // a global for the game
        public PlayerModel TheHuman
        {
            get => (PlayerModel)GetValue(TheHumanProperty);
            set => SetValue(TheHumanProperty, value);
        }

        private Task AddPlayer(PlayerModel pData, LogType logType)
        {
            MainPageModel.PlayingPlayers.Add(pData);
            pData.GameData.OnCardsLost += OnPlayerLostCards;
            AddPlayerMenu(pData);
            pData.Reset();
            //
            //  need to give the players some data about the game
            pData.GameData.MaxCities = _gameView.CurrentGame.MaxCities;
            pData.GameData.MaxRoads = _gameView.CurrentGame.MaxRoads;
            pData.GameData.MaxSettlements = _gameView.CurrentGame.MaxSettlements;
            pData.GameData.MaxShips = _gameView.CurrentGame.MaxShips;

            //  _playerToResourceCount[pData.PlayerPosition].Visibility = Visibility.Visible;
            // await AddLogEntry(pData, GameState.Starting, CatanAction.AddPlayer, false, logType, MainPageModel.PlayingPlayers.Count, pData.AllPlayerIndex); // can't undo adding players...
            return Task.CompletedTask;
        }

        private async Task AddPlayer(LogEntry le, LogType logType)
        {
            await AddPlayer(le.PlayerData, logType);
        }

        //
        //  this needs to be called *after* the log for the Roll because we need to undo all of these prior to undoing the Roll
        private void AddResourceCountForPlayer(PlayerModel player, ResourceType resource, int count, LogType logType = LogType.Normal)
        {
            int oldValPlayer = player.GameData.PlayerTurnResourceCount.AddResourceCount(resource, count); // update the player
                                                                                                          // int oldValGlobal = Ctrl_PlayerResourceCountCtrl.GlobalResourceCount.AddResourceCount(resource, count); // update for the game

            //
            //  TODO:  log GoodRoll, RollsWithResource and NoResourceCount

            // await AddLogEntry(player, this.GameState, CatanAction.AddResourceCount, false, logType, -1, new LogResourceCount(oldValPlayer, oldValPlayer + count, resource));
        }

        //
        //  these need to be combined together because we need to know what the state is both before and *after* we move -- and
        //  it used to be that we'd move and then set state.  this way we can do the Gold tile(s) correctly when moving to the next
        //  player only gets gold when it is their turn to roll (e.g. not when it is supplemental)
        //
        private async Task ChangePlayerAndSetState(int numberofPositions, GameState newState, LogType logType = LogType.Normal, [CallerFilePath] string filePath = "", [CallerMemberName] string cmn = "", [CallerLineNumber] int lineNumber = 0)
        {
            int from = MainPageModel.PlayingPlayers.IndexOf(CurrentPlayer);
            int to = GetNextPlayerPosition(numberofPositions);
            _currentPlayerIndex = to;
            GameState oldState = GameStateFromOldLog;

            var currentRandomGoldTiles = _gameView.CurrentRandomGoldTiles;
            List<int> newRandomGoldTiles = null;

            // this is the one spot where the CurrentPlayer is changed.  it should update all the bindings
            // the setter will update all the associated state changes that happen when the CurrentPlayer
            // changes

            CurrentPlayer = MainPageModel.PlayingPlayers[_currentPlayerIndex];

            //
            //  in supplemental, we don't show random gold tiles
            if (newState == GameState.Supplemental)
            {
                await _gameView.ResetRandomGoldTiles();
            }

            //
            // when we change player we optionally set tiles to be randomly gold - iff we are moving forward (not undo)
            // we need to check to make sure that we haven't already picked random goal tiles for this particular role.  the scenario is
            // we hit Next and are waiting for a role (and have thus picked random gold tiles) and then hit undo for some reason so that the
            // previous player can finish their turn.  when we hit Next again, we want the same tiles to be chosen to be gold.
            if ((newState == GameState.WaitingForRoll && logType == LogType.Normal) || (newState == GameState.WaitingForNext && logType == LogType.Undo))
            {
                int playerRoll = TotalRolls / MainPageModel.PlayingPlayers.Count;  // integer divide - drops remainder
                if (playerRoll == CurrentPlayer.GameData.GoldRolls.Count)
                {
                    newRandomGoldTiles = GetRandomGoldTiles();
                    foreach (int i in newRandomGoldTiles)
                    {
                        if (_gameView.AllTiles[i].ResourceType == ResourceType.Desert)
                        {
                            this.TraceMessage($"Desert got added to Random Gold tiles! [Player={CurrentPlayer} [PlayerRole={playerRoll}] [OldGoldTiles={StaticHelpers.SerializeList<int>(currentRandomGoldTiles)}] [NewGoldTiles={StaticHelpers.SerializeList<int>(newRandomGoldTiles)}]");
                        }
                    }
                    CurrentPlayer.GameData.GoldRolls.Add(newRandomGoldTiles);
                }
                else
                {
                    Debug.Assert(CurrentPlayer.GameData.GoldRolls.Count > playerRoll);
                    //
                    //  we've already picked the tiles for this roll -- use them
                    newRandomGoldTiles = CurrentPlayer.GameData.GoldRolls[playerRoll];
                }
                // this.TraceMessage($"[Player={CurrentPlayer} [PlayerRole={playerRoll}] [OldGoldTiles={StaticHelpers.SerializeList<int>(currentRandomGoldTiles)}] [NewGoldTiles={StaticHelpers.SerializeList<int>(newRandomGoldTiles)}]");
                await SetRandomTileToGold(newRandomGoldTiles);
            }

            //
            //  we are on the right person, now set the state

            if (logType == LogType.Normal || logType == LogType.Replay)
            {
                await AddLogEntry(CurrentPlayer, newState, CatanAction.ChangePlayerAndSetState, true, logType, -1, new LogChangePlayer(from, to, oldState, currentRandomGoldTiles, newRandomGoldTiles));
            }

            foreach (TileCtrl t in _gameView.AllTiles)
            {
                t.ResetOpacity();
                t.ResetTileRotation();
                t.StopHighlightingTile();
            }
            //
            //  on next, reset the resources for the turn to 0
            foreach (PlayerModel player in MainPageModel.PlayingPlayers)
            {
                player.GameData.PlayerTurnResourceCount.TurnReset();
            }
        }

        private async Task CopyScreenShotToClipboard(FrameworkElement element)
        {
            IRandomAccessStream stream = new InMemoryRandomAccessStream();
            Windows.UI.Xaml.Media.Imaging.RenderTargetBitmap renderTargetBitmap = new Windows.UI.Xaml.Media.Imaging.RenderTargetBitmap();

            await renderTargetBitmap.RenderAsync(element);

            IBuffer pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

            float dpi = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().LogicalDpi;

            BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Ignore,
                (uint)renderTargetBitmap.PixelWidth,
                (uint)renderTargetBitmap.PixelHeight,
                dpi,
                dpi,
                pixelBuffer.ToArray());
            await encoder.FlushAsync();

            DataPackage dp = new DataPackage();
            RandomAccessStreamReference streamReference = RandomAccessStreamReference.CreateFromStream(stream);
            dp.SetBitmap(streamReference);
            Clipboard.SetContent(dp);
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

        //
        //  helper function so I don't have to check the array length each time
        private bool GetAndVerifyNumber(string[] tokens, int index, out int value)
        {
            value = -1;
            if (tokens.Length < index)
            {
                return false;
            }

            return int.TryParse(tokens[index], out value);
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
                            if (ValidateBuildingLocation(b, out bool showError))
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

        //
        //  n is verified by the caller
        //  this adds a number to the list we use to keep track of what is rolled and updates all the statistics
        private async Task HandleNumber(int roll)
        {
            bool ret = this.PushRoll(roll); // only returns false on a number outside the range...
            if (!ret)
            {
                return;
            }

            List<TileCtrl> tilesWithNumber = await HighlightRolledTiles(roll);
            foreach (TileCtrl tile in tilesWithNumber)
            {
                tile.HighlightTile(CurrentPlayer.BackgroundBrush); // shows what was rolled
            }

            if (roll == 7)
            {
                CurrentPlayer.GameData.MovedBaronAfterRollingSeven = false;
                await SetStateAsync(CurrentPlayer, GameState.MustMoveBaron, false);
                foreach (PlayerModel player in MainPageModel.PlayingPlayers)
                {
                    player.GameData.NoResourceCount++;
                }
            }
            else
            {
                CountResourcesForRoll(tilesWithNumber, false);
                CurrentPlayer.GameData.MovedBaronAfterRollingSeven = null;
                await SetStateAsync(CurrentPlayer, GameState.WaitingForNext, false);
            }
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

        /// <summary>
        ///     Load Data that is global to the game
        ///     1. Players
        ///     2. Settings
        ///     3. Service settings
        /// </summary>
        /// <returns></returns>
        ///

        private async Task LoadMainPageModel()
        {
            try
            {
                this.MainPageModel = await ReadMainPageModelOffDisk();
                if (MainPageModel == null || MainPageModel.AllPlayers.Count == 0)
                {
                    var list = await GetDefaultUsers();

                    MainPageModel = new MainPageModel()
                    {
                        AllPlayers = list,
                        Settings = new Settings(),
                        HostName = "http://192.168.1.128:5000",
                        TheHuman = "",
                    };

                    await SaveGameState();
                    MainPageModel = await ReadMainPageModelOffDisk(); // just verifying round trip...
                    Contract.Assert(MainPageModel != null);
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

                if (MainPageModel.TheHuman == "")
                {
                    await PickDefaultUser();
                    CurrentPlayer = TheHuman;
                }
            }
            finally
            {
            }
        }

        private async Task LoadSavedGames()
        {
            IReadOnlyList<StorageFile> files = await GetSavedFilesInternal();
            List<StorageFile> fList = new List<StorageFile>();
            fList.AddRange(files);

            IOrderedEnumerable<StorageFile> sort = from s in files orderby s.DateCreated.Ticks descending select s;
            SavedGames.Clear();
            foreach (StorageFile f in sort)
            {
                DeprecatedLog log = new DeprecatedLog(f);
                SavedGames.Add(log);
            }
        }

        private async Task LogPlayerLostCards(PlayerModel player, int oldVal, int newVal, LogType logType)
        {
            await AddLogEntry(player, GameStateFromOldLog, CatanAction.CardsLost, true, logType, -1, new LogCardsLost(oldVal, newVal));
        }

        private async void OnAssignNumbers(object sender, RoutedEventArgs e)
        {
            if (GameStateFromOldLog != GameState.WaitingForStart)
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

        /// <summary>
        ///     this is called when you change players and you aren't in a state that rolls -- e.g. supplemenatl, the choosing, etc.
        /// </summary>
        /// <param name="playersToMove"></param>
        /// <param name="logType"></param>
        /// <returns></returns>
        private async Task OnNext(int playersToMove = 1, LogType logType = LogType.Normal)
        {
            await AnimatePlayers(playersToMove, logType);

            UpdateGlobalRollStats();
            foreach (TileCtrl t in _gameView.AllTiles)
            {
                t.ResetOpacity();
                t.ResetTileRotation();
            }
            //
            //  on next, reset the resources for the turn to 0
            foreach (PlayerModel player in MainPageModel.PlayingPlayers)
            {
                player.GameData.PlayerTurnResourceCount.TurnReset();
            }
        }

        private void OnNumberTapped(object sender, TappedRoutedEventArgs e)
        {
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

        private async void OnPlayerLostCards(PlayerModel player, int oldVal, int newVal)
        {
            if (_undoingCardLostHack)
            {
                return;
            }

            await LogPlayerLostCards(player, oldVal, newVal, LogType.Normal);
        }

        private void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
        }

        private async void OnRolled(int dice1, int dice2)
        {
            if (!MainPageModel.EnableRolls) return;

            await SynchronizedRollLog.StartSyncronizedRoll(this, dice1, dice2);
        }

        private async void OnScrollMouseWheel(object sender, PointerRoutedEventArgs e)
        {
            DateTime dt = DateTime.Now;
            TimeSpan diff = DateTime.Now - _dt;
            if (diff.TotalSeconds < 0.1)
            {
                //  this.TraceMessage($"Rejecting mousewheel call.  diff: {diff.TotalSeconds}");
                return;
            }

            _dt = dt;

            if (MainPageModel.IsServiceGame && CurrentGameState == GameState.PickingBoard)
            {
                await ScrollMouseWheelInServiceGame(e);
                return;
            }

            if (GameStateFromOldLog == GameState.WaitingForStart)
            {
                PickAGoodBoard(e);
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

                    if (ValidateBuildingLocation(building, out bool showerror) == false) // throw out the ones you can't build in
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

        private async Task PickSettlementsAndRoads()
        {
            while (GameStateFromOldLog == GameState.AllocateResourceForward || GameStateFromOldLog == GameState.AllocateResourceReverse)
            {
                await SetBuildingAndRoad();
                // move to next
                GameState oldState = GameStateFromOldLog;
                await NextState();
                //
                //  we do it this way because the last player goes twice in the same state
                //
                if (oldState == GameState.AllocateResourceForward && GameStateFromOldLog == GameState.AllocateResourceReverse)
                {
                    await SetBuildingAndRoad();
                }
            }
        }

        private async void PickSettlementsAndRoads(object sender, RoutedEventArgs e)
        {
            await PickSettlementsAndRoads();
        }

        private async Task<bool> ProcessRoll(int roll)
        {
            if (roll >= 2 && roll <= 12)
            {
                await HandleNumber(roll);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     this is called when the UI interactions cause a state change
        /// </summary>
        /// <param name="player"></param>
        /// <param name="inputText"></param>
        /// <returns></returns>
        private async Task ProcessState(PlayerModel player, string inputText)
        {
            try
            {
                switch (CurrentGameState) // this is actually better thought of as the state we are about to change or that we need to do something and the user takes care of hitting the Next button
                {
                    case GameState.WaitingForNewGame:
                        OnNewNetworkGame(null, null);
                        break;

                    case GameState.WaitingForPlayers:
                        //
                        //  if the current player created the game, then we start the game
                        //  otherwise we just listen to messages, which will have these messages in the queue
                        if (MainPageModel.GameStartedBy == TheHuman)
                        {
                            //
                            //  randomize the board
                            await RandomBoardLog.RandomizeBoard(this, 0);
                        }
                        await SetStateLog.SetState(this, GameState.WaitingForRollForOrder);

                        break;

                    case GameState.WaitingForRollForOrder:
                        //
                        // hide board measurement UI as we've now picked out poison

                        break;

                    case GameState.WaitingForStart:
                        MainPageModel.PlayingPlayers.ForEach((p) => p.GameData.RollOrientation = TileOrientation.FaceDown);
                        await CopyScreenShotToClipboard(_gameView);
                        await SetStateAsync(CurrentPlayer, GameState.AllocateResourceForward, true);
                        break;

                    case GameState.AllocateResourceForward:

                        await OnNext();
                        await SetStateAsync(CurrentPlayer, GameState.AllocateResourceForward, false);

                        if (CurrentPlayer == MainPageModel.PlayingPlayers.Last())
                        {
                            await SetStateAsync(CurrentPlayer, GameState.AllocateResourceReverse, false);
                        }

                        break;

                    case GameState.AllocateResourceReverse:

                        if (MainPageModel.PlayingPlayers.IndexOf(MainPageModel.PlayingPlayers.Last()) != GetNextPlayerPosition(-1))
                        {
                            await OnNext(-1);
                        }
                        else
                        {
                            await ChangePlayerAndSetState(0, GameState.WaitingForRoll);
                        }

                        break;

                    case GameState.WaitingForRoll:
                        {
                            await ProcessRoll(inputText); // this will either succeed and change state to Waiting for Next, or fail and leave it in the current state...
                        }
                        break;

                    case GameState.MustMoveBaron:
                        await SetStateAsync(CurrentPlayer, GameState.WaitingForNext, true);
                        break;

                    case GameState.WaitingForNext:
                        if (HasSupplementalBuild)
                        {
                            _supplementalStartIndex = _currentPlayerIndex;
                            await ChangePlayerAndSetState(1, GameState.Supplemental);
                        }
                        else
                        {
                            await ChangePlayerAndSetState(1, GameState.WaitingForRoll);
                        }
                        break;

                    case GameState.Supplemental:
                        //
                        //  look to see what the next spot is supposed to be
                        int tempPos = GetNextPlayerPosition(1);
                        if (tempPos == _supplementalStartIndex)
                        {
                            await ChangePlayerAndSetState(2, GameState.WaitingForRoll);

                            ////
                            ////  next spot is where we started -- skip over it
                            //await OnNext(2);
                            ////
                            ////  log that we finished supplemental
                            //await AddLogEntry(CurrentPlayer, GameState, CatanAction.DoneSupplemental, false, LogType.Normal, _supplementalStartIndex);
                            //_supplementalStartIndex = -1;

                            ////
                            ////  make the UI want to get a rolll
                            //await SetStateAsync(CurrentPlayer, GameState.WaitingForRoll, false);
                        }
                        else
                        {
                            await OnNext();
                        }
                        break;

                    default:
                        break;
                }
            }
            finally
            {
            }
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
                mainPageModel = CatanProxy.Deserialize<MainPageModel>(content);

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

            MainPageModel.Log = new NewLog();

            // _lbGames.SelectedValue = MainPageModel.Log;
            await ResetTiles(true);
            Ctrl_PlayerResourceCountCtrl.GlobalResourceCount.TurnReset();
            _stateStack.Clear();

            foreach (PlayerModel player in MainPageModel.AllPlayers)
            {
                player.Reset();
            }

            _raceTracking.Reset();

            //  await LoadPlayerData();
        }

        private void ResetDataForNewGame()
        {
            _gameView.Reset();
            _gameView.SetCallbacks(this, this);

            foreach (PlayerModel p in MainPageModel.PlayingPlayers)
            {
                p.GameData.OnCardsLost -= OnPlayerLostCards;
                p.Reset();
            }
            MainPageModel.PlayingPlayers.Clear();
            _currentPlayerIndex = 0;
            Rolls.Clear();
            if (TheHuman != null)
            {
                CurrentPlayer = TheHuman;
            }
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

        // can only undo to the first resource allocation
        private void SavedGame_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //try
            //{
            //    if (e.AddedItems.Count > 0)
            //    {
            //        DeprecatedLog newLog = e.AddedItems[0] as DeprecatedLog;
            //        if (MainPageModel.Log == newLog)
            //        {
            //            return;
            //        }

            //        if (await StaticHelpers.AskUserYesNoQuestion($"Switch to {newLog.File.DisplayName}?", "Yes", "No"))
            //        {
            //            MainPageModel.Log = newLog;
            //            //   await newLog.Parse(this);
            //            // TODO:...
            //            await ReplayLog(newLog);
            //            UpdateUiForState(MainPageModel.Log.Last().GameState);
            //        }
            //        else
            //        {
            //            _lbGames.SelectedItem = MainPageModel.Log;
            //        }

            //    }
            //}
            //catch (Exception exception)
            //{
            //    MessageDialog dlg = new MessageDialog($"Error loading file {e.AddedItems[0]}\nMessage:\n{exception.Message}");
            //}
        }
        private async Task SaveGameState()
        {
            try
            {
                if (fileGuard != null)
                {
                    await fileGuard.Task;
                }
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

                this.TraceMessage($"Saving GameState");                
                fileGuard = new TaskCompletionSource<object>();
                StorageFolder folder = await StaticHelpers.GetSaveFolder();
                var content = CatanProxy.Serialize<MainPageModel>(MainPageModel, true);
                StorageFile file = await folder.CreateFileAsync(PlayerDataFile, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, content);
                if (fileGuard != null)
                {
                    fileGuard.SetResult(null);
                    fileGuard = null;
                }
            }
            catch (Exception e)
            {
                this.TraceMessage($"eating exception: {e}");
            }
        }

        /// <summary>
        ///     Unlike previsous implementations, the use the Action/Undo stacks in the log to store the random boards.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task ScrollMouseWheelInServiceGame(PointerRoutedEventArgs e)
        {
            if (TheHuman.PlayerName != MainPageModel.GameInfo.Creator) return;

            if (MainPageModel.Log.GameState == GameState.PickingBoard)
            {
                if (e.GetCurrentPoint(this).Properties.MouseWheelDelta >= 0)

                {
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

        private async Task SetBuildingAndRoad()
        {
            // pick a tile with the highest pips and put a settlement on it
            var building = GetHighestPipsBuilding();
            await building.UpdateBuildingState(CurrentPlayer, building.BuildingState, BuildingState.Settlement);

            // pick a Random Road
            var road = building.AdjacentRoads[testRandom.Next(building.AdjacentRoads.Count)];
            //
            //  5/15/2020: moved to passying CurrentPlayer pointer around and binding to its colors
            road.CurrentPlayer = CurrentPlayer;

            await UpdateRoadState(road, road.RoadState, RoadState.Road, LogType.Normal);
        }

        private async Task StartGame(ICollection<PlayerModel> players, int selectedIndex)
        {
            ResetDataForNewGame();

            foreach (PlayerModel pData in players)
            {
                //
                //  add it to the collection that all the views bind to
                await AddPlayer(pData, LogType.Normal);
            }

            await VisualShuffle();
            await SetStateAsync(null, GameState.WaitingForStart, true);
            await AnimateToPlayerIndex(_currentPlayerIndex);

            //  var model = StartGameController.StartGame(this, selectedIndex, GameContainer.RandomBoardSettings, MainPageModel.PlayingPlayers);
            //  VerifyRoundTrip<StartGamLog>(model);
            //Log.PushAction(model);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.SelectAll();
        }

        private async Task VisualShuffle(RandomBoardSettings rbs = null)
        {
            await _gameView.VisualShuffle(rbs);
            _randomBoardList.Clear();
            _randomBoardList.Add(_gameView.RandomBoardSettings);
            _randomBoardListIndex = 0;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New)
            {
                _progress.IsActive = true;
                _progress.Visibility = Visibility.Visible;

                _gameView.Init(this, this);
                CreateMenuItems();

                await LoadMainPageModel();
                UpdateGridLocations();
                _progress.Visibility = Visibility.Collapsed;
                _progress.IsActive = false;

                Ctrl_PlayerResourceCountCtrl.MainPage = this;
            }

            InitTest();
            ResetDataForNewGame();
            await WsConnect();
        }

        public static string CreateSaveFileName(string Description)
        {
            DateTime dt = DateTime.Now;

            string ampm = dt.TimeOfDay.TotalMinutes > 720 ? "PM" : "AM";
            string min = dt.TimeOfDay.Minutes.ToString().PadLeft(2, '0');

            return string.Format($"{dt.TimeOfDay.Hours % 12}.{min} {ampm} - {Description}");
        }

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

        public Task AddLogEntry(PlayerModel player, GameState state, CatanAction action, bool stopProcessingUndo, LogType logType = LogType.Normal, int number = -1, object tag = null, [CallerFilePath] string filePath = "", [CallerMemberName] string name = "", [CallerLineNumber] int lineNumber = 0)
        {
            //  await MainPageModel.Log.AppendLogLine(new LogEntry(player, state, action, number, stopProcessingUndo, logType, tag, name, lineNumber, filePath));
            return Task.CompletedTask;
        }

        public void CountResourcesForRoll(IReadOnlyCollection<TileCtrl> tilesWithNumber, bool undo)
        {
            foreach (TileCtrl tile in tilesWithNumber)
            {
                foreach (BuildingCtrl building in tile.OwnedBuilding)
                {
                    if (building.Owner == null)
                    {
                        continue;
                        //System.Diagnostics.Debug.Assert(false);
                    }

                    //
                    //  this updates the *count* of resources and returns the value the user gets for the number being rolled
                    int value = building.Owner.GameData.UpdateResourceCount(tile.ResourceType, building.BuildingState, tile.HasBaron, undo);
                    //
                    //  need to look up the control given the player and add it to the right one
                    AddResourceCountForPlayer(building.Owner, tile.ResourceType, value);
                }
            }
            if (!undo)
            {
                //
                //  go through players and update the good/bad roll count
                foreach (var player in MainPageModel.PlayingPlayers)
                {
                    if (player.GameData.PlayerTurnResourceCount.Total == 0)
                    {
                        player.GameData.NoResourceCount++;
                        player.GameData.GoodRoll = false;
                    }
                    else
                    {
                        player.GameData.RollsWithResource++;
                        player.GameData.NoResourceCount = 0;
                        player.GameData.GoodRoll = true;
                    }
                }
            }
        }

        public List<int> GetRandomGoldTiles()
        {
            if (!this.RandomGold || this.RandomGoldTileCount < 1) return new List<int>();
            var currentRandomGoldTiles = _gameView.CurrentRandomGoldTiles;
            return _gameView.PickRandomTilesToBeGold(RandomGoldTileCount, currentRandomGoldTiles);
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

        //
        //  we use the build ellipses during the allocation phase to see what settlements have the most pips
        //  when we move to the next player, hide the build ellipses
        public async Task<List<TileCtrl>> HighlightRolledTiles(int rolledNumber)
        {
            List<TileCtrl> tilesWithNumber = new List<TileCtrl>();

            List<Task> tasks = new List<Task>();
            foreach (TileCtrl t in _gameView.CurrentGame.Tiles)
            {
                if (t.Number == rolledNumber)

                {
                    tilesWithNumber.Add(t);
                    if (MainPageModel.Settings.AnimateFade)
                    {
                        t.AnimateFadeAsync(1.0);
                    }
                    if (MainPageModel.Settings.RotateTile)
                    {
                        t.Rotate(180, tasks, true);
                    }
                }
                else
                {
                    if (MainPageModel.Settings.AnimateFade)
                    {
                        t.AnimateFadeAsync(.25);
                    }
                }
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks.ToArray());
            }

            //
            // now make sure we reverse the fade

            return tilesWithNumber;
        }

        public async Task<List<TileCtrl>> PlayRollAnimation(int rolledNumber)
        {
            List<TileCtrl> tilesWithNumber = new List<TileCtrl>();
            List<Task> tasks = new List<Task>();
            foreach (TileCtrl t in _gameView.CurrentGame.Tiles)
            {
                if (t.Number == rolledNumber)

                {
                    tilesWithNumber.Add(t);
                    if (MainPageModel.Settings.AnimateFade)
                    {
                        t.AnimateFadeAsync(1.0);
                    }
                    if (MainPageModel.Settings.RotateTile)
                    {
                        t.Rotate(180, tasks, true);
                    }
                }
                else
                {
                    if (MainPageModel.Settings.AnimateFade)
                    {
                        t.AnimateFadeAsync(0.25);
                    }
                }
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks.ToArray());
            }

            return tilesWithNumber;
        }

        public void PostLogEntry(PlayerModel player, GameState state, CatanAction action, bool stopProcessingUndo, LogType logType = LogType.Normal, int number = -1, object tag = null, [CallerFilePath] string filePath = "", [CallerMemberName] string name = "", [CallerLineNumber] int lineNumber = 0)

        {
            //    if (MainPageModel.Log == null)
            //    {
            //        return;
            //    }

            //    if (state == GameState.Unknown)
            //    {
            //        state = this.GameStateFromOldLog;
            //    }

            //    MainPageModel.Log.AppendLogLineNoDisk(new LogEntry(player, state, action, number, stopProcessingUndo, logType, tag, name, lineNumber, filePath));
        }

        public async Task<bool> ProcessRoll(string inputText)
        {
            if (int.TryParse(inputText, out int roll))
            {
                return await ProcessRoll(roll);
            }

            return false;
        }

        public async Task<bool> ReplayLog(DeprecatedLog log)
        {
            try
            {
                _progress.IsActive = true;
                _progress.Visibility = Visibility.Visible;
                await Task.Delay(0);
                await this.Reset();
                ResetDataForNewGame();
                int n = 0;
                log.State = LogState.Replay;

                //
                //  go through the first time and mark all entries that have been undone.
                //  this has to be two pass because the log needs to be replayed in order
                //  marking records as undone means we don't have to replay and then undo them
                //

                foreach (LogEntry logLine in log.Actions)
                {
                    n++;
                    if (logLine.LogType == LogType.Undo)
                    {
                        continue;
                    }

                    switch (logLine.Action)
                    {
                        case CatanAction.Rolled:
                            PushRoll(logLine.Number);
                            break;

                        case CatanAction.AddResourceCount:
                            LogResourceCount lrc = logLine.Tag as LogResourceCount;
                            if (logLine.PlayerData != null)
                            {
                                logLine.PlayerData.GameData.PlayerTurnResourceCount.AddResourceCount(lrc.ResourceType, logLine.Number);
                            }
                            else
                            {
                                Ctrl_PlayerResourceCountCtrl.GlobalResourceCount.AddResourceCount(lrc.ResourceType, logLine.Number);
                            }

                            break;

                        case CatanAction.ChangedState:
                            break;

                        case CatanAction.ChangedPlayer:
                            LogChangePlayer lcp = logLine.Tag as LogChangePlayer;
                            await AnimateToPlayerIndex(lcp.To, LogType.Replay);
                            break;

                        case CatanAction.Dealt:
                            break;

                        case CatanAction.CardsLost:
                            LogCardsLost lcl = logLine.Tag as LogCardsLost;
                            await LogPlayerLostCards(logLine.PlayerData, lcl.OldVal, lcl.NewVal, LogType.Replay);
                            break;

                        case CatanAction.CardsLostToSeven:
                            break;

                        case CatanAction.MissedOpportunity:
                            break;

                        case CatanAction.DoneSupplemental:
                            break;

                        case CatanAction.DoneResourceAllocation:
                            break;

                        case CatanAction.SetFirstPlayer:
                            if (logLine.Tag == null)
                            {
                                continue;
                            }

                            LogSetFirstPlayer lsfp = logLine.Tag as LogSetFirstPlayer;
                            await SetFirst(MainPageModel.PlayingPlayers[lsfp.FirstPlayerIndex]);
                            break;

                        case CatanAction.PlayedKnight:
                        case CatanAction.AssignedBaron:
                        case CatanAction.AssignedPirateShip:
                            LogBaronOrPirate lbp = logLine.Tag as LogBaronOrPirate;
                            await AssignBaronOrKnight(lbp.TargetPlayer, lbp.TargetTile, lbp.TargetWeapon, lbp.Action, LogType.Replay);
                            break;

                        case CatanAction.UpdatedRoadState:
                            LogRoadUpdate roadUpdate = logLine.Tag as LogRoadUpdate;
                            if (roadUpdate.NewRoadState != RoadState.Unowned)
                            {
                                //   roadUpdate.Road.Color = CurrentPlayer.GameData.PlayerColor;
                            }
                            else
                            {
                                //  roadUpdate.Road.Color = Colors.Transparent;
                            }

                            await UpdateRoadState(roadUpdate.Road, roadUpdate.OldRoadState, roadUpdate.NewRoadState, LogType.Replay);
                            break;

                        case CatanAction.UpdateBuildingState:

                            LogBuildingUpdate lsu = (LogBuildingUpdate)logLine.Tag;
                            if (lsu.OldBuildingState != BuildingState.City)
                            {
                                lsu.Building.Owner = CurrentPlayer;
                            }

                            await lsu.Building.UpdateBuildingState(CurrentPlayer, lsu.OldBuildingState, lsu.NewBuildingState);
                            break;

                        case CatanAction.AddPlayer:
                            await AddPlayer(logLine, LogType.Replay);
                            break;

                        case CatanAction.RandomizeBoard:
                            RandomBoardSettings boardSetting = logLine.Tag as RandomBoardSettings;
                            await _gameView.SetRandomCatanBoard(true, boardSetting);
                            break;

                        case CatanAction.InitialAssignBaron:
                            _gameView.BaronTile = _gameView.TilesInIndexOrder[logLine.Number];
                            break;

                        case CatanAction.SelectGame:
                            _gameView.CurrentGame = _gameView.Games[logLine.Number];
                            await Task.Delay(0);
                            break;

                        case CatanAction.RoadTrackingChanged:
                            //LogRoadTrackingChanged lrtc = logLine.Tag as LogRoadTrackingChanged;
                            //_raceTracking.Deserialize(lrtc.NewState, this);
                            break;

                        case CatanAction.ChangedPlayerProperty:
                            LogPropertyChanged lpc = logLine.Tag as LogPropertyChanged;
                            logLine.PlayerData.GameData.SetKeyValue<PlayerGameModel>(lpc.PropertyName, lpc.NewVal);
                            break;

                        default:
                            break;
                    }
                }

                _gameView.FlipAllAsync(TileOrientation.FaceUp);
            }
            finally
            {
                _progress.IsActive = false;
                _progress.Visibility = Visibility.Collapsed;
                log.State = LogState.Normal;
            }
            return true;
        }

        public async Task SetRandomTileToGold(List<int> goldTilesIndices)
        {
            await _gameView.ResetRandomGoldTiles();
            if (this.RandomGold && this.RandomGoldTileCount > 0)
            {
                await _gameView.SetRandomTilesToGold(goldTilesIndices);
            }
        }

        public Task SetStateAsync(PlayerModel playerData, GameState newState, bool stopUndo, LogType logType = LogType.Normal, [CallerFilePath] string filePath = "", [CallerMemberName] string cmn = "", [CallerLineNumber] int lineNumber = 0)

        {
            return Task.CompletedTask;
            //if (MainPageModel.Log == null)
            //{
            //    return;
            //}

            //LogStateTranstion lst = new LogStateTranstion(GameStateFromOldLog, newState);
            //await MainPageModel.Log.AppendLogLine(new LogEntry(playerData, newState, CatanAction.ChangedState, -1, stopUndo, logType, lst, cmn, lineNumber, filePath));
            //UpdateUiForState(newState);
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

        public void UpdateGlobalRollStats()
        {
            int[] roals = RollCount(Rolls);
            double[] percent = RollPercents(Rolls, roals);

            //String.Format("{0:0.#}%", percent * 100)
            TotalRolls = Rolls.Count();

            TwoPercent = string.Format($"{roals[0]} ({percent[0] * 100:0.#}%)");
            ThreePercent = string.Format($"{roals[1]} ({percent[1] * 100:0.#}%)");
            FourPercent = string.Format($"{roals[2]} ({percent[2] * 100:0.#}%)");
            FivePercent = string.Format($"{roals[3]} ({percent[3] * 100:0.#}%)");
            SixPercent = string.Format($"{roals[4]} ({percent[4] * 100:0.#}%)");
            SevenPercent = string.Format($"{roals[5]} ({percent[5] * 100:0.#}%)");
            EightPercent = string.Format($"{roals[6]} ({percent[6] * 100:0.#}%)");
            NinePercent = string.Format($"{roals[7]} ({percent[7] * 100:0.#}%)");
            TenPercent = string.Format($"{roals[8]} ({percent[8] * 100:0.#}%)");
            ElevenPercent = string.Format($"{roals[9]} ({percent[9] * 100:0.#}%)");
            TwelvePercent = string.Format($"{roals[10]} ({percent[10] * 100:0.#}%)");
        }

        

        /**
         *  this feature will take one tile and randomly turn it into the gold tile
         *  it happens *before* the user rolls so that the player can decide to (say)
         *  put a knight on the Gold tile, or move the knight off the gold tile.
         *
         *  based on game play experience, it also makes sure that it doesn't pick the same tile
         *  twice in a row
         *
         */
    }
}