using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class TradeCtrl : UserControl
    {
        #region Delegates + Fields + Events + Enums

        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(TradeCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty PlayingPlayersProperty = DependencyProperty.Register("PlayingPlayers", typeof(ObservableCollection<PlayerModel>), typeof(TradeCtrl), new PropertyMetadata(null, PlayingPlayersChanged));
        public static readonly DependencyProperty TheHumanProperty = DependencyProperty.Register("TheHuman", typeof(PlayerModel), typeof(TradeCtrl), new PropertyMetadata(null));
        private ObservableCollection<PlayerModel> PossibleTradePartners = new ObservableCollection<PlayerModel>();

        public PlayerModel CurrentPlayer
        {
            get => (PlayerModel)GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }

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

        public ObservableCollection<PlayerTradeTracker> ExceptMe(ObservableCollection<PlayerTradeTracker> list)
        {
            var ret = new ObservableCollection<PlayerTradeTracker>();
            foreach (var p in list)
            {
                if (p.PlayerIdentifier != TheHuman.PlayerIdentifier)
                {
                    ret.Add(p);
                }
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

        private void OnDeleteOffer(TradeOffer offer)
        {
            //
            //  TODO: Send a message to delete the offer from the list
            // if (offer.Owner) protected...
            TheHuman.GameData.Trades.PotentialTrades.Remove(offer);
        }

        private void OnSendOffer(TradeOffer offer)
        {
            //
            //  TODO: Send a message
            foreach (var player in offer.TradePartners)
            {
                if (player.PlayerIdentifier == offer.Owner.PlayerIdentifier) continue;
                var o = new TradeOffer()
                {
                    Desire = new TradeResources(offer.Desire),
                    Offer = new TradeResources(offer.Offer),
                    Owner = MainPage.Current.NameToPlayer(offer.Owner.PlayerName),
                    TradePartners = new ObservableCollection<PlayerTradeTracker>()
                    {
                        new PlayerTradeTracker()
                        {
                            PlayerIdentifier = player.PlayerIdentifier,
                            PlayerName = player.PlayerName,
                            InTrade = true
                        }
                    },
                    OwnerApproved = offer.OwnerApproved,
                    PartnerApproved = false
                };

                TheHuman.GameData.Trades.PotentialTrades.Add(o);
            }
        }
    }
}