using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Catan.Proxy;

using Windows.UI;
using Windows.UI.Xaml;

namespace Catan10
{
    public class MainPageModel : INotifyPropertyChanged
    {
        #region Properties + Fields 

        private string[] DynamicProperties { get; } = new string[] { "EnableNextButton", "EnableRedo", "StateMessage", "ShowBoardMeasurements", "ShowRolls", "EnableUndo", "GameState" };

        private bool _EnableUiInteraction = true;

        private int _FiveStarPositions = 0;

        private int _FourStarPosition = 0;

        private TradeResources _gameResources = new TradeResources();

        private bool _isServiceGame = false;

        private Log _newLog = null;

        private ObservableCollection<PlayerModel> _PlayingPlayers = new ObservableCollection<PlayerModel>();

        private Settings _Settings = new Settings();

        private int _ThreeStarPosition = 0;

        private int _TwoStarPosition = 0;

        private bool _WebSocketConnected = false;

        #endregion Properties + Fields 

        #region Methods

        private void InitBank()
        {
            Bank = new PlayerModel()
            {
                PlayerName = "Bank",
                ImageFileName = "ms-appx:Assets/bank.png",
                ForegroundColor = Colors.White,
                PrimaryBackgroundColor = Colors.Gold,
                SecondaryBackgroundColor = Colors.Black,
                PlayerIdentifier = Guid.Parse("{2B685447-31D9-4DCA-B29F-6FEC870E3ACC}")
            };
            Bank.GameData.Resources.TotalResourcesCollection.RemoveGold();
        }

        /// <summary>
        ///     We listent to changes from the Log.  We have "Dynamic Properties" which is where we apply logic to make decisions about what to show in the UI
        ///     if we add one of these properties (which are usually the get'ers only)
        /// </summary>
        private void Log_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DynamicProperties.ForEach((name) => NotifyPropertyChanged(name));
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UnspentEntitlements_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged("EnableNextButton");
            NotifyPropertyChanged("StateMessage");
        }

        internal void FinishedAddingPlayers()
        {
            //
            //   turn on NotifyPropertyChanged() events for the model and subscribe to the entitlements changed event so that
            //   we can enabled/disable the next button

            PlayingPlayers.ForEach((p) =>
            {
                p.GameData.NotificationsEnabled = true;
                p.GameData.Resources.UnspentEntitlements.CollectionChanged += UnspentEntitlements_CollectionChanged;
            }
            );
        }

        #endregion Methods

        #region Constructors

        #endregion Constructors

        #region Delegates  + Events + Enums

        #endregion Delegates  + Events + Enums

        public List<PlayerModel> AllPlayers { get; set; } = new List<PlayerModel>();

        [JsonIgnore]
        public PlayerModel Bank { get; private set; }


        [JsonIgnore]
        public ICatanService CatanService { get; set; }

        [JsonIgnore] public GameInfo DefaultGame { get; } = new GameInfo() { Name = "DefaultGame", Started = false, Creator = "", Id = Guid.Parse("F070F8DA-A5FD-4957-A528-0B915930123F") };
        public string DefaultUser { get; set; } = "";

        int _unprocessedMessages = 0;
        [JsonIgnore]
        public int UnprocessedMessages
        {
            get
            {
                return _unprocessedMessages;
            }
            set
            {
                Debug.Assert(value >= 0);
                if (_unprocessedMessages != value)
                {
                    _unprocessedMessages = value;
                    NotifyPropertyChanged();
                    DynamicProperties.ForEach((name) => NotifyPropertyChanged(name));
                    //  this.TraceMessage($"UnprocessedMessages: [Client={UnprocessedMessages}] [Service={CatanService.UnprocessedMessages}] ");
                }
            }
        }

