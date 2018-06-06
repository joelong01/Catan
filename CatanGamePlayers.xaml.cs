using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{

 

    public sealed partial class CatanGamePlayers : UserControl, INotifyPropertyChanged
    {

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


        Stack<int> _rolls = new Stack<int>(); // a useful cache of all the rolls the players have made
        ObservableCollection<CatanPlayer> _players = new ObservableCollection<CatanPlayer>();

        //
        //  events
        public event EnterEventHandler OnEnter;
        public event UndoEventHandler OnUndo;
        public event UndoBeforePrevious UndoBeforePrevious;
        public event PlayerSelectedHandler OnPlayerSelected;
        public event PropertyChangedEventHandler PropertyChanged;

        
        LayoutDirection _layoutDirection = LayoutDirection.ClockWise;
        private Visibility _playerIndexVisibility = Visibility.Collapsed;
        GameState _state = GameState.WaitingForNewGame;



        /// <summary>
        ///     This should be in the form
        ///     Joe=3,5,7,8     e.g. Name=CSV Rolls
        /// </summary>
        /// <returns></returns>

        internal string SaveGameState()
        {
            string nl = "\n";
            string sep = ",";
            string s = "";
            foreach (CatanPlayer p in _players)
            {
                s += String.Format($"{p.PlayerName}=");
                foreach (int i in p.Rolls)
                {
                    s += String.Format($"{i}{sep}");
                }
                s += nl;
            }

            return s;
        }

        public CatanGamePlayers()
        {
            this.InitializeComponent();
            if (!IsInDesignMode)
                Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;

            
            StartTimer();
        }

        private bool IsInDesignMode
        {
            get
            {
                return !(Application.Current is App);
            }
        }

        private async void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            args.Handled = false;

            if (args.VirtualKey != VirtualKey.Enter)
                return;

            if (args.EventType != CoreAcceleratorKeyEventType.KeyUp)
                return;
            
            // Ensures the ENTER key always runs the same code as your default button.
            if (_button.IsEnabled)
            {
                await MainButton_Enter();
                args.Handled = true;
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

        public int PlayerCount { get { return _players.Count; } }

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

        public void DetachAcclerator()
        {
            Dispatcher.AcceleratorKeyActivated -= Dispatcher_AcceleratorKeyActivated;
        }
        public void AttachAccelerator()
        {
            Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
        }

        /// <summary>
        ///     We keep a stack of rolls per player and per game
        /// </summary>

        private void StartTimer()
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

        //public string CommandText
        //{
        //    get
        //    {
        //        return (string)_button.Content;
        //    }
        //    set
        //    {

        //        _button.Content = value;
        //    }
        //}
        public string InputText
        {
            get
            {
                return _txtInput.Text;
            }
            //set
            //{

            //    _txtInput.Text = value;
            //}
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
            }
        }

      

        public void Reset()
        {
            foreach (CatanPlayer p in _players)
            {
                LayoutRoot.Children.Remove(p);

            }
            _rolls.Clear();
            _currentPlayerIndex = 0;
            _players.Clear();
        }

        public async Task AnimatePlayersClockwise()
        {
            await AnimatePlayers(1);

        }

        public  async  Task AnimatePlayers(int numberofPositions)
        {
            _currentPlayerIndex += numberofPositions;
            if (_currentPlayerIndex >= _players.Count)
            {
                _currentPlayerIndex = _currentPlayerIndex - _players.Count;
            }
            if (_currentPlayerIndex < 0)
            {
                _currentPlayerIndex += _players.Count;
            }

          //  Debug.WriteLine($"PlayerIndex: {_currentPlayerIndex} Rotate#:{numberofPositions}");


            double deltaAngle = 360 / _players.Count;
            deltaAngle *= numberofPositions;
            double angle = 0;
            double startAfter = 0;
            double duration = MainPage.GetAnimationSpeed(AnimationSpeed.Normal) * Math.Abs(numberofPositions);
            List<Task> taskList = new List<Task>();
            foreach (CatanPlayer p in _players)
            {
                angle = p.CurrentAngle + deltaAngle;

                taskList.AddRange(p.RotateToTask(angle, duration, startAfter));
            }

            await Task.WhenAll(taskList.ToArray());
            NotifyPropertyChanged("CurrentPlayer");

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


        public static double[] RollPercents(Stack<int> stack, int[] counts)
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

        public Stack<int> CurrentPlayerRolls
        {
            get

            {
                return _players[_currentPlayerIndex].Rolls;
            }
        }

        public Stack<int> AllRolls
        {
            get
            {
                return _rolls;
            }
        }

        public static int[] RollCount(Stack<int> stack)
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


        private async Task MainButton_Enter()
        {
            _button.IsEnabled = false;
            if (_players.Count > 0)
            {
                await OnEnter?.Invoke(this, new EnterEventArgs(_players[_currentPlayerIndex].PlayerName, _txtInput.Text));

            }
            else
            {
                await OnEnter?.Invoke(this, new EnterEventArgs(null, ""));
            }
            _button.IsEnabled = true;
            if (_txtInput.IsEnabled)
            {
                _txtInput.Focus(FocusState.Programmatic);
                _txtInput.Text = "";
                // _txtInput.SelectAll();
            }
        }

        public bool ViewOnlyMode
        {
            get
            {
                return _button.Visibility == Visibility.Visible;
            }
            set
            {
                Visibility vis = (value) ? Visibility.Collapsed : Visibility;
                _button.IsEnabled = value;
                _txtInput.IsEnabled = value;
                _button.Visibility = vis;
                _txtInput.Visibility = vis;
                _txtName.Visibility = vis;

                if (value && (!IsInDesignMode))
                {

                    DetachAcclerator();
                }


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

                UpdatePlayerLayout();
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
                    return _players[_currentPlayerIndex].PlayerName;
                }
                catch
                {
                    return "";
                }
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

        public async Task SetCurrentPlayer(string playerName)
        {
            _currentPlayerIndex = 0;
            foreach (CatanPlayer p in _players)
            {
                if (p.PlayerName.CompareTo(playerName) != 0)
                {
                    await AnimatePlayersClockwise();
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
            SetupPlayer(player);
            _players.Add(player);

            player.Index = (_players.Count).ToString(); // one index so that -n works (e.g. -0??)
            player.IndexVisibility = _playerIndexVisibility;           
            AddPlayerToGrid(player);
            UpdatePlayerLayout();

        }



        private void SetupPlayer(CatanPlayer player)
        {
            double controlWidth = 100.0;
            double firstColWidth = LayoutRoot.ColumnDefinitions[0].Width.Value;
            double CenterX = this.Width / 2.0 - controlWidth / 2.0 - firstColWidth; // where 50 is 1/2 the Width of the control 
            double TranslateY = CenterX;
           
            player.SetupTransform(CenterX, TranslateY, 0);

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

        internal async Task<CatanPlayer> AddPlayer(string playerName, string imageFileName, Visibility indexVisibility = Visibility.Collapsed)
        {
            double controlWidth = 100.0;
            double firstColWidth = LayoutRoot.ColumnDefinitions[0].Width.Value;
            double CenterX = this.ActualWidth / 2.0 - controlWidth / 2.0 - firstColWidth; // where 50 is 1/2 the Width of the control 
            double TranslateY = CenterX;
            CatanPlayer player = new CatanPlayer(CenterX, TranslateY, 0, playerName, imageFileName);
            player.IndexVisibility = indexVisibility;
            await player.LoadImage();
            _players.Add(player);

            player.Index = (_players.Count).ToString(); // one index so that -n works (e.g. -0??)

        
            AddPlayerToGrid(player);
            UpdatePlayerLayout();

            return player;

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

        public async Task UndoRoll()
        {
            if (_rolls.Count == 0)
                return;

            UndoEventArgs args = new UndoEventArgs();
            Debug.Assert(UndoBeforePrevious != null, "You forget to subscribe to the UndoBeforePrevious event");
            UndoBeforePrevious?.Invoke(this, args);


            if (args.UndoOrder == UndoOrder.None)
            {
                return;
            }

            if (args.UndoOrder == UndoOrder.PreviousThenUndo)
            {
                await AnimateOnePlayerCounterClockwise();
            }

            PopRoll();
            OnUndo?.Invoke(this, EventArgs.Empty);

            
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
            return true;
        }



        public int PopRoll()
        {
            if (_rolls.Count == 0)
                return -1;

            int n = _rolls.Pop();
            int m = CurrentPlayerRolls.Pop();

            if (n != m)
            {
                Debug.Assert(false, $"AllRolls has {n} and player rolls has {m} for {_players[_currentPlayerIndex].PlayerName}");
            }

            return n;
        }



        private async void OnButtonClicked(object sender, RoutedEventArgs e)
        {
            await MainButton_Enter();
        }



        private void Input_KeyPressed(object sender, KeyRoutedEventArgs e)
        {

            e.Handled = false;

            //
            //  filter out everythign not on Keypad (but other keyboard works too)
            if (!StaticHelpers.IsOnKeyPad(e.Key))
            {
                e.Handled = true;
                return;
            }
            char[] chars = _txtInput.Text.ToCharArray();

            if (chars.Length == 0) // first char
            {
                if (e.Key == VirtualKey.Number0 || e.Key == VirtualKey.NumberPad0)
                {
                    e.Handled = true; // can't start with a zero
                    return;
                }

                //
                //  there is nothing in the text box and this is a key that is on the keypad. 
                //  let them all in

                e.Handled = false;
                return;
            }

            if (chars.Length == 1)
            {
                //
                //  there is a character there, but it is highlited, so we will replace it
                if (_txtInput.SelectionLength == 1)
                {
                    e.Handled = false;
                    return;

                }

                if (chars[0] != '1')
                {
                    e.Handled = true; // can't be > 12
                    return;
                }

                if (e.Key == VirtualKey.Number0 || e.Key == VirtualKey.Number1 || e.Key == VirtualKey.Number2 ||
                    e.Key == VirtualKey.NumberPad0 || e.Key == VirtualKey.NumberPad1 || e.Key == VirtualKey.NumberPad2)
                {
                    // allow 10, 11, 12
                    e.Handled = false;
                    return;
                }



                e.Handled = true;
            }
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
        }

        public void SetState(GameState state)
        {
            _state = state;

            double fontSize = 28;

            switch (state)
            {
                case GameState.WaitingForNewGame:
                    StateHelper("New Game", true, false, fontSize);
                    _txtInput.Text = "";
                    break;
                case GameState.WaitingForStart:
                    StateHelper("Start", true, false, fontSize);
                    break;
                case GameState.AllocateResource:
                    StateHelper("Done - Next!", true, false, fontSize);
                    break;
                case GameState.WaitingForRoll:
                    StateHelper("Enter Roll", true, true, fontSize);
                    break;
                case GameState.Supplemental:
                    StateHelper("Suppl -> Next", true, false, fontSize);
                    break;
                case GameState.WaitingForNext:
                    StateHelper("Done - Next!", true, false, fontSize);
                    break;
                case GameState.Iterating:
                    break;
                default:
                    break;
            }
        }

        public async Task PlayerWon()
        {

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

            IEnumerable<CatanPlayer> unionPlayers = allPlayers.Union(_players, new CatanPlayerComparer());

            await MainPage.SavePlayers(unionPlayers, MainPage.PlayerDataFile);

        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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
