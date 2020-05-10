using Catan.Proxy;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Windows.UI.Xaml;

namespace Catan10
{
    public class ServiceData
    {
        public CatanProxy Proxy { get; } = new CatanProxy() { HostName = "http://192.168.1.128:5000" };
        public string HostName
        {
            get
            {
                return Proxy.HostName;
            }
            set
            {

                Proxy.HostName = value;

            }
        }

        public SessionInfo SessionInfo { get; set; }


        public ServiceData()
        {
            Proxy.HostName = HostName;
        }
    }

    public class MainPageModel : INotifyPropertyChanged
    {
        readonly string[] _StateMessages = new string[] {
            "Uninitialized",        // 0    Uninitialized,                      
            "New Game",             // 1    WaitingForNewGame,                  
            "Starting...",          // 2    Starting,                           
            "Dealing",              // 3    Dealing,                            
            "Start Game",           // 4    WaitingForStart,                    
            "Next When Done",       // 5    AllocResourceForward,            
            "Next When Done",       // 6    AllocateResourceReverse,            
            "",                     // 7    DoneResourceAllocation,             
            "Enter Roll",           // 8    WaitingForRoll,                     
            "Targeted",             // 9    Targeted,                           
            "Cards Lost",           // 10    LostToCardsLikeMonopoly,            
            "Supplemental?",        // 11    Supplemental,                       
            "Done",                 // 12    DoneSupplemental,                   
            "NextWhenDone",         // 13    WaitingForNext,                     
            "Cards Lost",           // 14    LostCardsToSeven,                   
            "Cards Lost",           // 15    MissedOpportunity,                  
            "Pick Game",            // 16    GamePicked,                         
            "Move Baron or Ship",   // 17    MustMoveBaron  
            "",                     // 18    Unknown
            "Roll For Order"        // 19     WaitingToRollForPosition


        };

        string _StateMessage = "Uninitialized";
        public string StateMessage
        {
            get
            {
                return _StateMessage;
            }
            set
            {
                if (_StateMessage != value)
                {
                    _StateMessage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ServiceData ServiceData { get; } = new ServiceData();
        bool _EnableUiInteraction = true;

        public void SetPipCount(int[] value)
        {
            FiveStarPositions = value[0];
            FourStarPositions = value[1];
            ThreeStarPositions = value[2];
            TwoStarPositions = value[3];

        }


       
        private bool _isServiceGame = false;

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
           
        }

        private void Log_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            // this.TraceMessage($"Log_PropertyChanged: [Property={e.PropertyName}] [GameState={_Log.GameState}");
            NotifyPropertyChanged("EnableNextButton");
            NotifyPropertyChanged("EnableRedo");

            StateMessage = _StateMessages[(int)_newLog.GameState];

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

        public PlayerModel GameStartedBy { get; internal set; }


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


        public bool EnableNextButton
        {
            get
            {
                if (Log == null) return true;
                GameState state = Log.GameState;
                return (EnableUiInteraction && (state == GameState.WaitingForNewGame || state == GameState.WaitingForNext || state == GameState.WaitingForStart ||
                        state == GameState.DoneSupplemental || state == GameState.Supplemental || state == GameState.AllocateResourceForward || state == GameState.AllocateResourceReverse ||
                        state == GameState.DoneResourceAllocation || state == GameState.WaitingForPlayers));

            }
        }

        public bool RollGridEnabled
        {
            get
            {
                if (Log == null) return true;
                GameState state = Log.GameState;

                return false;
            }
        }

        public Visibility RollGridVisible
        {
            get
            {
                if (Log == null) return Visibility.Visible;
                GameState state = Log.GameState;
                if (EnableUiInteraction && (state == GameState.WaitingForRoll || state == GameState.WaitingForPlayers)) return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }


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