        [JsonIgnore]
        public bool EnableNextButton
        {
            get
            {
                bool ret = false;
                GameState state = Log.GameState;
                try
                {
                    if (!EnableUiInteraction) return ret;

                    if (Log == null) return true;

                    Debug.Assert(UnprocessedMessages >= 0);

                    //
                    //  whevener this changes, the log changes...so you don't need to send a NotifyPropertyChanged() event...I hope...
                    if (UnprocessedMessages > 0)
                    {
                        // this.TraceMessage($"Enable false because UnprocessedMessages > 0: [Client={UnprocessedMessages}] [Service={CatanService.UnprocessedMessages}] ");
                        return false;
                    }

                    if (MainPage.Current.CurrentPlayer.GameData.Resources.UnspentEntitlements.Count > 0) return false;

                    if (state == GameState.PickingBoard || state == GameState.WaitingForPlayers)
                    {
                        ret = (MainPage.Current.TheHuman == this.GameStartedBy);
                        return ret;
                    }

                    if (state == GameState.WaitingForNext)
                    {
                        ret = (MainPage.Current.TheHuman == MainPage.Current.CurrentPlayer); // only the person whose turn it is can hit "Next"
                        return ret;
                    }

                    ret = (state == GameState.WaitingForNewGame || state == GameState.WaitingForNext || state == GameState.BeginResourceAllocation || state == GameState.PickingBoard ||
                            state == GameState.Supplemental || state == GameState.AllocateResourceForward || state == GameState.AllocateResourceReverse ||
                            state == GameState.DoneResourceAllocation || state == GameState.WaitingForPlayers);
                    return ret;
                }
                finally
                {
                    // this.TraceMessage($"[State={state}][EnableNextButton={ret}]");
                }
            }
        }

        [JsonIgnore]
        public bool EnableRedo
        {
            get
            {
                if (Log == null) return false;

                return (Log.CanRedo && MainPage.Current.CurrentPlayer.PlayerName == TheHuman);
            }
        }

        //    }
        //}
        [JsonIgnore]
        public bool EnableRolls
        {
            get
            {
                if (Log == null) return false;
                if ((Log.GameState == GameState.WaitingForRoll) || (Log.GameState == GameState.WaitingForRollForOrder)) return true;
                return false;
            }
        }

        //        return (EnableUiInteraction && (state == GameState.WaitingForNewGame || state == GameState.WaitingForNext || state == GameState.WaitingForStart || state == GameState.PickingBoard ||
        //                state == GameState.DoneSupplemental || state == GameState.Supplemental || state == GameState.AllocateResourceForward || state == GameState.AllocateResourceReverse ||
        //                state == GameState.DoneResourceAllocation || state == GameState.WaitingForPlayers));
        [JsonIgnore]
        public bool EnableUiInteraction
        {
            get
            {
                return _EnableUiInteraction;
            }
            set
            {
                if (_EnableUiInteraction != value)
                {
                    _EnableUiInteraction = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("EnableRedo");
                    NotifyPropertyChanged("EnableUdo");
                    NotifyPropertyChanged("EnableNextButton");
                }
            }
        }

        //        if (state == GameState.WaitingForNext || state == GameState.WaitingForRoll)
        //        {
        //            return (MainPage.Current.TheHuman == MainPage.Current.CurrentPlayer); // only the person whose turn it is can hit "Next"
        //        }
        [JsonIgnore]
        public bool EnableUndo
        {
            get
            {
                if (Log == null) return false;

                return (Log.CanUndo && MainPage.Current.CurrentPlayer.PlayerName == TheHuman);
            }
        }

        //        if (state == GameState.PickingBoard || state == GameState.WaitingForRollForOrder || state == GameState.WaitingForPlayers)
        //        {
        //            return (MainPage.Current.TheHuman == this.GameStartedBy);
        //        }
        [JsonIgnore]
        public int FiveStarPositions
        {
            get
            {
                return _FiveStarPositions;
            }
            set
            {
                if (_FiveStarPositions != value)
                {
                    _FiveStarPositions = value;
                    NotifyPropertyChanged();
                }
            }
        }

