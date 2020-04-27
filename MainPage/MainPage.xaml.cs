using Catan.Proxy;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Networking.Sockets;
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
        public PlayerModel TheHuman { get; set; } = null;
        public SavedState SavedAppState { get; set; } = null;
        private int _supplementalStartIndex = -1;
        public static readonly string SAVED_GAME_EXTENSION = ".log";
        public const string PlayerDataFile = "catansettings.json";
        
        public ObservableCollection<Log> SavedGames { get; set; } = new ObservableCollection<Log>();
        
        private readonly DispatcherTimer _timer = new DispatcherTimer();  // flips tiles back to Opacaticy = 0
        private bool _doDragDrop = false;   // this lets you double tap a map and then move it around
        private int _currentPlayerIndex = 0; // the index into PlayingPlayers that is the CurrentPlayer
        public static MainPage Current { get; private set; } // a global for the game
        private readonly RoadRaceTracking _raceTracking = null; // used to calculate longest road -- whoever gets their first wins LR, and it has to work if an Undo action ahppanes
        //  State for MainPage -- the thought was to move all save/load state into one place...but that work hasn't finished
        public static readonly DependencyProperty MainPageModelProperty = DependencyProperty.Register("MainPageModel", typeof(MainPageModel), typeof(MainPage), new PropertyMetadata(new MainPageModel()));

        public MainPageModel MainPageModel
        {
            get => (MainPageModel)GetValue(MainPageModelProperty);
            set => SetValue(MainPageModelProperty, value);
        }


        public MainPage()
        {
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            this.InitializeComponent();

            Current = this;
            this.DataContext = this;
            _timer.Tick += AsyncReverseFade;
            _raceTracking = new RoadRaceTracking(this);
            Ctrl_PlayerResourceCountCtrl.Log = this;
            MainPageModel.IsServiceGame = true; // for TESTING!!
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

                _gameView.Init(this, this);
                CreateMenuItems();

                await LoadGameData();
                UpdateGridLocations();
                _progress.Visibility = Visibility.Collapsed;
                _progress.IsActive = false;

                Ctrl_PlayerResourceCountCtrl.MainPage = this;
            }

            InitTest();

        }

        private Task SaveSettings()
        {
            return SaveGameState(SavedAppState);
        }

        private async Task SaveGameState(SavedState state)
        {
            StorageFolder folder = await StaticHelpers.GetSaveFolder();


            var content = JsonSerializer.Serialize<SavedState>(state);

            StorageFile file = await folder.CreateFileAsync(PlayerDataFile, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, content);
        }

        private async Task<SavedState> LoadGameState()
        {
            StorageFolder folder = await StaticHelpers.GetSaveFolder();
            string content = await StaticHelpers.ReadWholeFile(folder, PlayerDataFile);
            SavedState state;

            if (String.IsNullOrEmpty(content))
            {
                return null;
            }
            try
            {
               
                state = CatanProxy.Deserialize<SavedState>(content);

                _timer.Interval = TimeSpan.FromSeconds(state.Settings.FadeSeconds);

                return state;
            }
            catch (JsonException j)
            {
                this.TraceMessage($"JSON error: {j}");
            }

            return null;

        }

        /// <summary>
        ///     Load Data that is global to the game 
        ///     1. Players
        ///     2. Settings
        ///     3. Service settings
        /// </summary>
        /// <returns></returns>
        private async Task LoadGameData()
        {


            SavedAppState = await LoadGameState();
            if (SavedAppState == null)
            {
                var list = await GetDefaultUsers();
                
                SavedAppState = new SavedState()
                {
                    Players = list,
                    Settings = new Settings(),
                    ServiceState = new ServiceState() { HostName = "localhost:5000" }

                };
                await SaveGameState(SavedAppState);
                SavedAppState = await LoadGameState(); // just verifying round trip...
                Debug.Assert(SavedAppState != null);
            }

            

            foreach (var player in SavedAppState.Players)
            {
                if (player.PlayerIdentifier == Guid.Empty)
                {
                    player.PlayerIdentifier = Guid.NewGuid();
                }
                player.Log = this;
                await player.LoadImage();
                player.AllPlayerIndex = SavedAppState.Players.Count;
                if (SavedAppState.DefaultPlayerName == player.PlayerName)
                {
                    TheHuman = player;
                }

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




        private async Task VisualShuffle(bool randomize = true)
        {
            await _gameView.VisualShuffle(randomize);
            _randomBoardList.Clear();
            _randomBoardList.Add(_gameView.RandomBoardSettings);
            _randomBoardListIndex = 0;

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

            MainPageModel = new MainPageModel();



            _lbGames.SelectedValue = MainPageModel.Log;
            await ResetTiles(true);
            Ctrl_PlayerResourceCountCtrl.GlobalResourceCount.TurnReset();
            _stateStack.Clear();

            foreach (PlayerModel player in SavedAppState.Players)
            {
                player.Reset();
            }

            _raceTracking.Reset();
            
            //  await LoadPlayerData();

        }

        /// <summary>
        /// Update this because you did the sorting work in the dialog
        /// hide all positions and then loop through the array to make them visible
        /// 
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>

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

            var model = StartGameController.StartGame(this, selectedIndex, GameContainer.RandomBoardSettings, MainPageModel.PlayingPlayers);
            VerifyRoundTrip<StartGameModel>(model);
            NewLog.PushAction(model);
        }



        private void ResetDataForNewGame()
        {
            _gameView.Reset();
            _gameView.SetCallbacks(this, this);

            foreach (PlayerModel p in MainPageModel.PlayingPlayers)
            {
                p.GameData.OnCardsLost -= OnPlayerLostCards;
            }
            MainPageModel.PlayingPlayers.Clear();
            _currentPlayerIndex = 0;
            Rolls.Clear();



        }

        private bool _undoingCardLostHack = false;
        private async void OnPlayerLostCards(PlayerModel player, int oldVal, int newVal)
        {
            if (_undoingCardLostHack)
            {
                return;
            }

            await LogPlayerLostCards(player, oldVal, newVal, LogType.Normal);
        }

        private async Task LogPlayerLostCards(PlayerModel player, int oldVal, int newVal, LogType logType)
        {
            await AddLogEntry(player, GameState, CatanAction.CardsLost, true, logType, -1, new LogCardsLost(oldVal, newVal));
        }


        private async Task AddPlayer(PlayerModel pData, LogType logType)
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
            await AddLogEntry(pData, GameState.Starting, CatanAction.AddPlayer, false, logType, MainPageModel.PlayingPlayers.Count, pData.AllPlayerIndex); // can't undo adding players...

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


        private async Task ProcessEnter(PlayerModel player, string inputText)
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
                        //
                        //  TODO: This is where you call the service to tell it what state to start the game in
                        //        Only the starting player should be able to hit "Start"  
                        string boardSettings = _gameView.RandomBoardSettings.Serialize();
                        this.TraceMessage(boardSettings);
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

        private async Task<bool> ProcessRoll(int roll)
        {
            if (roll >= 2 && roll <= 12)
            {
                await HandleNumber(roll);
                return true;
            }

            return false;
        }

        public async Task<bool> ProcessRoll(string inputText)
        {
            if (int.TryParse(inputText, out int roll))
            {
                return await ProcessRoll(roll);

            }

            return false;
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

        public List<int> GetRandomGoldTiles()
        {
            if (!this.RandomGold || this.RandomGoldTileCount < 1) return new List<int>();
            var currentRandomGoldTiles = _gameView.GetCurrentRandomGoldTiles();
            return _gameView.PickRandomTilesToBeGold(RandomGoldTileCount, currentRandomGoldTiles);
        }

        public async Task SetRandomTileToGold(List<int> goldTilesIndices)
        {
            await _gameView.ResetRandomGoldTiles();
            if (this.RandomGold && this.RandomGoldTileCount > 0)
            {
                await _gameView.SetRandomTilesToGold(goldTilesIndices);
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
            GameState oldState = GameState;

            var currentRandomGoldTiles = _gameView.GetCurrentRandomGoldTiles();
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

            UpdateUiForState(newState);
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
                tile.HighlightTile(CurrentPlayer.GameData.BackgroundBrush); // shows what was rolled                                
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

        public async Task<List<TileCtrl>> HighlightRolledTiles(int rolledNumber)
        {
            List<TileCtrl> tilesWithNumber = new List<TileCtrl>();


            List<Task> tasks = new List<Task>();
            foreach (TileCtrl t in _gameView.CurrentGame.Tiles)
            {

                if (t.Number == rolledNumber)

                {
                    tilesWithNumber.Add(t);
                    if (SavedAppState.Settings.AnimateFade)
                    {

                        t.AnimateFade(1.0, tasks);

                    }
                    if (SavedAppState.Settings.RotateTile)
                    {
                        t.Rotate(180, tasks, true);
                    }
                }
                else
                {
                    if (SavedAppState.Settings.AnimateFade)
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
            return tilesWithNumber;
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
                if (MainPageModel.Log.ActionCount == 0)
                {
                    return null;
                }

                return MainPageModel.Log.Last();
            }
        }

        public GameState NewGameState
        {
            get
            {
                return NewLog.GameState;
            }
        }

        public GameState GameState
        {
            get
            {
                if (MainPageModel.Log == null)
                {
                    return GameState.WaitingForNewGame;
                }

                if (MainPageModel.Log.ActionCount == 0)
                {
                    return GameState.WaitingForNewGame;
                }

                return MainPageModel.Log.Last().GameState;
            }

        }

        public async Task AddLogEntry(PlayerModel player, GameState state, CatanAction action, bool stopProcessingUndo, LogType logType = LogType.Normal, int number = -1, object tag = null, [CallerFilePath] string filePath = "", [CallerMemberName] string name = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (MainPageModel.Log == null)
            {
                return;
            }

            if (state == GameState.Unknown)
            {
                state = this.GameState;
            }

            await MainPageModel.Log.AppendLogLine(new LogEntry(player, state, action, number, stopProcessingUndo, logType, tag, name, lineNumber, filePath));

        }
        public void PostLogEntry(PlayerModel player, GameState state, CatanAction action, bool stopProcessingUndo, LogType logType = LogType.Normal, int number = -1, object tag = null, [CallerFilePath] string filePath = "", [CallerMemberName] string name = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (MainPageModel.Log == null)
            {
                return;
            }

            if (state == GameState.Unknown)
            {
                state = this.GameState;
            }

            MainPageModel.Log.AppendLogLineNoDisk(new LogEntry(player, state, action, number, stopProcessingUndo, logType, tag, name, lineNumber, filePath));

        }

        public async Task SetStateAsync(PlayerModel playerData, GameState newState, bool stopUndo, LogType logType = LogType.Normal, [CallerFilePath] string filePath = "", [CallerMemberName] string cmn = "", [CallerLineNumber] int lineNumber = 0)
        {
            if (MainPageModel.Log == null)
            {
                return;
            }

            LogStateTranstion lst = new LogStateTranstion(GameState, newState);
            await MainPageModel.Log.AppendLogLine(new LogEntry(playerData, newState, CatanAction.ChangedState, -1, stopUndo, logType, lst, cmn, lineNumber, filePath));
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
                        case CatanAction.RandomizeBoard:
                            RandomBoardSettings boardSetting = logLine.Tag as RandomBoardSettings;
                            await _gameView.RandomizeCatanBoard(true, boardSetting);
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


        public async Task<List<TileCtrl>> PlayRollAnimation(int rolledNumber)
        {
            List<TileCtrl> tilesWithNumber = new List<TileCtrl>();
            List<Task> tasks = new List<Task>();
            foreach (TileCtrl t in _gameView.CurrentGame.Tiles)
            {

                if (t.Number == rolledNumber)

                {
                    tilesWithNumber.Add(t);
                    if (SavedAppState.Settings.AnimateFade)
                    {

                        t.AnimateFade(1.0, tasks);

                    }
                    if (SavedAppState.Settings.RotateTile)
                    {
                        t.Rotate(180, tasks, true);
                    }
                }
                else
                {
                    if (SavedAppState.Settings.AnimateFade)
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

            return tilesWithNumber;
        }

        private async void OnWebSocketTest(object sdr, RoutedEventArgs rea)
        {
            /* using (var client = new HttpClient())
             {
                 client.BaseAddress = new Uri("http://localhost:8080/");
                 client.DefaultRequestHeaders.Accept.Clear();
                 //GET Method  
                 HttpResponseMessage response = await client.GetAsync("/roll/");
                 if (response.IsSuccessStatusCode)
                 {
                     var resp = await response.Content.ReadAsStringAsync();
                     Debug.WriteLine(resp);
                 }
                 else
                 {
                     Debug.WriteLine("Internal server Error");
                 }
             }
 */


            MessageWebSocket messageWebSocket = new MessageWebSocket();
            Uri catanRelay = new Uri("ws://localhost/ws");


            // In this example, we send/receive a string, so we need to set the MessageType to Utf8.
            messageWebSocket.Control.MessageType = SocketMessageType.Utf8;

            messageWebSocket.MessageReceived += (sender, args) =>
            {
                try
                {
                    using (DataReader dataReader = args.GetDataReader())
                    {
                        dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                        string message = dataReader.ReadString(dataReader.UnconsumedBufferLength);
                        Debug.WriteLine("Message received from MessageWebSocket: " + message);
                        messageWebSocket.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Windows.Web.WebErrorStatus webErrorStatus = Windows.Networking.Sockets.WebSocketError.GetStatus(ex.GetBaseException().HResult);
                    // Add additional code here to handle exceptions.
                }
            };
            messageWebSocket.Closed += (s, a) =>
            {
                Debug.WriteLine("Socket closed");
            };

            try
            {
                await messageWebSocket.ConnectAsync(catanRelay);
                using (var dataWriter = new DataWriter(messageWebSocket.OutputStream))
                {
                    dataWriter.WriteString("This is the first fucking message!");
                    await dataWriter.StoreAsync();
                    dataWriter.DetachStream();
                }
            }
            catch (Exception ex)
            {
                Windows.Web.WebErrorStatus webErrorStatus = Windows.Networking.Sockets.WebSocketError.GetStatus(ex.GetBaseException().HResult);
                // Add additional code here to handle exceptions.
            }

        }

        private async void OnGrowOrShrinkControls(object sender, RoutedEventArgs e)
        {
            await GrowOrShrink(ControlGrid);

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

        private async void OnGrowOrShrinkRolls(object sender, RoutedEventArgs e)
        {
            await GrowOrShrink(RollGrid);
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
            PlayerManagementDlg dlg = new PlayerManagementDlg(SavedAppState.Players, this);
            if (await dlg.ShowAsync() == ContentDialogResult.Primary)
            {
                SavedAppState.Players.Clear();
                SavedAppState.Players.AddRange(dlg.PlayerDataList);
                await SaveGameState(SavedAppState);
            }
        }

        private async void SavedGame_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems.Count > 0)
                {
                    Log newLog = e.AddedItems[0] as Log;
                    if (MainPageModel.Log == newLog)
                    {
                        return;
                    }

                    if (await StaticHelpers.AskUserYesNoQuestion($"Switch to {newLog.File.DisplayName}?", "Yes", "No"))
                    {
                        MainPageModel.Log = newLog;
                        //   await newLog.Parse(this);
                        // TODO:...
                        await ReplayLog(newLog);
                        UpdateUiForState(MainPageModel.Log.Last().GameState);
                    }
                    else
                    {
                        _lbGames.SelectedItem = MainPageModel.Log;
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


        private void OnRedoPossible(bool redo)
        {
            EnableRedo = redo;
        }

        private async Task PickSettlementsAndRoads()
        {
            while (GameState == GameState.AllocateResourceForward || GameState == GameState.AllocateResourceReverse)
            {
                await SetBuildingAndRoad();
                // move to next
                GameState oldState = GameState;
                await NextState();
                //
                //  we do it this way because the last player goes twice in the same state
                //
                if (oldState == GameState.AllocateResourceForward && GameState == GameState.AllocateResourceReverse)
                {
                    await SetBuildingAndRoad();
                }
            }
        }

        private async Task SetBuildingAndRoad()
        {
            // pick a tile with the highest pips and put a settlement on it
            var building = GetHighestPipsBuilding();
            await building.UpdateBuildingState(building.BuildingState, BuildingState.Settlement);

            // pick a Random Road
            var road = building.AdjacentRoads[testRandom.Next(building.AdjacentRoads.Count)];
            road.Color = CurrentPlayer.GameData.PlayerColor;
            await UpdateRoadState(road, road.RoadState, RoadState.Road, LogType.Normal);
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

        private readonly List<RandomBoardSettings> _randomBoardList = new List<RandomBoardSettings>();
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
            if (diff.TotalSeconds < 0.1)
            {
                this.TraceMessage($"Rejecting mousewheel call.  diff: {diff.TotalSeconds}");
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
                    if (list.Count == 0)
                    {
                        Debug.WriteLine($"{hideIndex} doesn't have any buildings");
                    }
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
                    await building.UpdateBuildingState(building.BuildingState, BuildingState.Pips, LogType.DoNotLog);
                }
            }
        }

        private async void PickSettlementsAndRoads(object sender, RoutedEventArgs e)
        {

            await PickSettlementsAndRoads();
        }


    }
}


