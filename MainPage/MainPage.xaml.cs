using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

    public sealed partial class MainPage : Page
    {



        string _settingsFileName = "CatanSettings.ini";
        Settings _settings = new Settings();
        int _supplementalStartIndex = -1;

        public static readonly string SAVED_GAME_EXTENSION = ".log";
        public const string PlayerDataFile = "players.data";
        private const string SERIALIZATION_VERSION = "3";

        public ObservableCollection<Log> SavedGames { get; set; } = new ObservableCollection<Log>();



        DispatcherTimer _timer = new DispatcherTimer();

        bool _doDragDrop = false;
        int _currentPlayerIndex = 0; // the index into PlayingPlayers that is the CurrentPlayer


        public static MainPage Current;
        UserControl _currentView = null;
        Log _log = null;


        public ObservableCollection<PlayerData> PlayingPlayers { get; set; } = new ObservableCollection<PlayerData>();
        public ObservableCollection<PlayerData> AllPlayers { get; set; } = new ObservableCollection<PlayerData>();



        public static readonly DependencyProperty StateDescriptionProperty = DependencyProperty.Register("StateDescription", typeof(string), typeof(MainPage), new PropertyMetadata("Hit Start"));
        public static readonly DependencyProperty GameStateProperty = DependencyProperty.Register("GameState", typeof(GameState), typeof(MainPage), new PropertyMetadata(GameState.WaitingForNewGame));
        public static readonly DependencyProperty ActivePlayerBackgroundProperty = DependencyProperty.Register("ActivePlayerBackground", typeof(string), typeof(MainPage), new PropertyMetadata("Blue"));
        public static readonly DependencyProperty ActivePlayerForegroundProperty = DependencyProperty.Register("ActivePlayerForeground", typeof(string), typeof(MainPage), new PropertyMetadata("Blue"));
        public static readonly DependencyProperty ActivePlayerNameProperty = DependencyProperty.Register("ActivePlayerName", typeof(string), typeof(PlayerView), new PropertyMetadata("Nobody (hit +)"));

        #region RollProperties
        public static readonly DependencyProperty TotalRollsProperty = DependencyProperty.Register("TotalRolls", typeof(int), typeof(MainPage), new PropertyMetadata(0));
        public static readonly DependencyProperty TwoPercentProperty = DependencyProperty.Register("TwoPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty ThreePercentProperty = DependencyProperty.Register("ThreePercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty FourPercentProperty = DependencyProperty.Register("FourPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty FivePercentProperty = DependencyProperty.Register("FivePercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty SixPercentProperty = DependencyProperty.Register("SixPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty SevenPercentProperty = DependencyProperty.Register("SevenPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty EightPercentProperty = DependencyProperty.Register("EightPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty NinePercentProperty = DependencyProperty.Register("NinePercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty TenPercentProperty = DependencyProperty.Register("TenPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty ElevenPercentProperty = DependencyProperty.Register("ElevenPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty TwelvePercentProperty = DependencyProperty.Register("TwelvePercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));



        public string TwelvePercent
        {
            get
            {
                return (string)GetValue(TwelvePercentProperty);
            }
            set
            {
                SetValue(TwelvePercentProperty, value);
            }
        }
        public string ElevenPercent
        {
            get
            {
                return (string)GetValue(ElevenPercentProperty);
            }
            set
            {
                SetValue(ElevenPercentProperty, value);
            }
        }
        public string TenPercent
        {
            get
            {
                return (string)GetValue(TenPercentProperty);
            }
            set
            {
                SetValue(TenPercentProperty, value);
            }
        }

        public string NinePercent
        {
            get
            {
                return (string)GetValue(NinePercentProperty);
            }
            set
            {
                SetValue(NinePercentProperty, value);
            }
        }
        public string EightPercent
        {
            get
            {
                return (string)GetValue(EightPercentProperty);
            }
            set
            {
                SetValue(EightPercentProperty, value);
            }
        }
        public string SevenPercent
        {
            get
            {
                return (string)GetValue(SevenPercentProperty);
            }
            set
            {
                SetValue(SevenPercentProperty, value);
            }
        }
        public string SixPercent
        {
            get
            {
                return (string)GetValue(SixPercentProperty);
            }
            set
            {
                SetValue(SixPercentProperty, value);
            }
        }
        public string FivePercent
        {
            get
            {
                return (string)GetValue(FivePercentProperty);
            }
            set
            {
                SetValue(FivePercentProperty, value);
            }
        }
        public string FourPercent
        {
            get
            {
                return (string)GetValue(FourPercentProperty);
            }
            set
            {
                SetValue(FourPercentProperty, value);
            }
        }
        public string ThreePercent
        {
            get
            {
                return (string)GetValue(ThreePercentProperty);
            }
            set
            {
                SetValue(ThreePercentProperty, value);
            }
        }
        public string TwoPercent
        {
            get
            {
                return (string)GetValue(TwoPercentProperty);
            }
            set
            {
                SetValue(TwoPercentProperty, value);
            }
        }
        public int TotalRolls
        {
            get
            {
                return (int)GetValue(TotalRollsProperty);
            }
            set
            {
                SetValue(TotalRollsProperty, value);
            }
        }
        #endregion
        public string ActivePlayerName
        {
            get
            {
                return (string)GetValue(ActivePlayerNameProperty);
            }
            set
            {
                SetValue(ActivePlayerNameProperty, value);
            }
        }
        public string ActivePlayerBackground
        {
            get
            {
                return (string)GetValue(ActivePlayerBackgroundProperty);
            }
            set
            {
                SetValue(ActivePlayerBackgroundProperty, value);
                ActivePlayerForeground = StaticHelpers.BackgroundToForegroundDictionary[value];
            }
        }
        public string ActivePlayerForeground
        {
            get
            {
                return (string)GetValue(ActivePlayerForegroundProperty);
            }
            set
            {
                SetValue(ActivePlayerForegroundProperty, value);
            }
        }
        public string StateDescription
        {
            get
            {
                return (string)GetValue(StateDescriptionProperty);
            }
            set
            {
                SetValue(StateDescriptionProperty, value);
            }
        }


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

        }

        public static double GetAnimationSpeed(AnimationSpeed speed)
        {
            double baseSpeed = 2;
            if (Current != null)
                baseSpeed = (double)Current.AnimationSpeedBase;

            if (speed == AnimationSpeed.Ultra)
                return (double)speed;
            // AnimationSpeedFactor is a value of 1...4
            double d = (double)speed / ((double)baseSpeed + 2);
            return d;

        }

        void ShowNumberUi()
        {
            _daNumberOpacity.To = 1.0;
            _sbNumberOpacity.Begin();
            RollGrid.IsHitTestVisible = true;
        }
        void HideNumberUi()
        {
            _daNumberOpacity.To = 0;
            _sbNumberOpacity.Begin();
            RollGrid.IsHitTestVisible = false;
        }

        void ToggleNumberUi()
        {
            if (_daNumberOpacity.To == 1.0)
                HideNumberUi();
            else
                ShowNumberUi();
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


            }




        }



        private async Task LoadPlayerData()
        {

            try
            {

                var folder = await StaticHelpers.GetSaveFolder();
                var playersDictionary = await StaticHelpers.LoadSectionsFromFile(folder, PlayerDataFile);


                foreach (var kvp in playersDictionary)
                {

                    PlayerData p = new PlayerData();

                    p.Deserialize(kvp.Value, false);
                    await p.LoadImage();
                    if (p.PlayerIdentifier == Guid.Empty)
                        p.PlayerIdentifier = Guid.NewGuid();


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
            get
            {
                return _gameView.CurrentGame.GameType;

            }
            set
            {

            }
        }

        private async Task CopyScreenShotToClipboard(FrameworkElement element)
        {
            IRandomAccessStream stream = new InMemoryRandomAccessStream();
            var renderTargetBitmap = new Windows.UI.Xaml.Media.Imaging.RenderTargetBitmap();

            await renderTargetBitmap.RenderAsync(element);

            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

            var dpi = Windows.Graphics.Display.DisplayInformation.GetForCurrentView().LogicalDpi;

            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
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




        public bool HasSupplementalBuild
        {
            get
            {

                return GameType == GameType.SupplementalBuildPhase;
            }

        }




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
        }



        private async Task Reset()
        {
            _lbGames.SelectedValue = _log;
            await ResetTiles(true);
            PlayingPlayers.Clear();
            Ctrl_PlayerResourceCountCtrl.GameResourceData.Reset();
            _stateStack.Clear();

            foreach (var player in AllPlayers)
            {
                player.Reset();
            }

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
            await ProcessEnter(CurrentPlayer, "");


        }



        private void ResetDataForNewGame()
        {
            _gameView.Reset();
            _gameView.SetCallbacks(this, this);

            foreach (var p in PlayingPlayers)
            {
                p.GameData.OnCardsLost -= OnPlayerLostCards;
            }
            PlayingPlayers.Clear();
            _currentPlayerIndex = 0;
            _rolls.Clear();



        }
        bool _undoingCardLostHack = false;
        private async void OnPlayerLostCards(PlayerData player, int oldVal, int newVal)
        {
            if (_undoingCardLostHack) return;
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
            if (_gameView.CurrentGame.Tiles[0].TileOrientation == TileOrientation.FaceDown) return;

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
            _btnWinner.IsEnabled = false;
            _btnNextStep.IsEnabled = false;
            _btnUndo.IsEnabled = false;
            _btnNewGame.IsEnabled = true;
            _btnWinner.IsEnabled = false;
            switch (State.GameState)
            {
                case GameState.Uninitialized:
                case GameState.WaitingForNewGame:
                case GameState.Dealing:
                    break;
                case GameState.WaitingForStart:
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


        private void AddResourceCountForPlayer(PlayerData player, ResourceType resource, int count)
        {
            player.GameData.PlayerResourceData.AddResourceCount(resource, count); // update the player
            Ctrl_PlayerResourceCountCtrl.GameResourceData.AddResourceCount(resource, count); // update for the game
            if (player.GameData.GoodRoll == false)
            {
                player.GameData.GoodRoll = true;
                player.GameData.RollsWithResource++;

            }
            if (count > 0)
            {
                player.GameData.NoResourceCount = 0;
            }

        }

        public async Task<IReadOnlyList<StorageFile>> GetSavedFilesInternal()
        {
            var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, new[] { $"{SAVED_GAME_EXTENSION}" })
            {
                FolderDepth = FolderDepth.Shallow
            };
            var folder = await StaticHelpers.GetSaveFolder();
            var query = folder.CreateFileQueryWithOptions(queryOptions);
            var files = await query.GetFilesAsync();

            return files;
        }

        private const int MAX_SAVE_FILES_RETAINED = 5;

        private async Task LoadSavedGames()
        {

            IReadOnlyList<StorageFile> files = await GetSavedFilesInternal();
            List<StorageFile> fList = new List<StorageFile>();
            fList.AddRange(files);

            var sort = from s in files orderby s.DateCreated.Ticks descending select s;
            SavedGames.Clear();
            foreach (var f in sort)
            {
                Log log = new Log(f);
                SavedGames.Add(log);
            }

        }

        public async static Task<bool> SavePlayers(IEnumerable<PlayerData> players, string fileName)
        {

            var folder = await StaticHelpers.GetSaveFolder();
            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            string toWrite = "";
            foreach (PlayerData p in players)
            {
                toWrite += String.Format($"[{p.PlayerName}]{StaticHelpers.lineSeperator}{p.Serialize(false)}\n");
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
            if (Int32.TryParse(inputText, out roll))
            {
                return await ProcessRoll(roll);

            }

            return false;
        }



        private void UpdateChart()
        {
            int[] allRolls = RollCount(AllRolls);
            double[] allPercent = RollPercents(AllRolls, allRolls);
            int[] playerRolls = RollCount(CurrentPlayerRolls);
            double[] playerPercent = RollPercents(CurrentPlayerRolls, playerRolls);

            //String.Format("{0:0.#}%", percent * 100)
            TotalRolls = AllRolls.Count();

            TwoPercent = String.Format($"{allRolls[0]} ({allPercent[0] * 100:0.#}%)");
            ThreePercent = String.Format($"{allRolls[1]} ({allPercent[1] * 100:0.#}%)");
            FourPercent = String.Format($"{allRolls[2]} ({allPercent[2] * 100:0.#}%)");
            FivePercent = String.Format($"{allRolls[3]} ({allPercent[3] * 100:0.#}%)");
            SixPercent = String.Format($"{allRolls[4]} ({allPercent[4] * 100:0.#}%)");
            SevenPercent = String.Format($"{allRolls[5]} ({allPercent[5] * 100:0.#}%)");
            EightPercent = String.Format($"{allRolls[6]} ({allPercent[6] * 100:0.#}%)");
            NinePercent = String.Format($"{allRolls[7]} ({allPercent[7] * 100:0.#}%)");
            TenPercent = String.Format($"{allRolls[8]} ({allPercent[8] * 100:0.#}%)");
            ElevenPercent = String.Format($"{allRolls[9]} ({allPercent[9] * 100:0.#}%)");
            TwelvePercent = String.Format($"{allRolls[10]} ({allPercent[10] * 100:0.#}%)");


        }

        private async Task OnNext(int playersToMove = 1, LogType logType = LogType.Normal)
        {


            await AnimatePlayers(playersToMove, logType);


            UpdateChart();
            foreach (TileCtrl t in _gameView.AllTiles)
            {
                t.ResetOpacity();
                t.ResetTileRotation();

            }
            //
            //  on next, reset the resources for the turn to 0
            foreach (var player in PlayingPlayers)
            {
                player.GameData.PlayerResourceData.Reset();
            }


        }

        //
        //  we use the build ellipses during the allocation phase to see what settlements have the most pips
        //  when we move to the next player, hide the build ellipses

        private void HideAllPipEllipses()
        {
            foreach (var s in _gameView.CurrentGame.HexPanel.Buildings)
            {
                if (s.BuildingState == BuildingState.Pips) s.BuildingState = BuildingState.None;
            }
        }



        //
        //  helper function so I don't have to check the array length each time
        private bool GetAndVerifyNumber(string[] tokens, int index, out int value)
        {
            value = -1;
            if (tokens.Length < index)
                return false;
            return Int32.TryParse(tokens[index], out value);
        }



        //
        //  n is verified by the caller
        //  this adds a number to the list we use to keep track of what is rolled and updates all the statistics
        private async Task HandleNumber(int val)
        {



            bool ret = this.PushRoll(val); // only returns false on a number outside the range...
            if (!ret)
                return;

            List<TileCtrl> tilesWithNumber = new List<TileCtrl>();
            UpdateChart();

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
                        t.Rotate(180, tasks, true);

                }
                else
                {
                    if (_settings.AnimateFade)
                        t.AnimateFade(0.25, tasks);
                }
            }

            if (tasks.Count > 0)
                await Task.WhenAll(tasks.ToArray());

            //
            // now make sure we reverse the fade
            _timer.Start();


            await AddLogEntry(CurrentPlayer, GameState.WaitingForRoll, CatanAction.Rolled, true, LogType.Normal, val);

            if (val == 7)
            {
                CurrentPlayer.GameData.MovedBaronAfterRollingSeven = false;
                await SetStateAsync(CurrentPlayer, GameState.MustMoveBaron, false);
                foreach (var player in PlayingPlayers)
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
            var ret = await dlg.ShowAsync();

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
                    return null;

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
                    return GameState.WaitingForNewGame;

                return _log.Last().GameState;
            }

        }

        public async Task AddLogEntry(PlayerData player, GameState state, CatanAction action, bool stopProcessingUndo, LogType logType = LogType.Normal, int number = -1, object tag = null, [CallerFilePath] string filePath = "", [CallerMemberName] string name = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (_log == null) return;
            await _log.AppendLogLine(new LogEntry(player, state, action, number, stopProcessingUndo, logType, tag, name, lineNumber, filePath));
        }


        private async Task SetStateAsync(PlayerData player, GameState newState, bool stopUndo, LogType logType = LogType.Normal, [CallerFilePath] string filePath = "", [CallerMemberName] string name = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (_log == null)
                return;

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
            _progress.IsActive = true;
            _progress.Visibility = Visibility.Visible;
            await Task.Delay(0);
            await this.Reset();
            ResetDataForNewGame();
            int n = 0;

            log.Replaying = true;

            foreach (LogEntry logLine in log.LogEntries)
            {
                n++;
                if (logLine.LogType == LogType.Undo)
                {
                    await UndoLogLine(logLine, true);
                    continue;
                }
                switch (logLine.Action)
                {
                    case CatanAction.Rolled:
                        PushRoll(logLine.Number);
                        UpdateChart();
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
                        if (logLine.Tag == null) continue;
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
                            roadUpdate.Road.Color = CurrentPlayer.Background;
                        else
                            roadUpdate.Road.Color = Colors.Transparent;

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
                    case CatanAction.AssignRandomNumbersToTileGroup:
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
                    default:
                        break;
                }



            }


            _gameView.FlipAllAsync(TileOrientation.FaceUp);
            _progress.IsActive = false;
            _progress.Visibility = Visibility.Collapsed;
            log.Replaying = false;
            return true;


        }

        private void OnTest(object sender, RoutedEventArgs rea)
        {
            // Frame.Navigate(typeof(TestPage));
            // _gameView.CalculateAdjacentSettlements();
            //_gameView.PirateShipTile = _gameView.AllTiles[18];
            // _gameView.BaronTile = _gameView.AllTiles.Last();
            // _gameView.OnTest();


            //take 2
            //string fileName = "replace this";
            //Log newLog = await _log.LoadLog(fileName, this);
            //await ReplayLog(newLog);
            //_log = newLog;



            //ValidateBuilding = true;
#if false
            #region probability testing
            string s = "";

            foreach (ResourceType res in Enum.GetValues(typeof(ResourceType)))
            {
                if (_gameView.Probabilities[res] == 0)
                    continue;

                if (res == ResourceType.Sea || res == ResourceType.Desert) continue;

                string tabs = "\t\t";
                if (res == ResourceType.GoldMine) tabs = "\t";
                s += String.Format($"{res}:{tabs}{_gameView.Probabilities[res]}\n");
                
            }
            await StaticHelpers.ShowErrorText(s);
            #endregion
#endif


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
            await _gameView.RandomizeCatanBoard(true);
            await SetStateAsync(CurrentPlayer, GameState.WaitingForStart, true);
            if (CurrentPlayer != null) await ProcessEnter(CurrentPlayer, "");
        }

        private const int SMALLEST_STATE_COUNT = 1; // game starts with NewGame and then Deal


        private async Task OnWin()
        {

            var ret = await StaticHelpers.AskUserYesNoQuestion(String.Format($"Did {CurrentPlayer.PlayerName} really win?"), "Yes", "No");
            if (ret == true)
            {
                try
                {
                    await PlayerWon();
                    await SetStateAsync(State.PlayerData, GameState.WaitingForNewGame, true);
                }
                catch (Exception e)
                {
                    MessageDialog dlg = new MessageDialog(String.Format($"Error in OnWin\n{e.Message}"));
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
                        return;
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
        private async void OnShowPips(object sender, RoutedEventArgs e)
        {
            _showPipGroupIndex++;
            List<BuildingCtrl> buildingsOrderedByPips = new List<BuildingCtrl>(_gameView.CurrentGame.HexPanel.Buildings);
            buildingsOrderedByPips.Sort((s1, s2) => s2.Pips - s1.Pips);
           
            int pipCountToShow = buildingsOrderedByPips[0].Pips;
            bool shownOne = false;
            foreach (var building in buildingsOrderedByPips)
            {
                //
                //  keep going until the pip count changes - but we have to show at least one
                if (pipCountToShow != building.Pips  && shownOne)
                {
                    break;
                }

                if (building.Pips == 0)  // throw out the ones that have no pips
                {
                    building.PipGroup = -1;
                    continue; // outside the main map or a desert next to nothing
                }

                if (ValidateBuildingLocation(building, out bool showerror) == false) // throw out the ones you can't build in
                {
                    continue;
                }

                if (building.BuildingState != BuildingState.None) continue;  // throw out the non-empty ones

                //
                //  if we've got here, we can build on this location and we need to show everythign tha thas this pipcount
                if (!shownOne)
                {
                    pipCountToShow = building.Pips;
                    shownOne = true;
                }

                building.PipGroup = _showPipGroupIndex;
                await building.UpdateBuildingState(building.BuildingState, BuildingState.Pips, LogType.Normal);
                
            }



        }

        private void OnClearPips(object sender, RoutedEventArgs e)
        {

            HideAllPipEllipses();
            _showPipGroupIndex = 0;

        }
    }
}