        //[JsonIgnore]
        //public Visibility CommandGridVisible
        //{
        //    get
        //    {
        //        if (Log == null) return Visibility;
        //        GameState state = Log.GameState;
        [JsonIgnore]
        public int FourStarPositions
        {
            get
            {
                return _FourStarPosition;
            }
            set
            {
                if (_FourStarPosition != value)
                {
                    _FourStarPosition = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public IGameController GameController { get; set; }

        [JsonIgnore]
        public TradeResources GameResources
        {
            get
            {
                return _gameResources;
            }
            set
            {
                if (_gameResources != value)
                {
                    _gameResources = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public PlayerModel GameStartedBy { get; internal set; }

        [JsonIgnore]
        public GameState GameState
        {
            get
            {
                if (Log == null) return GameState.WaitingForNewGame;

                return Log.GameState;
            }
        }
        [JsonIgnore]
        public bool IsServiceGame
        {
            get => _isServiceGame;
            set
            {
                if (value != _isServiceGame)
                {
                    _isServiceGame = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public Log Log
        {
            get
            {
                return _newLog;
            }
            set
            {
                if (_newLog != value)
                {
                    _newLog = value;
                    _newLog.LogChanged += Log_PropertyChanged;
                    NotifyPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public ObservableCollection<PlayerModel> PlayingPlayers
        {
            get
            {
                return _PlayingPlayers;
            }
            set
            {
                if (_PlayingPlayers != value)
                {
                    _PlayingPlayers = value;
                    NotifyPropertyChanged();
                }
            }
        }




        [JsonIgnore]
        public TradeResources ResourcesLeftInBank
        {
            get
            {
                TradeResources tr = new TradeResources();

                foreach (ResourceType resType in Enum.GetValues(typeof(ResourceType)))
                {
                    if (!TradeResources.GrantableResources(resType)) continue;

                    int total = 0;
                    PlayingPlayers.ForEach((p) => total += p.GameData.Resources.Current.CountForResource(resType));
                    total = MainPage.Current.GameContainer.CurrentGame.GameData.MaxResourceAllocated - total;
                    tr.AddResource(resType, total);
                }

                return tr;
            }
        }

        [JsonIgnore]
        public bool RollGridEnabled
        {
            get
            {
                if (Log == null) return true;
                GameState state = Log.GameState;

                return false;
            }
        }

        [JsonIgnore]
        public Visibility RollGridVisible
        {
            get
            {
                if (Log == null) return Visibility.Visible;
                if (Log.GameState == GameState.WaitingForNewGame) return Visibility.Visible;

                GameState state = Log.GameState;
                if (EnableUiInteraction && (state == GameState.WaitingForRoll || state == GameState.WaitingForPlayers)) return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        [JsonIgnore]
        public GameInfo ServiceGameInfo { get; set; }

        public Settings Settings
        {
            get
            {
                return _Settings;
            }
            set
            {
                if (_Settings != value)
                {
                    _Settings = value;
                    NotifyPropertyChanged();
                }
            }
        }

        
        public Visibility ShowBoardMeasurements(GameState gameState)
        {
            
            switch (gameState)
            {
                case GameState.WaitingForNewGame:
                case GameState.PickingBoard:
                case GameState.AllocateResourceForward:
                case GameState.AllocateResourceReverse:
                    return Visibility.Visible;
                default:
                    return Visibility.Collapsed;
            }
        }

        /// <summary>
        ///     Binding function for setting the visibility of the RollUi
        ///     Added WaitingForNewGame so that the setting of the grid positions works.
        /// </summary>
        /// <param name="gameState"></param>
        /// <returns></returns>
        public Visibility ShowRollUi(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.WaitingForNewGame:
                case GameState.WaitingForRoll:
                case GameState.WaitingForRollForOrder:
                    return Visibility.Visible;
                default:
                    return Visibility.Collapsed;
            }
           
        }
        /// <summary>
        /// When to show the TradeUi
        /// Added WaitingForNewGame so that the setting of the grid positions works.
        /// </summary>
        /// <param name="gameState"></param>
        /// <returns></returns>
        public Visibility ShowTradeUi(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.WaitingForNewGame:
                case GameState.WaitingForNext:
                    return Visibility.Visible;
                default:
                    return Visibility.Collapsed;
            }
        }

        [JsonIgnore]
        public string StateMessage
        {
            get
            {
                if (Log == null) return "New Game";
                return Log.GameState.Description();
            }
        }

        public string TheHuman { get; set; } = "";

        [JsonIgnore]
        public int ThreeStarPositions
        {
            get
            {
                return _ThreeStarPosition;
            }
            set
            {
                if (_ThreeStarPosition != value)
                {
                    _ThreeStarPosition = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public int TwoStarPositions
        {
            get
            {
                return _TwoStarPosition;
            }
            set
            {
                if (_TwoStarPosition != value)
                {
                    _TwoStarPosition = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public bool WebSocketConnected
        {
            get
            {
                return _WebSocketConnected;
            }
            set
            {
                if (_WebSocketConnected != value)
                {
                    _WebSocketConnected = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool IsGameStarted { get; internal set; }

        public MainPageModel()
        {
            InitBank();
            Log = new Log(MainPage.Current);
            GameController = MainPage.Current;
        }

        public event PropertyChangedEventHandler PropertyChanged;


        public void SetPipCount(int[] value)
        {
            FiveStarPositions = value[0];
            FourStarPositions = value[1];
            ThreeStarPositions = value[2];
            TwoStarPositions = value[3];
        }
    }
}
