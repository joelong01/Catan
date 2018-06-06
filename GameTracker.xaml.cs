using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class GameTracker : UserControl, INotifyPropertyChanged
    {

        Stack<GameState> _stateStack = new Stack<GameState>();
        public IPageCallback PageCallBack { get; set; } = null;


        //
        //  stopwatch support
        DispatcherTimer _timer = new DispatcherTimer();
        DateTimeOffset _startTime;
        SolidColorBrush _green = new SolidColorBrush(Colors.Green);
        SolidColorBrush _red = new SolidColorBrush(Colors.Red);
        SolidColorBrush _yellow = new SolidColorBrush(Colors.Yellow);



        //
        //  keeps track of all player rolls
        int _currentPlayerIndex = 0;


        List<int> _rolls = new List<int>(); // a useful cache of all the rolls the players have made
        ObservableCollection<CatanPlayer> _players = new ObservableCollection<CatanPlayer>();
        public List<PlayerView> PlayerViews { get; set; } // the new style of player data
        //
        //  events
        //public event EnterEventHandler OnEnter;
        public event PlayerSelectedHandler OnPlayerSelected;
        public event PropertyChangedEventHandler PropertyChanged;



        LayoutDirection _layoutDirection = LayoutDirection.ClockWise;
        private Visibility _playerIndexVisibility = Visibility.Collapsed;
        GameState _state = GameState.Uninitialized;

        public GameState State
        {
            get
            {
                return _state;
            }
        }


       



        /*
                   [Chris]            
                   CardsLostToSeven=3
                   TimesTargetd=1
                   CardsLost=1
                   Rolls=12,5,6
                   TotalTime=00:00:12.6600888
      */

        internal string SaveGameState()
        {

            string s = "";
            foreach (CatanPlayer p in _players)
            {
                s += String.Format($"[{p.PlayerName}]{StaticHelpers.lineSeperator}");
                s += p.GameSerialize();
                s += StaticHelpers.lineSeperator;
            }

            return s;
        }

        public string Serialize()
        {
            string s = $"Players={StaticHelpers.SerializeListWithProperty<CatanPlayer>(_players, "PlayerName")}";
            foreach (CatanPlayer p in _players)
            {
                s += String.Format($"[{p.PlayerName}]{StaticHelpers.lineSeperator}");
                s += p.GameSerialize();
                s += StaticHelpers.lineSeperator;
            }


            return s;
        }

        public GameTracker()
        {
            this.InitializeComponent();
            
        }


      

        public void Init()
        {
            if (!ViewOnlyMode)
            {
                Reset();
               
                StartTimer();
            }
        }




        //
        //  move to player and show message.  wait for Enter and return what was typed in
        public async Task<string> ShowAndWait(string message, string value, bool enableInput = true)
        {
            

                _button.Content = message;
                _txtInput.Text = value;
                _txtInput.SelectAll();
                bool enabled = _txtInput.IsEnabled;
                _button.IsEnabled = true;
                _txtInput.IsEnabled = enableInput;
                _txtInput.Focus(FocusState.Programmatic);
                _button.Click -= OnButtonClicked;
                await _button.WhenClicked();
                _button.Click += OnButtonClicked;
                _txtInput.IsEnabled = enabled;
                return _txtInput.Text;
            
          
        }

     

        public string SerializedPlayerNames
        {
            get
            {
                return StaticHelpers.SerializeListWithProperty<CatanPlayer>(_players, "PlayerName");
            }
        }


     




    
        internal void DetachPlayersFromGrid()
        {
            foreach (var p in _players)
            {
                RemovePlayerFromGrid(p);
                p.IndexVisibility = Visibility.Collapsed;
            }
        }

        public int PlayerCount { get
            {
                if (CatanPlayerStyle)
                    return _players.Count;
                else
                    return PlayerViews.Count;
            }
        }

        public TimeSpan TimeSpan
        {
            get
            {
                DateTimeOffset time = DateTimeOffset.Now;
                return time - _startTime;
            }
        }



        private void Timer_Tick(object sender, object e)
        {
            DateTimeOffset time = DateTimeOffset.Now;
            TimeSpan span = time - _startTime;
            _tbTime.Text = span.ToString(@"mm\:ss");

            if (span.TotalSeconds < 60)
                _tbTime.Foreground = _green;
            else if (span.TotalSeconds < 120)
                _tbTime.Foreground = _yellow;
            else if (span.TotalSeconds > 180)
                _tbTime.Foreground = _red;


        }

        /// <summary>
        ///     We keep a stack of rolls per player and per game
        /// </summary>

        public void StartTimer()
        {
            _startTime = DateTimeOffset.Now;
            _timer.Start();
        }

        internal CatanPlayer RemovePlayer(int index)
        {
            CatanPlayer p = _players[index];
            _players.Remove(p);
            RemovePlayerFromGrid(p);
            UpdatePlayerLayout();
            return p;
        }

        public void StopTimer()
        {
            _timer.Stop();

        }

        public bool ShowStopWatch
        {
            get
            {
                return _tbTime.Visibility == Visibility.Visible;
            }
            set
            {
                _tbTime.Visibility = value ? Visibility.Visible : Visibility.Collapsed;

                if (value)
                {
                    _timer.Tick += Timer_Tick;
                    _timer.Interval = TimeSpan.FromMilliseconds(100);
                }
                else
                {
                    _timer.Stop();
                    _timer.Tick -= Timer_Tick;
                }
            }
        }

        public bool Remove(CatanPlayer player)
        {
            if (LayoutRoot.Children.Contains(player))
            {
                RemovePlayerFromGrid(player);
                _players.Remove(player);
                UpdatePlayerLayout();
                return true;
            }

            return false;

        }


        public string InputText
        {
            get
            {
                return _txtInput.Text;
            }
            set
            {
                _txtInput.Text = value;
                _txtInput.SelectAll();
                _txtInput.Focus(FocusState.Programmatic);
            }

        }

        public bool EnableInput
        {
            get
            {
                return _txtInput.IsEnabled;
            }
            set
            {
                _txtInput.IsEnabled = value;
                _button.IsEnabled = value;
            }
        }



        public void Reset()
        {
            foreach (CatanPlayer p in _players)
            {
                LayoutRoot.Children.Remove(p);

            }
            if (PlayerViews != null)
            {
                foreach (var playerview in PlayerViews)
                {
                    playerview.Close();

                    playerview.Visibility = Visibility.Collapsed;

                }

                PlayerViews.Clear();
            }



            _daRotateCurrentPlayer.Duration = TimeSpan.FromMilliseconds(250);
            _daRotateCurrentPlayer.To = 0;
            _sbRotateCurrentPlayer.Begin();
            _rolls.Clear();
            _currentPlayerIndex = 0;
            _players.Clear();
          
            _stateStack.Clear();
            SetState(GameState.Uninitialized);

        }



        public async Task AnimateOnePlayerClockwise()
        {
            await AnimatePlayers(1);

        }


        //  this has proved to be confusing.
        //  the problem is that we need to convert from the absolute number
        //  to a reletive number to rotate...sigh.
        public async Task AnimateToPlayerNumber(int playerNumber)
        {
            if (playerNumber >= _players.Count)
                return;

            int i = 0;
            int idx = _currentPlayerIndex;
            while (idx != playerNumber)
            {
                idx = CircleAdd(idx, 1, _players.Count);
                i++;
            }
            await AnimatePlayers(i);
        }

        public Task AnimateToPlayer(CatanPlayer player)
        {
            return AnimateToPlayerNumber(Convert.ToInt32(player.Index) - 1);
        }


        int CircleAdd(int start, int increment, int max)
        {
            start += increment;
            if (start >= max)
            {
                start = start - max;
            }
            else if (start < 0)
            {
                start += max;
            }

            return start;
        }

        public bool CatanPlayerStyle { get; set; } = false;
     
        public async Task AnimatePlayers(int numberofPositions)
        {
            int oldIndex = _currentPlayerIndex;
            int size = 0;
            if (CatanPlayerStyle)
            {
                CurrentPlayer.StopTimer();
                size = _players.Count;
            }
            else
            {
                size = PlayerViews.Count;
            }





            _currentPlayerIndex += numberofPositions;
            if (_currentPlayerIndex >= size)
            {
                _currentPlayerIndex = _currentPlayerIndex - size;
            }
            if (_currentPlayerIndex < 0)
            {
                _currentPlayerIndex += size;
            }

            if (CatanPlayerStyle)
            {

                double deltaAngle = 360 / _players.Count;
                deltaAngle *= numberofPositions;

                double duration = MainPage.GetAnimationSpeed(AnimationSpeed.Normal) * Math.Abs(numberofPositions);
                _daRotateCurrentPlayer.Duration = TimeSpan.FromMilliseconds(duration);
                _daRotateCurrentPlayer.To += deltaAngle;

                await _sbRotateCurrentPlayer.ToTask();
                NotifyPropertyChanged("CurrentPlayer");
            }
           

          

            if (!CatanPlayerStyle)
            {
                //
                //   new UI
                foreach (var player in PlayerViews)
                {
                    player.Close();
                }


                if (_currentPlayerIndex >= 0 && _currentPlayerIndex < PlayerViews.Count)
                {
                    PlayerViews[_currentPlayerIndex].Open();
                }
            }

            PageCallBack.CurrentPlayerChanged();

        }

        public async Task AnimateOnePlayerCounterClockwise()
        {
            await AnimatePlayers(-1);
        }



        private void Control_GotFocus(object sender, RoutedEventArgs e)
        {

        }

        internal void SelectAll()
        {
            _txtInput.SelectAll();
        }


        public static double[] RollPercents(List<int> stack, int[] counts)
        {


            double[] percents = new double[11];
            Array.Clear(percents, 0, 11);
            if (stack.Count != 0)
            {
                int total = 0;
                foreach (int n in counts)
                {
                    total += n;
                }

                for (int i = 0; i < 11; i++)
                {
                    percents[i] = (double)counts[i] / (double)stack.Count;
                }

            }

            return percents;
        }

        public List<int> CurrentPlayerRolls
        {
            get

            {
                return PlayerViews[_currentPlayerIndex].PlayerGameData.Rolls;
            }
        }

        public List<int> AllRolls
        {
            get
            {
                return _rolls;
            }
            set
            {
                _rolls = value;
            }
        }

        public static int[] RollCount(List<int> stack)
        {
            int[] counts = new int[11];
            Array.Clear(counts, 0, 11);
            if (stack.Count != 0)
            {
                foreach (int n in stack)
                {
                    counts[n - 2]++;
                }
            }
            return counts;
        }


   

        public bool ViewOnlyMode
        {
            get
            {
                return _button.Visibility == Visibility.Collapsed;
            }
            set
            {
                Visibility vis = (value) ? Visibility.Collapsed : Visibility;
                _button.IsEnabled = value;
                _txtInput.IsEnabled = value;
                _button.Visibility = vis;
                _txtInput.Visibility = vis;
                _txtName.Visibility = vis;
                


            }

        }

        public ObservableCollection<CatanPlayer> Players
        {
            get
            {
                return _players;
            }
            set
            {

                Reset();

                foreach (var player in value)
                {
                    this.AddPlayer(player);
                }

                NotifyPropertyChanged();
                if (_players != null && !ViewOnlyMode)
                {
                    if (_players.Count > 0)
                    {
                        CurrentPlayer = _players[0];
                        NotifyPropertyChanged("CurrentPlayer");
                        UpdatePlayerLayout();
                      



                    }

               
                }

            }
        }




        public Visibility PlayerIndexVisibility
        {
            get
            {
                return _playerIndexVisibility;
            }

            set
            {
                _playerIndexVisibility = value;
                foreach (var p in _players)
                {
                    p.IndexVisibility = _playerIndexVisibility;
                }
            }
        }

        public LayoutDirection LayoutDirection
        {
            get
            {
                return _layoutDirection;
            }

            set
            {
                _layoutDirection = value;
            }
        }

        public string CurrentPlayerName
        {
            get
            {
                try
                {
                    //return _players[_currentPlayerIndex].PlayerName;
                    return ActivePlayerView.PlayerName;
                }
                catch
                {
                    return "";
                }
            }
        }

        public PlayerView ActivePlayerView
        {
            get
            {
                if (PlayerViews != null)
                {
                    if (PlayerViews.Count > 0)
                    {
                        return PlayerViews[_currentPlayerIndex];
                    }
                }

                return null;
            }
        }

        public CatanPlayer CurrentPlayer
        {
            get
            {
                if (_players != null)
                {
                    if (_players.Count > 0)
                    {
                        return _players[_currentPlayerIndex];
                    }
                }

                return null;
            }
            set
            {

            }
        }

        public int CurrentPlayerIndex
        {
            get
            {
                return _currentPlayerIndex;
            }
        }

        

        public async Task SetCurrentPlayer(string playerName)
        {
            _currentPlayerIndex = 0;
            foreach (CatanPlayer p in _players)
            {
                if (p.PlayerName.CompareTo(playerName) != 0)
                {
                    await AnimateOnePlayerClockwise();
                }
                else
                {
                    break;
                }
            }


            NotifyPropertyChanged("CurrentPlayer");
        }

        internal void AddPlayer(CatanPlayer player)
        {

            _players.Add(player);
            player.Index = (_players.Count).ToString(); // one index so that -n works (e.g. -0??)
            player.IndexVisibility = _playerIndexVisibility;
            AddPlayerToGrid(player);
            UpdatePlayerLayout();
            if (!ViewOnlyMode)
            {
              
                player.VisibleTimer = true;
            }


        }



        private void AddPlayerToGrid(CatanPlayer player)
        {
            LayoutRoot.Children.Add(player);
            player.OnPlayerSelected += Player_OnPlayerSelected; ;
        }


        private void Player_OnPlayerSelected(object sender, EventArgs e)
        {
            OnPlayerSelected?.Invoke(sender, e);
        }

        private void RemovePlayerFromGrid(CatanPlayer player)
        {
            LayoutRoot.Children.Remove(player);
            player.OnPlayerSelected -= Player_OnPlayerSelected;
         

        }




        private void UpdatePlayerLayout()
        {

            if (_players.Count == 0) return;
            double deltaAngle = 360 / _players.Count;
            double angle = 90;

            if (LayoutDirection == LayoutDirection.CounterClockwise)
                deltaAngle = -deltaAngle;

            double duration = MainPage.GetAnimationSpeed(AnimationSpeed.Normal);
            double startAfter = duration;
            for (int i = 0; i < _players.Count; i++)
            {
                CatanPlayer p = _players[i];
                p.Index = (i + 1).ToString();
                p.RotateToAsync(angle, duration, startAfter);
                angle += deltaAngle;
            }
        }


        /// <summary>
        ///  called by MainPage to tell me that a user has rolled an acceptable roll
        /// </summary>
        /// <param name="roll"></param>
        public bool PushRoll(int roll)
        {

            if (roll < 2 || roll > 12)
                return false;

            _rolls.Push(roll);
            CurrentPlayerRolls.Push(roll);
            //   this.TraceMessage($"\n{CurrentPlayer.PlayerName} rolled {roll}\n");
            return true;
        }



        public int PopRoll()
        {
            if (_rolls.Count == 0)
                return -1;

            int n = _rolls.Pop();
            int m = CurrentPlayerRolls.Pop();
            this.TraceMessage($"Popping role {m}/{n} for {ActivePlayerView.PlayerName}");
            this.Assert(n == m, $"AllRolls has {n} and player rolls has {m} for {_players[_currentPlayerIndex].PlayerName}");


            return n;
        }


        private void OnButtonClicked(object sender, RoutedEventArgs e)
        {
            
        }

        
        private void StateHelper(string buttonContent, bool buttonEnable, bool textEnable, double fontSize)
        {
            _button.Content = buttonContent;
            _button.IsEnabled = buttonEnable;
            _txtInput.IsEnabled = textEnable;
            _txtInput.FontSize = fontSize;

            StartTimer();

            if (textEnable)
                _txtInput.Focus(FocusState.Programmatic);
            else
                _button.Focus(FocusState.Programmatic);
            //
            //  new UI
            
            PageCallBack.UpdateUiForState(_state);
        }
        
            //Uninitialized, WaitingForNewGame, Dealing, WaitingForStart, AllocateResourceForward, AllocateResourceReverse, DoneResourceAllocation, WaitingForRoll, Targeted,
            //LostToCardsLikeMonopoly, Supplemental, DoneSupplemental, WaitingForNext, LostCardsToSeven, MissedOpportunity
        string[] _StateMessages = new string[] {"Uninitialized", "New Game", "Wait...", "Click to Start", "Next When Done", "Next When Done", "Next to Start", "Enter Roll", "Targetted",
                                                "Cards Lost", "Supplemental? Next.", "", "Next When Done", "Cards Lost"};
        public void SetState(GameState state)
        {
            if (_state != state)
            {
                _state = state;
                _stateStack.Push(_state);
            }
          
            double fontSize = 28;
            
            switch (state)
            {
                case GameState.WaitingForNewGame:
                    StateHelper(_StateMessages[(int)state], true, false, fontSize);
                    _txtInput.Text = "";
                    break;
                case GameState.Dealing:
                    StateHelper(_StateMessages[(int)state], false, false, fontSize);
                    break;
                case GameState.WaitingForStart:
                    StateHelper(_StateMessages[(int)state], true, false, fontSize);
                    break;
                case GameState.AllocateResourceReverse:
                case GameState.AllocateResourceForward:
                   
                    StateHelper(_StateMessages[(int)state], true, false, fontSize);
                    break;

                case GameState.WaitingForRoll:
                 
                    StateHelper(_StateMessages[(int)state], true, true, fontSize);
                    break;
                case GameState.Supplemental:
                    StateHelper(_StateMessages[(int)state], true, false, fontSize);
                    break;
                case GameState.WaitingForNext:
                    StateHelper(_StateMessages[(int)state], true, false, fontSize);
                    
                    break;
                case GameState.Targeted:
                case GameState.LostToCardsLikeMonopoly:
                case GameState.LostCardsToSeven:
                case GameState.MissedOpportunity:
                    StateHelper(_StateMessages[(int)state], true, true, fontSize);
                    InputText = "0";
                    break;
                default:
                    break;
            }
        }

        public async Task PlayerWon()
        {
            await Task.Delay(0);
            throw new NotImplementedException();
#if false
            foreach (CatanPlayer p in _players)
            {
                int gp = Int32.Parse(p.GamesPlayed);
                gp++;
                p.GamesPlayed = gp.ToString();

            }

            int gw = Int32.Parse(_players[_currentPlayerIndex].GamesWon);
            gw++;
            _players[_currentPlayerIndex].GamesWon = gw.ToString();


            //
            //  read in all the players because some might not have played
            ObservableCollection<CatanPlayer> allPlayers = await MainPage.LoadPlayers(MainPage.PlayerDataFile);

            IEnumerable<CatanPlayer> unionPlayers = MergeInto<CatanPlayer>(_players, allPlayers, new CatanPlayerComparer());

            await MainPage.SavePlayers(unionPlayers, MainPage.PlayerDataFile);
#endif

        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        /// <summary>
        ///     This looks through list 2 and if it finds anything in list 1 that looks like it is in list 2, replace the 
        ///     object in list 2 with the one from list 1
        /// </summary>

        public static IEnumerable<T> MergeInto<T>(IEnumerable<T> list1, IEnumerable<T> list2, IEqualityComparer<T> comparer)
        {
            T[] tArray = list2.ToArray();
            foreach (T t1 in list1)
            {
                for (int i = 0; i < tArray.Count(); i++)
                {
                    T t2 = tArray[i];
                    if (comparer.Equals(t1, t2))
                    {
                        tArray[i] = t1;
                        break;
                    }
                }
            }

            return tArray;
        }

        private void MainButton_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            
        }


    }


   

    class CatanPlayerComparer : IEqualityComparer<CatanPlayer>
    {
        public CatanPlayerComparer()
        {

        }

        public bool Equals(CatanPlayer p1, CatanPlayer p2)
        {
            if (String.Compare(p1.PlayerName, p2.PlayerName) == 0)
                return true;

            return false;
        }

        public int GetHashCode(CatanPlayer p1)
        {

            return p1.PlayerName == null ? 0 : p1.PlayerName.GetHashCode();

        }
    }
}
