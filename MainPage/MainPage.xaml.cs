﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI;
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
        private string _settingsFileName = "CatanSettings.ini";
        private Settings _settings = new Settings();
        private int _supplementalStartIndex = -1;

        public static readonly string SAVED_GAME_EXTENSION = ".log";
        public const string PlayerDataFile = "players.data";
        private const string SERIALIZATION_VERSION = "3";

        public ObservableCollection<Log> SavedGames { get; set; } = new ObservableCollection<Log>();

        private DispatcherTimer _timer = new DispatcherTimer();
        private bool _doDragDrop = false;
        private int _currentPlayerIndex = 0; // the index into PlayingPlayers that is the CurrentPlayer


        public static MainPage Current;
        private UserControl _currentView = null;
        private Log _log = null;


        public ObservableCollection<PlayerData> PlayingPlayers { get; set; } = new ObservableCollection<PlayerData>();
        public ObservableCollection<PlayerData> AllPlayers { get; set; } = new ObservableCollection<PlayerData>();

        private RoadRaceTracking _raceTracking = null;




        public MainPage()
        {
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            this.InitializeComponent();

            Current = this;
            this.DataContext = this;
            _timer.Interval = TimeSpan.FromSeconds(FadeSeconds);
            _timer.Tick += AsyncReverseFade;

            //
            //  this sets the view's listboxes.  from now on it will 
            //  dynamically changes as we add/remove players who are playing.
            //  note that we should never new a new collection -- just modify it.
            Ctrl_PlayerResourceCountCtrl.PlayingPlayers = PlayingPlayers;

            _raceTracking = new RoadRaceTracking(this);
            Ctrl_PlayerResourceCountCtrl.Log = this;
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

        private void ShowNumberUi()
        {
            _daNumberOpacity.To = 1.0;
            _sbNumberOpacity.Begin();
            RollGrid.IsHitTestVisible = true;
        }

        private void HideNumberUi()
        {
            _daNumberOpacity.To = 0;
            _sbNumberOpacity.Begin();
            RollGrid.IsHitTestVisible = false;
        }

        private void ToggleNumberUi()
        {
            if (_daNumberOpacity.To == 1.0)
            {
                HideNumberUi();
            }
            else
            {
                ShowNumberUi();
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Windows.ApplicationModel.Activation.IActivatedEventArgs args)
            {

                if (args.Kind == Windows.ApplicationModel.Activation.ActivationKind.File)
                {
                    //var fileArgs = args as Windows.ApplicationModel.Activation.FileActivatedEventArgs;
                    //string strFilePath = fileArgs.Files[0].Path;
                    //var file = (StorageFile)fileArgs.Files[0];
                    //await LoadCatanFile(file);
                }
            }

            if (e.NavigationMode == NavigationMode.New)
            {
                _progress.IsActive = true;
                _progress.Visibility = Visibility.Visible;
                _currentView = _gameView;
                _gameView.Init(this, this);
                CreateMenuItems();
                await _settings.LoadSettings(_settingsFileName);

                UpdateGridLocations();
                await LoadPlayerData();
                _progress.Visibility = Visibility.Collapsed;
                _progress.IsActive = false;

                Ctrl_PlayerResourceCountCtrl.MainPage = this;
            }




        }



        private async Task LoadPlayerData()
        {

            try
            {

                StorageFolder folder = await StaticHelpers.GetSaveFolder();
                Dictionary<string, string> playersDictionary = await StaticHelpers.LoadSectionsFromFile(folder, PlayerDataFile);


                foreach (KeyValuePair<string, string> kvp in playersDictionary)
                {

                    PlayerData p = new PlayerData(this);

                    p.Deserialize(kvp.Value, false);
                    await p.LoadImage();
                    if (p.PlayerIdentifier == Guid.Empty)
                    {
                        p.PlayerIdentifier = Guid.NewGuid();
                    }

                    p.AllPlayerIndex = AllPlayers.Count;
                    AllPlayers.Add(p);

                }


            }
            catch
            {

            }

        }



        public GameType GameType
        {
            get => _gameView.CurrentGame.GameType;
            set
            {

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




        public bool HasSupplementalBuild => GameType == GameType.SupplementalBuildPhase;




        private async void AsyncReverseFade(object sender, object e)
        {
            List<Task> tasks = new List<Task>();
            foreach (TileCtrl t in _gameView.CurrentGame.Tiles)
            {
                t.AnimateFade(1.0, tasks);
            }

            await Task.WhenAll(tasks.ToArray());
            _timer.Stop();
        }




        private async Task VisualShuffle()
        {
            await _gameView.VisualShuffle();
            _randomBoardList.Clear();
            _randomBoardList.Add(_gameView.RandomBoardSettings);
            _randomBoardListIndex = 0;

        }



        private async Task Reset()
        {
            _log?.Reset();
            _lbGames.SelectedValue = _log;
            await ResetTiles(true);
            PlayingPlayers.Clear();
            Ctrl_PlayerResourceCountCtrl.GameResourceData.Reset();
            _stateStack.Clear();

            foreach (PlayerData player in AllPlayers)
            {
                player.Reset();
            }

            _raceTracking.Reset();
            _log?.Start();


        }

        /// <summary>
        /// Update this because you did the sorting work in the dialog
        /// hide all positions and then loop through the array to make them visible
        /// 
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>

        private async Task StartGame(List<PlayerData> players)
        {



            ResetDataForNewGame();

            foreach (PlayerData pData in players)
            {
                //
                //  add it to the collection that all the views bind to
                await AddPlayer(pData, LogType.Normal);

            }


            await VisualShuffle();
            await AnimateToPlayerIndex(_currentPlayerIndex);
            await SetStateAsync(null, GameState.WaitingForStart, true);


            //
            //  we used to wait until somebody clicked "Start" after starting a new game.  this was annoying. do it for them.
            //  await ProcessEnter(CurrentPlayer, "");


        }



        private void ResetDataForNewGame()
        {
            _gameView.Reset();
            _gameView.SetCallbacks(this, this);

            foreach (PlayerData p in PlayingPlayers)
            {
                p.GameData.OnCardsLost -= OnPlayerLostCards;
            }
            PlayingPlayers.Clear();
            _currentPlayerIndex = 0;
            Rolls.Clear();



        }

        private bool _undoingCardLostHack = false;
        private async void OnPlayerLostCards(PlayerData player, int oldVal, int newVal)
        {
            if (_undoingCardLostHack)
            {
                return;
            }

            await LogPlayerLostCards(player, oldVal, newVal, LogType.Normal);
        }

        private async Task LogPlayerLostCards(PlayerData player, int oldVal, int newVal, LogType logType)
        {
            await AddLogEntry(player, GameState, CatanAction.CardsLost, true, logType, -1, new LogCardsLost(oldVal, newVal));
        }


        private async Task AddPlayer(PlayerData pData, LogType logType)
        {

            PlayingPlayers.Add(pData);
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
            await AddLogEntry(pData, GameState.Starting, CatanAction.AddPlayer, false, logType, PlayingPlayers.Count, pData.AllPlayerIndex); // can't undo adding players...

        }

        private async Task AddPlayer(LogEntry le, LogType logType)
        {

            await AddPlayer(le.PlayerData, logType);

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
                t.AnimateFade(1.0, tasks);
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

        private void UpdateGameCommands()
        {
           
            _btnNextStep.IsEnabled = false;
            _btnUndo.IsEnabled = false;
            _btnNewGame.IsEnabled = true;
           
            switch (State.GameState)
            {
                case GameState.Uninitialized:
                case GameState.WaitingForNewGame:
                case GameState.Dealing:
                    break;
                case GameState.WaitingForStart:
                    _btnNextStep.IsEnabled = true;
                    break;
                case GameState.AllocateResourceForward:
                case GameState.AllocateResourceReverse:
                    _btnNextStep.IsEnabled = true;
                    _btnUndo.IsEnabled = true;
                    break;
                case GameState.DoneResourceAllocation:
                    _btnNextStep.IsEnabled = true;
                    _btnUndo.IsEnabled = true;
                    break;
                case GameState.WaitingForRoll:
                    _btnNextStep.IsEnabled = false;
                    _btnUndo.IsEnabled = true;
                    break;
                case GameState.Targeted:
                    break;
                case GameState.LostToCardsLikeMonopoly:
                    break;
                case GameState.Supplemental:
                    _btnNextStep.IsEnabled = true;
                    _btnUndo.IsEnabled = true;
                    break;
                case GameState.DoneSupplemental:
                    _btnNextStep.IsEnabled = true;
                    _btnUndo.IsEnabled = true;
                    break;
                case GameState.WaitingForNext:
                    _btnNextStep.IsEnabled = true;
                    _btnUndo.IsEnabled = true;
                    break;
                case GameState.LostCardsToSeven:
                    break;
                case GameState.MissedOpportunity:
                    break;
                default:
                    break;
            }
        }


        private async Task ProcessEnter(PlayerData player, string inputText)
        {


            try
            {

                UpdateGameCommands();
                switch (State.GameState)
                {
                    case GameState.Dealing: // a state just to be undone...
                        break;
                    case GameState.WaitingForNewGame:
                        await OnNewGame();
                        break;
                    case GameState.WaitingForStart:
                        await CopyScreenShotToClipboard(_gameView);
                        await SetStateAsync(CurrentPlayer, GameState.AllocateResourceForward, true);
                        break;
                    case GameState.AllocateResourceForward:

                        await OnNext();
                        await SetStateAsync(CurrentPlayer, GameState.AllocateResourceForward, false);
                        if (CurrentPlayer == PlayingPlayers.Last())
                        {
                            await SetStateAsync(CurrentPlayer, GameState.AllocateResourceReverse, false);
                        }

                        break;
                    case GameState.AllocateResourceReverse:

                        if (PlayingPlayers.IndexOf(PlayingPlayers.Last()) != GetNextPlayerPosition(-1))
                        {
                            await OnNext(-1);
                        }
                        else
                        {
                            await SetStateAsync(CurrentPlayer, GameState.DoneResourceAllocation, false); // I'm logging two states here because there used to be a "hit to start" UI state...
                            await SetStateAsync(CurrentPlayer, GameState.WaitingForRoll, true);
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
                            await OnNext();
                            await SetStateAsync(CurrentPlayer, GameState.Supplemental, false);

                        }
                        else
                        {
                            await OnNext();
                            await SetStateAsync(CurrentPlayer, GameState.WaitingForRoll, false);
                        }
                        break;
                    case GameState.Supplemental:
                        //
                        //  look to see what the next spot is supposed to be
                        int tempPos = GetNextPlayerPosition(1);
                        if (tempPos == _supplementalStartIndex)
                        {
                            //
                            //  next spot is where we started -- skip over it 
                            await OnNext(2);
                            //
                            //  log that we finished supplemental
                            await AddLogEntry(CurrentPlayer, GameState, CatanAction.DoneSupplemental, false, LogType.Normal, _supplementalStartIndex);
                            _supplementalStartIndex = -1;

                            //
                            //  make the UI want to get a rolll
                            await SetStateAsync(CurrentPlayer, GameState.WaitingForRoll, false);
                        }
                        else
                        {
                            await OnNext();
                            //
                            //   stay in Supplemental, but set this in case 
                            UpdateUiForState(GameState);
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


        //
        //  this needs to be called *after* the log for the Roll because we need to undo all of these prior to undoing the Roll
        private void AddResourceCountForPlayer(PlayerData player, ResourceType resource, int count, LogType logType = LogType.Normal)
        {
            int oldValPlayer = player.GameData.PlayerResourceData.AddResourceCount(resource, count); // update the player
            int oldValGlobal = Ctrl_PlayerResourceCountCtrl.GameResourceData.AddResourceCount(resource, count); // update for the game
            if (player.GameData.GoodRoll == false)
            {
                player.GameData.GoodRoll = true;
                player.GameData.RollsWithResource++;

            }
            if (count > 0)
            {
                player.GameData.NoResourceCount = 0;
            }

            //
            //  TODO:  log GoodRoll, RollsWithResource and NoResourceCount

            // await AddLogEntry(player, this.GameState, CatanAction.AddResourceCount, false, logType, -1, new LogResourceCount(oldValPlayer, oldValPlayer + count, resource));
        }

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

        private const int MAX_SAVE_FILES_RETAINED = 5;

        private async Task LoadSavedGames()
        {

            IReadOnlyList<StorageFile> files = await GetSavedFilesInternal();
            List<StorageFile> fList = new List<StorageFile>();
            fList.AddRange(files);

            IOrderedEnumerable<StorageFile> sort = from s in files orderby s.DateCreated.Ticks descending select s;
            SavedGames.Clear();
            foreach (StorageFile f in sort)
            {
                Log log = new Log(f);
                SavedGames.Add(log);
            }

        }

        public async static Task<bool> SavePlayers(IEnumerable<PlayerData> players, string fileName)
        {

            StorageFolder folder = await StaticHelpers.GetSaveFolder();
            StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            string toWrite = "";
            foreach (PlayerData p in players)
            {
                toWrite += string.Format($"[{p.PlayerName}]{StaticHelpers.lineSeperator}{p.Serialize(false)}\n");
            }
            try
            {
                await FileIO.WriteTextAsync(file, toWrite);
            }
            catch (Exception e)
            {
                folder.TraceMessage($"Exception: {e.ToString()}");
            }
            return true;
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

        private async Task<bool> ProcessRoll(string inputText)
        {
            int roll = -1;
            if (int.TryParse(inputText, out roll))
            {
                return await ProcessRoll(roll);

            }

            return false;
        }



        private void UpdateRollStats()
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

        private async Task OnNext(int playersToMove = 1, LogType logType = LogType.Normal)
        {


            await AnimatePlayers(playersToMove, logType);


            UpdateRollStats();
            foreach (TileCtrl t in _gameView.AllTiles)
            {
                t.ResetOpacity();
                t.ResetTileRotation();

            }
            //
            //  on next, reset the resources for the turn to 0
            foreach (PlayerData player in PlayingPlayers)
            {
                player.GameData.PlayerResourceData.Reset();
            }


        }

        //
        //  we use the build ellipses during the allocation phase to see what settlements have the most pips
        //  when we move to the next player, hide the build ellipses

        private async Task HideAllPipEllipses()
        {
            foreach (BuildingCtrl s in _gameView.CurrentGame.HexPanel.Buildings)
            {
                if (s.BuildingState == BuildingState.Pips)
                {
                    await s.UpdateBuildingState(s.BuildingState, BuildingState.None, LogType.DoNotLog);
                }
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



        //
        //  n is verified by the caller
        //  this adds a number to the list we use to keep track of what is rolled and updates all the statistics
        private async Task HandleNumber(int val)
        {



            bool ret = this.PushRoll(val); // only returns false on a number outside the range...
            if (!ret)
            {
                return;
            }

            List<TileCtrl> tilesWithNumber = new List<TileCtrl>();


            List<Task> tasks = new List<Task>();
            foreach (TileCtrl t in _gameView.CurrentGame.Tiles)
            {

                if (t.Number == val)

                {
                    tilesWithNumber.Add(t);
                    if (_settings.AnimateFade)
                    {

                        t.AnimateFade(1.0, tasks);

                    }
                    if (_settings.RotateTile)
                    {
                        t.Rotate(180, tasks, true);
                    }
                }
                else
                {
                    if (_settings.AnimateFade)
                    {
                        t.AnimateFade(0.25, tasks);
                    }
                }
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks.ToArray());
            }

            //
            // now make sure we reverse the fade
            _timer.Start();



            if (val == 7)
            {
                CurrentPlayer.GameData.MovedBaronAfterRollingSeven = false;
                await SetStateAsync(CurrentPlayer, GameState.MustMoveBaron, false);
                foreach (PlayerData player in PlayingPlayers)
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

        private void CountResourcesForRoll(List<TileCtrl> tilesWithNumber, bool undo)
        {
            //
            //  add one to the "no resources count for each player -- we will reset it if they get some
            //  also set the flag that says we haven't counted this as a good roll for the player yet
            foreach (PlayerData player in PlayingPlayers)
            {
                player.GameData.NoResourceCount++;
                player.GameData.GoodRoll = false;

            }

            foreach (TileCtrl tile in tilesWithNumber)
            {
                foreach (BuildingCtrl building in tile.OwnedBuilding)
                {
                    if (building.Owner == null)
                    {
                        continue;
                        //System.Diagnostics.Debug.Assert(false);

                    }

                    int value = building.Owner.GameData.UpdateResourceCount(tile.ResourceType, building.BuildingState, tile.HasBaron, undo);
                    //
                    //  need to look up the control given the player and add it to the right one
                    AddResourceCountForPlayer(building.Owner, tile.ResourceType, value);

                }
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

        public LogEntry State
        {
            get
            {
                if (_log.Count == 0)
                {
                    return null;
                }

                return _log.Last();
            }
        }

        public GameState GameState
        {
            get
            {
                if (_log == null)
                {
                    return GameState.WaitingForNewGame;
                }

                if (_log.Count == 0)
                {
                    return GameState.WaitingForNewGame;
                }

                return _log.Last().GameState;
            }

        }

        public async Task AddLogEntry(PlayerData player, GameState state, CatanAction action, bool stopProcessingUndo, LogType logType = LogType.Normal, int number = -1, object tag = null, [CallerFilePath] string filePath = "", [CallerMemberName] string name = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (_log == null)
            {
                return;
            }

            if (state == GameState.Unknown)
            {
                state = this.GameState;
            }

            await _log.AppendLogLine(new LogEntry(player, state, action, number, stopProcessingUndo, logType == LogType.DoNotLog ? logType : LogType.Test, tag, name, lineNumber, filePath));

        }
        public void PostLogEntry(PlayerData player, GameState state, CatanAction action, bool stopProcessingUndo, LogType logType = LogType.Normal, int number = -1, object tag = null, [CallerFilePath] string filePath = "", [CallerMemberName] string name = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (_log == null)
            {
                return;
            }

            if (state == GameState.Unknown)
            {
                state = this.GameState;
            }

            _log.AppendLogLineNoDisk(new LogEntry(player, state, action, number, stopProcessingUndo, logType == LogType.DoNotLog ? logType : LogType.Test, tag, name, lineNumber, filePath));

        }

        private async Task SetStateAsync(PlayerData player, GameState newState, bool stopUndo, LogType logType = LogType.Normal, [CallerFilePath] string filePath = "", [CallerMemberName] string name = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (_log == null)
            {
                return;
            }

            string n = (player != null) ? player.PlayerName : "<no player>";

            LogStateTranstion lst = new LogStateTranstion(GameState, newState);


            await _log.AppendLogLine(new LogEntry(player, newState, CatanAction.ChangedState, -1, stopUndo, logType, lst, n, lineNumber, filePath));
            UpdateUiForState(newState);

        }



        private void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.SelectAll();
        }


        private void OnGameViewControlTapped(object sender, TappedRoutedEventArgs e)
        {
            //   Pointer p = await StaticHelpers.DragAsync((UIElement)sender, e);
        }

        private async void GameViewControlPointerPressed(object sender, PointerRoutedEventArgs pRoutedEvents)
        {
            if (_doDragDrop)
            {
                await StaticHelpers.DragAsync((UIElement)sender, pRoutedEvents);
            }

        }



        private void GameViewControlDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _doDragDrop = _doDragDrop ? false : true;
        }

        public async Task<bool> ReplayLog(Log log)
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

                for (int i = log.LogEntries.Count - 1; i > 0; i--)
                {
                    LogEntry logLine = log.LogEntries[i];
                    if (logLine.LogType == LogType.Undo)
                    {
                        log.LogEntries[logLine.IndexOfUndoneAction].Undone = true;
                        Debug.Assert(log.LogEntries[logLine.IndexOfUndoneAction].LogLineIndex == logLine.IndexOfUndoneAction);
                    }

                }


                foreach (LogEntry logLine in log.LogEntries)
                {
                    n++;
                    if (logLine.LogType == LogType.Undo)
                    {
                        continue;
                    }

                    if (logLine.Undone == true)
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
                                logLine.PlayerData.GameData.PlayerResourceData.AddResourceCount(lrc.ResourceType, logLine.Number);
                            }
                            else
                            {
                                Ctrl_PlayerResourceCountCtrl.GameResourceData.AddResourceCount(lrc.ResourceType, logLine.Number);
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
                            await SetFirst(PlayingPlayers[lsfp.FirstPlayerIndex]);
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
                                roadUpdate.Road.Color = CurrentPlayer.GameData.PlayerColor;
                            }
                            else
                            {
                                roadUpdate.Road.Color = Colors.Transparent;
                            }

                            await UpdateRoadState(roadUpdate.Road, roadUpdate.OldRoadState, roadUpdate.NewRoadState, LogType.Replay);
                            break;
                        case CatanAction.UpdateBuildingState:

                            LogBuildingUpdate lsu = (LogBuildingUpdate)logLine.Tag;
                            if (lsu.OldBuildingState != BuildingState.City)
                            {
                                lsu.Building.Owner = CurrentPlayer;
                            }

                            await lsu.Building.UpdateBuildingState(lsu.OldBuildingState, lsu.NewBuildingState, LogType.Replay);
                            break;
                        case CatanAction.AddPlayer:
                            await AddPlayer(logLine, LogType.Replay);
                            break;
                        case CatanAction.RandomizeTiles:
                            List<int> randomNumbers = logLine.Tag as List<int>;
                            await _gameView.AssignRandomNumbersToTileGroup(logLine.Number, randomNumbers);
                            break;
                        case CatanAction.AssignHarbors:
                            _gameView.AssignRandomNumbersToHarbors((List<int>)logLine.Tag);
                            break;
                        case CatanAction.AssignRandomTiles:
                            await _gameView.AssignRandomTilesToTileGroup(logLine.Number, (List<int>)logLine.Tag);
                            break;
                        case CatanAction.InitialAssignBaron:
                            _gameView.BaronTile = _gameView.TilesInIndexOrder[logLine.Number];
                            break;
                        case CatanAction.SelectGame:
                            _gameView.CurrentGame = _gameView.Games[logLine.Number];
                            await Task.Delay(0);
                            break;
                        case CatanAction.RoadTrackingChanged:
                            LogRoadTrackingChanged lrtc = logLine.Tag as LogRoadTrackingChanged;
                            _raceTracking.Deserialize(lrtc.NewState, this);
                            break;
                        case CatanAction.ChangedPlayerProperty:
                            LogPropertyChanged lpc = logLine.Tag as LogPropertyChanged;
                            logLine.PlayerData.GameData.SetKeyValue<PlayerGameData>(lpc.PropertyName, lpc.NewVal);
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

        private ConcurrentBag<KeyValuePair<string, string>> concurrentBag = new ConcurrentBag<KeyValuePair<string, string>>();
        private void OnTest(object sender, RoutedEventArgs rea)
        {
            //StaticHelpers.SetKeyValue<PlayerGameData>(PlayingPlayers[0].GameData, "TimesTargeted","10");
            concurrentBag.Add(new KeyValuePair<string, string>("TimesTargeted", "10"));
            concurrentBag.Add(new KeyValuePair<string, string>("UseLightFile", "True"));
            concurrentBag.Add(new KeyValuePair<string, string>("ColorAsString", "Yellow"));
            StartCallback();
        }

        private void StartCallback()
        {
            Task ignored = Task.Run(async () =>
            {
                while (!concurrentBag.IsEmpty)
                {
                    if (concurrentBag.TryTake(out KeyValuePair<string, string> kvp))
                    {

                        this.TraceMessage($"{kvp.Key}={kvp.Value}");
                        await Task.Delay(1000);
                    }
                    else
                    {
                        return;
                    }
                }

            });
        }

        private void OnGrowOrShrinkControls(object sender, RoutedEventArgs e)
        {
            GrowOrShrink(ControlGrid);
        }

        private void GrowOrShrink(UIElement el)
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
        }

        private void OnGrowOrShrinkRolls(object sender, RoutedEventArgs e)
        {
            GrowOrShrink(RollGrid);
        }

        private async void OnAssignNumbers(object sender, RoutedEventArgs e)
        {
            if (GameState != GameState.WaitingForStart)
            {
                return;
            }

            bool ret = await StaticHelpers.AskUserYesNoQuestion(string.Format($"Are you sure?  This will likely offend some people and annoy others."), "Yes", "No");
            if (!ret)
            {
                return;
            }

            await _gameView.RandomizeCatanBoard(true);
            _randomBoardList.Add(_gameView.RandomBoardSettings);

        }

        private const int SMALLEST_STATE_COUNT = 8; // can only undo to the first resource allocation


        private async Task OnWin()
        {

            bool ret = await StaticHelpers.AskUserYesNoQuestion(string.Format($"Did {CurrentPlayer.PlayerName} really win?"), "Yes", "No");
            if (ret == true)
            {
                try
                {
                    await PlayerWon();
                    await SetStateAsync(State.PlayerData, GameState.WaitingForNewGame, true);
                }
                catch (Exception e)
                {
                    MessageDialog dlg = new MessageDialog(string.Format($"Error in OnWin\n{e.Message}"));
                    await dlg.ShowAsync();
                }
            }
        }

        private async void OnManagePlayers(object sender, RoutedEventArgs e)
        {
            PlayerManagementDlg dlg = new PlayerManagementDlg(AllPlayers);
            if (await dlg.ShowAsync() == ContentDialogResult.Primary)
            {
                AllPlayers.Clear();
                AllPlayers.AddRange(dlg.PlayerDataList);

            }
        }

        private async void SavedGame_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems.Count > 0)
                {
                    Log newLog = e.AddedItems[0] as Log;
                    if (_log == newLog)
                    {
                        return;
                    }

                    if (await StaticHelpers.AskUserYesNoQuestion($"Switch to {newLog.File.DisplayName}?", "Yes", "No"))
                    {
                        _log = newLog;
                        await newLog.Parse(this);
                        await ReplayLog(newLog);
                        UpdateUiForState(_log.Last().GameState);
                    }
                    else
                    {
                        _lbGames.SelectedItem = _log;
                    }


                }
            }
            catch (Exception exception)
            {
                MessageDialog dlg = new MessageDialog($"Error loading file {e.AddedItems[0]}\nMessage:\n{exception.Message}");
            }


        }

        private void OnCloseSavedGames(object sender, RoutedEventArgs e)
        {
            _savedGameGrid.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        ///     This will go through all the buildings and find the ones that are
        ///        1. Buildable
        ///        2. in the "none" state (e.g. not already shown in some way) 
        ///      and then have them show the PipGroup.  
        ///      you have to do this every time because people might have built in locations that change the PipGroup
        /// </summary>
        private int _showPipGroupIndex = 0;
        private void OnShowPips(object sender, RoutedEventArgs e)
        {




        }

        private async void OnClearPips(object sender, RoutedEventArgs e)
        {

            await HideAllPipEllipses();
            _showPipGroupIndex = 0;

        }

        private async void OnTestGame(object sender, RoutedEventArgs e)
        {
            AnimationSpeedBase = 4; // speed up the animations
            await this.Reset();
            await SetStateAsync(null, GameState.WaitingForNewGame, true);
            _gameView.CurrentGame = _gameView.Games[0];

            _log = new Log();
            await _log.Init(CreateSaveFileName("Test"));
            SavedGames.Insert(0, _log);
            await AddLogEntry(null, GameState.GamePicked, CatanAction.SelectGame, true, LogType.Normal, 0);
            List<PlayerData> PlayerDataList = new List<PlayerData>
            {
                AllPlayers[0],
                AllPlayers[1],
                AllPlayers[2]
            };
            await StartGame(PlayerDataList);
        }

        public static string CreateSaveFileName(string Description)
        {
            DateTime dt = DateTime.Now;

            string ampm = dt.TimeOfDay.TotalMinutes > 720 ? "PM" : "AM";
            string min = dt.TimeOfDay.Minutes.ToString().PadLeft(2, '0');

            return string.Format($"{dt.TimeOfDay.Hours % 12}.{min} {ampm} - {Description}");
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

        private List<RandomBoardSettings> _randomBoardList = new List<RandomBoardSettings>();
        int _randomBoardListIndex = 0;
        private async void PickAGoodBoard(PointerRoutedEventArgs e)
        {


            if (e.GetCurrentPoint(this).Properties.MouseWheelDelta >= 0)
            {
                _randomBoardListIndex++;
                if (_randomBoardListIndex >= _randomBoardList.Count) 
                {

                    
                    //
                    //  get new ones
                    await _gameView.RandomizeCatanBoard(true);

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

            await _gameView.RandomizeCatanBoard(true, _randomBoardList[_randomBoardListIndex]);
        }
        DateTime _dt = DateTime.Now;
        private async void OnScrollMouseWheel(object sender, PointerRoutedEventArgs e)
        {
            DateTime dt = DateTime.Now;
            TimeSpan diff = DateTime.Now - _dt;
            if (diff.TotalSeconds < 1)
            {
                Debug.WriteLine($"Rejecting mousewheel call.  diff: {diff.TotalSeconds}");
                return;
            }

            _dt = dt;
            if (GameState == GameState.WaitingForStart)
            {
                PickAGoodBoard(e);
                return;
            }

            if (GameState != GameState.AllocateResourceForward && GameState != GameState.AllocateResourceReverse)
            {
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

                    foreach (BuildingCtrl building in list)
                    {
                        if (building.BuildingState == BuildingState.Pips)
                        {
                            await building.UpdateBuildingState(building.BuildingState, BuildingState.None, LogType.DoNotLog);
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

                foreach (BuildingCtrl building in list)
                {

                    if (building.Pips == 0)  // throw out the ones that have no pips
                    {
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
                    await building.UpdateBuildingState(building.BuildingState, BuildingState.Pips, LogType.Normal);
                }
            }
        }
        public bool BaronButtonChecked
        {
            get => (btn_BaronToggle.IsChecked == true);            
            set => btn_BaronToggle.IsChecked = false;
        }
        
    }
}


