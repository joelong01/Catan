using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

using Catan.Proxy;

using Windows.UI.Xaml;

namespace Catan10
{
    public class MainPageModel : INotifyPropertyChanged
    {
        private string[] DynamicProperties { get; } = new string[] { "EnableNextButton", "EnableRedo", "StateMessage", "ShowBoardMeasurements", "ShowRolls", "EnableUndo" };

        [JsonIgnore]
        private Dictionary<GameState, string> StateMessages { get; } = new Dictionary<GameState, string>()
        {
               {GameState.Uninitialized, "" },
               {GameState.WaitingForNewGame, "New Game" },
               {GameState.WaitingForPlayers, "Pick Board" },  // you stay in this state until you hit the button.  while in this state, the button stays this...
               {GameState.PickingBoard, "Roll for Order" },
               {GameState.WaitingForRollForOrder, "Select Roll!" },
               {GameState.BeginResourceAllocation, "Start Game" },
               {GameState.AllocateResourceForward, "Next (Forward)" },
               {GameState.AllocateResourceReverse, "Next (Back)" },
               {GameState.DoneResourceAllocation, "Click To Start Game" },
               {GameState.WaitingForRoll, "Select Roll" },
               {GameState.WaitingForNext, "End My Turn" },
               {GameState.Supplemental, "Finished (Next)" }
        };

        private bool _EnableUiInteraction = true;
        private int _FiveStarPositions = 0;
        private int _FourStarPosition = 0;
        private TradeResources _gameResources = new TradeResources(); // the total number of resources that have been handed out in this game
        private string _HostName = "http://192.168.1.128:5000";
        private bool _isServiceGame = false;
        private Log _newLog = null;
        private ObservableCollection<PlayerModel> _PlayingPlayers = new ObservableCollection<PlayerModel>();
        private Settings _Settings = new Settings();
        private int _ThreeStarPosition = 0;
        private int _TwoStarPosition = 0;
        private bool _WebSocketConnected = false;

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

        public MainPageModel()
        {
        }

        public MainPageModel(IGameController gameController)
        {
            Log = new Log(gameController);
            GameController = gameController;
        }

        public List<PlayerModel> AllPlayers { get; set; } = new List<PlayerModel>();
        public string DefaultUser { get; set; } = "";

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

                    if (MainPage.Current.CurrentPlayer.GameData.Resources.UnspentEntitlements.Count > 0) return false;

                    if (state == GameState.PickingBoard || state == GameState.WaitingForPlayers)
                    {
                        ret = (MainPage.Current.TheHuman == this.GameStartedBy);
                        return ret;
                    }

                    if (state == GameState.WaitingForNext || state == GameState.WaitingForRoll)
                    {
                        ret = (MainPage.Current.TheHuman == MainPage.Current.CurrentPlayer); // only the person whose turn it is can hit "Next"
                        return ret;
                    }

                    ret = (state == GameState.WaitingForNewGame || state == GameState.WaitingForNext || state == GameState.BeginResourceAllocation || state == GameState.PickingBoard ||
                            state == GameState.DoneSupplemental || state == GameState.Supplemental || state == GameState.AllocateResourceForward || state == GameState.AllocateResourceReverse ||
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

        public string HostName
        {
            get
            {
                return _HostName;
            }
            set
            {
                if (_HostName != value)
                {
                    _HostName = value;
                    NotifyPropertyChanged();
                }
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
        public CatanProxy Proxy { get; } = new CatanProxy() { HostName = "http://192.168.1.128:5000" };

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

        [JsonIgnore]
        public Visibility ShowBoardMeasurements
        {
            get
            {
                if (Log == null) return Visibility.Visible;
                if (Log.GameState == GameState.WaitingForNewGame) return Visibility.Visible;
                if (Log.GameState == GameState.PickingBoard) return Visibility.Visible;
                if (Log.GameState == GameState.AllocateResourceForward || Log.GameState == GameState.AllocateResourceReverse) return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        [JsonIgnore]
        public Visibility ShowRolls
        {
            get
            {
                Visibility visibility = Visibility.Visible;
                try
                {
                    if (Log == null) return visibility;
                    if (Log.GameState == GameState.WaitingForNewGame || Log.GameState == GameState.WaitingForRoll || Log.GameState == GameState.BeginResourceAllocation ||
                        Log.GameState == GameState.WaitingForRollForOrder || Log.GameState == GameState.WaitingForNext) return visibility;

                    visibility = Visibility.Collapsed;
                    return visibility;
                }
                finally
                {
                    // this.TraceMessage($"ShowRolls:[State={Log.GameState}] [Visibility={visibility}]");
                }
            }
        }

        [JsonIgnore]
        public string StateMessage
        {
            get
            {
                if (Log == null) return "New Game";

                if (StateMessages.TryGetValue(Log.GameState, out string msg))
                {
                    // this.TraceMessage($"[State={Log.GameState}][StateMessage={msg}]");
                    return msg;
                }

                return $"GameStart.{Log.GameState}";
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
