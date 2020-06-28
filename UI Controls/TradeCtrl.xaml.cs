using System.Collections;
using System.Collections.ObjectModel;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class TradeCtrl : UserControl
    {
        #region Delegates + Fields + Events + Enums


        public static readonly DependencyProperty PlayingPlayersProperty = DependencyProperty.Register("PlayingPlayers", typeof(ObservableCollection<PlayerModel>), typeof(TradeCtrl), new PropertyMetadata(null, PlayingPlayersChanged));
        public static readonly DependencyProperty TheHumanProperty = DependencyProperty.Register("TheHuman", typeof(PlayerModel), typeof(TradeCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(TradeCtrl), new PropertyMetadata(null, CurrentPlayerChanged));
        public PlayerModel CurrentPlayer
        {
            get => (PlayerModel)GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }
        private static void CurrentPlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as TradeCtrl;
            var depPropValue = (PlayerModel)e.NewValue;
            depPropClass?.SetCurrentPlayer(depPropValue);
        }
        private void SetCurrentPlayer(PlayerModel player)
        {
            this.TraceMessage($"CurrentPlayer = {player}");
        }

        private ObservableCollection<PlayerModel> PossibleTradePartners = new ObservableCollection<PlayerModel>();


        public ObservableCollection<PlayerModel> PlayingPlayers
        {
            get => (ObservableCollection<PlayerModel>)GetValue(PlayingPlayersProperty);
            set => SetValue(PlayingPlayersProperty, value);
        }

        private static void PlayingPlayersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as TradeCtrl;
            var depPropValue = (ObservableCollection<PlayerModel>)e.NewValue;
            depPropClass?.SetPlayingPlayers(depPropValue);
        }

        private void SetPlayingPlayers(ObservableCollection<PlayerModel> value)
        {
            PossibleTradePartners.Clear();
            PossibleTradePartners.AddRange(value);
        }

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public PlayerModel TheHuman
        {
            get => (PlayerModel)GetValue(TheHumanProperty);
            set => SetValue(TheHumanProperty, value);
        }

        #endregion Properties

        #region Constructors + Destructors

        public TradeCtrl()
        {
            this.InitializeComponent();
        }

        #endregion Constructors + Destructors

        #region Methods

        public ObservableCollection<PlayerTradeTracker> GetTradePartners(PlayerModel theHuman, PlayerModel currentPlayer, ObservableCollection<PlayerModel> playingPlayers)
        {
            if (MainPage.Current == null) return null;

            if (TheHuman == null) return null;
            if (currentPlayer == null) return null;
            if (playingPlayers == null) return null;
            if (playingPlayers.Count == 0) return null;

            theHuman.GameData.Trades.TradeRequest.AddPotentialTradingPartners(playingPlayers);
            if (theHuman.GameData.Trades.TradeRequest.Owner == null)
            {
                theHuman.GameData.Trades.TradeRequest.Owner = theHuman;
            }

            //
            // we don't want to display the current player

            var ret = new ObservableCollection<PlayerTradeTracker>();
            if (CurrentPlayer == TheHuman)
            {
                ret.AddRange(theHuman.GameData.Trades.TradeRequest.TradePartners);
                for (int i = ret.Count - 1; i >= 0; i--)
                {
                    if (ret[i].PlayerIdentifier == CurrentPlayer.PlayerIdentifier)
                    {
                        ret.RemoveAt(i);
                        return ret;
                    }
                }
            }
            else
            {
                PlayerTradeTracker tracker = new PlayerTradeTracker()
                {
                    PlayerName = CurrentPlayer.PlayerName,
                    InTrade = true,
                    PlayerIdentifier = CurrentPlayer.PlayerIdentifier
                };
                ret.Add(tracker);
            }
            return ret;

        }

        private void OnClickAll(object sender, RoutedEventArgs e)
        {
        }

        #endregion Methods

        public static double SmallOffers(bool smallOffers)
        {
            if (MainPage.Current == null) return 400;

            if (smallOffers)
                return 188;

            return 400;
        }

        private void OnClickPlayer(object sender, RoutedEventArgs e)
        {
            this.TraceMessage("What does this do?");
        }

        private async void OnDeleteOffer(TradeOffer offer)
        {
            if (offer.Owner != TheHuman) return;

            await DeleteTradeOfferLog.DeleteOffer(MainPage.Current, offer);
        }

        private async void OnSendOffer(TradeOffer offer)
        {
            await PlayerTradesLog.DoTrade(MainPage.Current, offer);

        }

        private void OnDone(object sender, RoutedEventArgs e)
        {

        }
    }
}