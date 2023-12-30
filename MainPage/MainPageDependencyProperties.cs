using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class MainPage : Page
    {
        private static void CurrentPlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainPage depPropClass = d as MainPage;
            PlayerModel depPropValue = (PlayerModel)e.NewValue;
            depPropClass?.SetCurrentPlayer(depPropValue);
        }

        private static void RandomGoldChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as MainPage;
            depPropClass?.SetRandomGold();
        }

        private static void RandomGoldTileCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as MainPage;
            var depPropValue = (int)e.NewValue;
            depPropClass?.SetRandomGoldTileCount(depPropValue);
        }

        /// <summary>
        ///     A place to update the GameUI when it is a new player's turn.  when it is a local game, TheHuman tracks the CurrentPlayer
        ///     to make all the game logic work right.
        /// </summary>
        /// <param name="player"></param>
        public async void SetCurrentPlayer(PlayerModel player)
        {
            if (player == null) return;

            if (!IsServiceGame)
            {
                TheHuman = player;
            }

            UpdateTurnFlag();

            if (player == TheHuman)
            {
               await ResetRollControl();
            }

            _stopWatchForTurn.TotalTime = TimeSpan.FromSeconds(0);
            _stopWatchForTurn.StartTimer();

            if (CurrentGameState == GameState.AllocateResourceForward || CurrentGameState == GameState.AllocateResourceReverse)
            {
                _ = HideAllPipEllipses();
                _showPipGroupIndex = 0;
            }
        }

        private void SetRandomGold()
        {
            //
            //  TODO:  what should we do if the option is set while the game is running?

            if (CurrentGameState != GameState.BeginResourceAllocation)
                return;

            // _ = SetRandomTileToGold();
        }

        private void SetRandomGoldTileCount(int value)
        {
        }

        public bool MustMoveBaron
        {
            get => (bool)GetValue(MustMoveBaronProperty);
            set => SetValue(MustMoveBaronProperty, value);
        }

        public PlayerModel CurrentPlayer
        {
            get => (PlayerModel)GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }

        public string EightPercent
        {
            get => (string)GetValue(EightPercentProperty);
            set => SetValue(EightPercentProperty, value);
        }

        public string ElevenPercent
        {
            get => (string)GetValue(ElevenPercentProperty);
            set => SetValue(ElevenPercentProperty, value);
        }

        public string FivePercent
        {
            get => (string)GetValue(FivePercentProperty);
            set => SetValue(FivePercentProperty, value);
        }

        public string FourPercent
        {
            get => (string)GetValue(FourPercentProperty);
            set => SetValue(FourPercentProperty, value);
        }

        public string NinePercent
        {
            get => (string)GetValue(NinePercentProperty);
            set => SetValue(NinePercentProperty, value);
        }

        public TradeResources PipCount
        {
            get => (TradeResources)GetValue(PipCountProperty);
            set => SetValue(PipCountProperty, value);
        }

        public bool RandomGold
        {
            get => (bool)GetValue(RandomGoldProperty);
            set => SetValue(RandomGoldProperty, value);
        }

        public int RandomGoldTileCount
        {
            get => (int)GetValue(RandomGoldTileCountProperty);
            set => SetValue(RandomGoldTileCountProperty, value);
        }

        public string SevenPercent
        {
            get => (string)GetValue(SevenPercentProperty);
            set => SetValue(SevenPercentProperty, value);
        }

        public string SixPercent
        {
            get => (string)GetValue(SixPercentProperty);
            set => SetValue(SixPercentProperty, value);
        }

        public string TenPercent
        {
            get => (string)GetValue(TenPercentProperty);
            set => SetValue(TenPercentProperty, value);
        }

        public string ThreePercent
        {
            get => (string)GetValue(ThreePercentProperty);
            set => SetValue(ThreePercentProperty, value);
        }

        public int TotalRolls
        {
            get => (int)GetValue(TotalRollsProperty);
            set => SetValue(TotalRollsProperty, value);
        }

        public string TwelvePercent
        {
            get => (string)GetValue(TwelvePercentProperty);
            set => SetValue(TwelvePercentProperty, value);
        }

        public string TwoPercent
        {
            get => (string)GetValue(TwoPercentProperty);
            set => SetValue(TwoPercentProperty, value);
        }

        public static readonly DependencyProperty MustMoveBaronProperty = DependencyProperty.Register("MustMoveBaron", typeof(bool), typeof(MainPage), new PropertyMetadata(false));
        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(MainPage), new PropertyMetadata(new PlayerModel() { PlayerName = "Unset" }, CurrentPlayerChanged));
        public static readonly DependencyProperty EightPercentProperty = DependencyProperty.Register("EightPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty ElevenPercentProperty = DependencyProperty.Register("ElevenPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty FivePercentProperty = DependencyProperty.Register("FivePercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty FourPercentProperty = DependencyProperty.Register("FourPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty GameStateProperty = DependencyProperty.Register("GameState", typeof(GameState), typeof(MainPage), new PropertyMetadata(GameState.WaitingForNewGame));
        public static readonly DependencyProperty NinePercentProperty = DependencyProperty.Register("NinePercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty PipCountProperty = DependencyProperty.Register("PipCount", typeof(TradeResources), typeof(MainPage), new PropertyMetadata(new TradeResources()));
        public static readonly DependencyProperty RandomGoldProperty = DependencyProperty.Register("RandomGold", typeof(bool), typeof(MainPage), new PropertyMetadata(true, RandomGoldChanged));

        public static readonly DependencyProperty RandomGoldTileCountProperty = DependencyProperty.Register("RandomGoldTileCount", typeof(int), typeof(MainPage), new PropertyMetadata(1, RandomGoldTileCountChanged));
        public static readonly DependencyProperty SevenPercentProperty = DependencyProperty.Register("SevenPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty SixPercentProperty = DependencyProperty.Register("SixPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty TenPercentProperty = DependencyProperty.Register("TenPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty ThreePercentProperty = DependencyProperty.Register("ThreePercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty TotalRollsProperty = DependencyProperty.Register("TotalRolls", typeof(int), typeof(MainPage), new PropertyMetadata(0));
        public static readonly DependencyProperty TwelvePercentProperty = DependencyProperty.Register("TwelvePercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty TwoPercentProperty = DependencyProperty.Register("TwoPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
    }
}
