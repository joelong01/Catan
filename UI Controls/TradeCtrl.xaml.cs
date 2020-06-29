using System.Collections;
using System.Collections.ObjectModel;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

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



        private void OnClickAll(object sender, RoutedEventArgs e)
        {
        }

        #endregion Methods

        public static double SmallOffers(bool smallOffers)
        {
            if (MainPage.Current == null) return 387;

            if (smallOffers)
                return 188;

            return 387;
        }

        private void OnClickPlayer(object sender, RoutedEventArgs e)
        {
            this.TraceMessage("What does this do?");
        }

        
        private async void OnSendOffer(TradeOffer offer)
        {
            await PlayerTradesLog.DoTrade(MainPage.Current, offer);

        }

        private void OnDone(object sender, RoutedEventArgs e)
        {

        }
        private bool OwnerHitTest(PlayerModel player)
        {
            if (this.TheHuman.GameData.Trades.TradeRequest == null) return false;
            return (this.TheHuman.GameData.Trades.TradeRequest.Owner == player);
        }

        public static bool PartnerHitTest(ObservableCollection<PlayerTradeTracker> players)
        {
            if (players == null || players.Count == 0) return false;


            return (players[0].PlayerIdentifier == MainPage.Current?.TheHuman.PlayerIdentifier);
        }

        public static Brush PartnerImageBrush(ObservableCollection<PlayerTradeTracker> players)
        {
            if (players == null || players.Count == 0)
            {
                return (Brush)App.Current.Resources["ResourceType.Back"];
            }

            return OfferCtrl.PlayerIdToBrush(players[0].PlayerIdentifier);
        }

        private async void OnDelete(object sender, RoutedEventArgs e)
        {


            TradeOffer offer = sender as TradeOffer;
            if (offer.Owner != TheHuman) return;

            await DeleteTradeOfferLog.DeleteOffer(MainPage.Current, offer);
        }

        private void ApprovalChanged(object sender, RoutedEventArgs e)
        {
            
        }
    }
}