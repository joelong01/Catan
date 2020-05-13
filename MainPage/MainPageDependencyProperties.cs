using Catan.Proxy;
using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{

    public sealed partial class MainPage : Page
    {
       
        public static readonly DependencyProperty PipCountProperty = DependencyProperty.Register("PipCount", typeof(TradeResources), typeof(MainPage), new PropertyMetadata(new TradeResources()));
        public TradeResources PipCount
        {
            get => (TradeResources)GetValue(PipCountProperty);
            set => SetValue(PipCountProperty, value);
        }

        public static readonly DependencyProperty EnableRedoProperty = DependencyProperty.Register("EnableRedo", typeof(bool), typeof(MainPage), new PropertyMetadata(false));
        public bool EnableRedo
        {
            get => (bool)GetValue(EnableRedoProperty);
            set => SetValue(EnableRedoProperty, value);
        }

        public static readonly DependencyProperty CanMoveBaronBeforeRollProperty = DependencyProperty.Register("CanMoveBaronBeforeRoll", typeof(bool), typeof(MainPage), new PropertyMetadata(false));
        public bool CanMoveBaronBeforeRoll
        {
            get => (bool)GetValue(CanMoveBaronBeforeRollProperty);
            set => SetValue(CanMoveBaronBeforeRollProperty, value);
        }

        
        public static readonly DependencyProperty GameStateProperty = DependencyProperty.Register("GameState", typeof(GameState), typeof(MainPage), new PropertyMetadata(GameState.WaitingForNewGame));
        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(MainPage), new PropertyMetadata(new PlayerModel() { PlayerName = "Unset" }, CurrentPlayerChanged));
        public PlayerModel CurrentPlayer
        {
            get => (PlayerModel)GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }
        private static void CurrentPlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MainPage depPropClass = d as MainPage;
            PlayerModel depPropValue = (PlayerModel)e.NewValue;
            depPropClass?.SetCurrentPlayer(depPropValue);
        }
        private void SetCurrentPlayer(PlayerModel player)
        {
            //
            //  the next player can always play a baron once
            player.GameData.PlayedKnightThisTurn = false;
            player.GameData.MovedBaronAfterRollingSeven = null;

            UpdateTurnFlag();

            _stopWatchForTurn.TotalTime = TimeSpan.FromSeconds(0);
            _stopWatchForTurn.StartTimer();

            if (GameStateFromOldLog == GameState.AllocateResourceForward || GameStateFromOldLog == GameState.AllocateResourceReverse)
            {

                _ = HideAllPipEllipses();
                _showPipGroupIndex = 0;

            }

            // tell all the Buildings that the CurrentPlayer has changed
            foreach (BuildingCtrl building in _gameView.AllBuildings)
            {
                building.CurrentPlayer = player;
            }
        }

        public static readonly DependencyProperty RandomGoldProperty = DependencyProperty.Register("RandomGold", typeof(bool), typeof(MainPage), new PropertyMetadata(true, RandomGoldChanged));
        public bool RandomGold
        {
            get => (bool)GetValue(RandomGoldProperty);
            set => SetValue(RandomGoldProperty, value);
        }
        private static void RandomGoldChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as MainPage;
            depPropClass?.SetRandomGold();
        }
        private void SetRandomGold()
        {
            //
            //  TODO:  what should we do if the option is set while the game is running?

            if (GameStateFromOldLog != GameState.WaitingForStart)
                return;

            // _ = SetRandomTileToGold();

        }

        public static readonly DependencyProperty RandomGoldTileCountProperty = DependencyProperty.Register("RandomGoldTileCount", typeof(int), typeof(MainPage), new PropertyMetadata(1, RandomGoldTileCountChanged));
        public int RandomGoldTileCount
        {
            get => (int)GetValue(RandomGoldTileCountProperty);
            set => SetValue(RandomGoldTileCountProperty, value);
        }
        private static void RandomGoldTileCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as MainPage;
            var depPropValue = (int)e.NewValue;
            depPropClass?.SetRandomGoldTileCount(depPropValue);
        }
        private void SetRandomGoldTileCount(int value)
        {

        }



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
            get => (string)GetValue(TwelvePercentProperty);
            set => SetValue(TwelvePercentProperty, value);
        }
        public string ElevenPercent
        {
            get => (string)GetValue(ElevenPercentProperty);
            set => SetValue(ElevenPercentProperty, value);
        }
        public string TenPercent
        {
            get => (string)GetValue(TenPercentProperty);
            set => SetValue(TenPercentProperty, value);
        }

        public string NinePercent
        {
            get => (string)GetValue(NinePercentProperty);
            set => SetValue(NinePercentProperty, value);
        }
        public string EightPercent
        {
            get => (string)GetValue(EightPercentProperty);
            set => SetValue(EightPercentProperty, value);
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
        public string ThreePercent
        {
            get => (string)GetValue(ThreePercentProperty);
            set => SetValue(ThreePercentProperty, value);
        }
        public string TwoPercent
        {
            get => (string)GetValue(TwoPercentProperty);
            set => SetValue(TwoPercentProperty, value);
        }
        public int TotalRolls
        {
            get => (int)GetValue(TotalRollsProperty);
            set => SetValue(TotalRollsProperty, value);
        }
        #endregion

    }
}
