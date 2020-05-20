using Catan.Proxy;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Windows.UI.Xaml;

namespace Catan10
{
   


    public class MainPageModel : INotifyPropertyChanged
    {
        [JsonIgnore]
        private Dictionary<GameState, string> StateMessages { get; } = new Dictionary<GameState, string>()
        {
               {GameState.Uninitialized, "" },
               {GameState.WaitingForNewGame, "New Game" },
               {GameState.WaitingForPlayers, "Pick Board" },  // you stay in this state until you hit the button.  while in this state, the button stays this...
               {GameState.PickingBoard, "Roll for Order" },
               {GameState.WaitingForRollForOrder, "Start" },
               {GameState.WaitingForStart, "Pick Resource" },
               {GameState.AllocateResourceForward, "Pick Resource" },
               {GameState.AllocateResourceReverse, "Pick Resource" },
               {GameState.WaitingForRoll, "Select Roll" },
               {GameState.WaitingForNext, "Done" },
               {GameState.Supplemental, "Suplemental" }
        };
        [JsonIgnore]
        public CatanProxy Proxy { get; } = new CatanProxy() { HostName = "http://192.168.1.128:5000" };
        [JsonIgnore]
        public GameInfo GameInfo { get; set; }


        bool _AutoJoinGames = false;
        public bool AutoJoinGames
        {
            get
            {
                return _AutoJoinGames;
            }
            set
            {
                if (_AutoJoinGames != value)
                {
                    _AutoJoinGames = value;
                    NotifyPropertyChanged();
                }
            }
        }


        string _HostName = "http://192.168.1.128:5000";
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
        public string DefaultUser { get; set; } = "";
        public string TheHuman { get; set; } = "";
        public List<PlayerModel> AllPlayers { get; set; } = new List<PlayerModel>();
        public Settings Settings { get; set; } = new Settings();


        [JsonIgnore]
        public string StateMessage
        {
            get
            {
                if (Log == null) return "New Game";

                if (StateMessages.TryGetValue(Log.GameState, out string msg))
                {
                    return msg;
                }

                return $"GameStart.{Log.GameState}";
            }
        }



        
        bool _EnableUiInteraction = true;

        public void SetPipCount(int[] value)
        {
            FiveStarPositions = value[0];
            FourStarPositions = value[1];
            ThreeStarPositions = value[2];
            TwoStarPositions = value[3];

        }



        private bool _isServiceGame = false;

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
        public MainPageModel()
        {
            Log = new NewLog();
        }
        /// <summary>
        ///     We listent to changes from the Log.  We have "Dynamic Properties" which is where we apply logic to make decisions about what to show in the UI
        ///     if we add one of these properties (which are usually the get'ers only)
        /// </summary>

        private string[] DynamicProperties { get; } = new string[] { "EnableNextButton", "EnableRedo", "StateMessage", "ShowBoardMeasurements", "ShowRolls" };
        private void Log_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            DynamicProperties.ForEach((name) => NotifyPropertyChanged(name));

        }

        bool _WebSocketConnected = false;
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
        [JsonIgnore]
        public Visibility ShowRolls
        {
            get
            {
                if (Log == null) return Visibility.Visible;
                if (Log.GameState == GameState.WaitingForNewGame) return Visibility.Visible;

                if ((Log.GameState == GameState.WaitingForRoll) || (Log.GameState == GameState.WaitingForRollForOrder) || (Log.GameState == GameState.WaitingForStart)) return Visibility.Visible;
                return Visibility.Collapsed;
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
                return Visibility.Collapsed;
            }
        }

        private ObservableCollection<PlayerModel> _PlayingPlayers = new ObservableCollection<PlayerModel>();
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
        public PlayerModel GameStartedBy { get; internal set; }

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
                    NotifyPropertyChanged("EnableNextButton");
                }
            }
        }

        [JsonIgnore]
        public bool EnableNextButton
        {
            get
            {
                if (Log == null) return true;
                GameState state = Log.GameState;



                if (state == GameState.PickingBoard || state == GameState.WaitingForRollForOrder || state == GameState.WaitingForPlayers)
                {
                    return (MainPage.Current.TheHuman == this.GameStartedBy);
                }

                if (state == GameState.WaitingForNext || state == GameState.WaitingForRoll)
                {
                    return (MainPage.Current.TheHuman == MainPage.Current.CurrentPlayer); // only the person whose turn it is can hit "Next"
                }

                return (EnableUiInteraction && (state == GameState.WaitingForNewGame || state == GameState.WaitingForNext || state == GameState.WaitingForStart || state == GameState.PickingBoard ||
                        state == GameState.DoneSupplemental || state == GameState.Supplemental || state == GameState.AllocateResourceForward || state == GameState.AllocateResourceReverse ||
                        state == GameState.DoneResourceAllocation || state == GameState.WaitingForPlayers));

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
        public bool EnableRedo
        {
            get
            {
                if (Log == null) return false;

                return Log.CanRedo;
            }
        }

        NewLog _newLog = null;
        [JsonIgnore]
        public NewLog Log
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
        int _FiveStarPositions = 0;
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
        int _FourStarPosition = 0;
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
        int _ThreeStarPosition = 0;
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

        int _TwoStarPosition = 0;
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

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}