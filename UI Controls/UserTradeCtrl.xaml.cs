using System.Collections.Generic;
using System.Collections.ObjectModel;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class UserTradeCtrl : UserControl
    {
        #region Delegates + Fields + Events + Enums

        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(UserTradeCtrl), new PropertyMetadata(null, CurrentPlayerChanged));
        public static readonly DependencyProperty MyOffersOnlyProperty = DependencyProperty.Register("MyOffersOnly", typeof(bool), typeof(UserTradeCtrl), new PropertyMetadata(false));
        public static readonly DependencyProperty PlayingPlayersProperty = DependencyProperty.Register("PlayingPlayers", typeof(ObservableCollection<PlayerModel>), typeof(UserTradeCtrl), new PropertyMetadata(null, PlayingPlayersChanged));
        public static readonly DependencyProperty SelectedTradeProperty = DependencyProperty.Register("SelectedOffer", typeof(TradeOffer), typeof(UserTradeCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty TheHumanProperty = DependencyProperty.Register("TheHuman", typeof(PlayerModel), typeof(UserTradeCtrl), new PropertyMetadata(null));
        private ObservableCollection<PlayerModel> PossibleTradePartners = new ObservableCollection<PlayerModel>();

        public PlayerModel CurrentPlayer
        {
            get => (PlayerModel)GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }

        public bool MyOffersOnly
        {
            get => (bool)GetValue(MyOffersOnlyProperty);
            set => SetValue(MyOffersOnlyProperty, value);
        }

        public ObservableCollection<PlayerModel> PlayingPlayers
        {
            get => (ObservableCollection<PlayerModel>)GetValue(PlayingPlayersProperty);
            set => SetValue(PlayingPlayersProperty, value);
        }

        public TradeOffer SelectedOffer
        {
            get => (TradeOffer)GetValue(SelectedTradeProperty);
            set => SetValue(SelectedTradeProperty, value);
        }

        private static void CurrentPlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as UserTradeCtrl;
            var depPropValue = (PlayerModel)e.NewValue;
            depPropClass?.SetCurrentPlayer(depPropValue);
        }

        private static void PlayingPlayersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as UserTradeCtrl;
            var depPropValue = (ObservableCollection<PlayerModel>)e.NewValue;
            depPropClass?.SetPlayingPlayers(depPropValue);
        }

        private void SetCurrentPlayer(PlayerModel player)
        {
//            this.TraceMessage($"CurrentPlayer = {player}");
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

        public UserTradeCtrl()
        {
            this.InitializeComponent();
        }

        #endregion Constructors + Destructors

        #region Methods

        private void OnClickAll(object sender, RoutedEventArgs e)
        {
        }

        #endregion Methods

        //  returns true if the passed in player is the CurrentPlayer
        //  this needs to be static because it is bound in a DataTemplate
        public static bool IsCurrentPlayer(PlayerModel player)
        {
            if (player == null) return false;
            return (player == MainPage.Current?.CurrentPlayer);
        }

        //  returns true if the passed in player is TheHuman
        //  this needs to be static because it is bound in a DataTemplate
        public static bool IsHuman(PlayerModel player)
        {
            if (player == null) return false;
            return (player == MainPage.Current?.TheHuman);
        }

        public static double SmallOffers(bool smallOffers)
        {
            if (MainPage.Current == null) return 387;

            if (smallOffers)
                return 188;

            return 387;
        }

        private ObservableCollection<TradeOffer> FilteredOffers(ObservableCollection<TradeOffer> offers, bool mineOnly)
        {
            if (mineOnly == false) return offers;
            var list = new ObservableCollection<TradeOffer>();
            offers.ForEach((offer) =>
              {
                  if (offer.Owner.Player == TheHuman || offer.Partner.Player == TheHuman)
                  {
                      list.Add(offer);
                  }
              });
            return list;
        }

        private void OnClickPlayer(object sender, RoutedEventArgs e)
        {
            this.TraceMessage("What does this do?");
        }

        private async void OnDelete(object sender, RoutedEventArgs e)
        {
            if (!(((Button)sender).Tag is TradeOffer offer)) return;

            if (offer.Owner.Player == TheHuman)
            {
                //
                //  pull it from view everywhere
                await DeleteTradeOfferLog.DeleteOffer(MainPage.Current, offer);
            }
            else
            {
                //
                //  only remove it locally
                TheHuman.GameData.Trades.PotentialTrades.Remove(offer);
            }
        }

        private void OnDone(object sender, RoutedEventArgs e)
        {
        }

        private void OnFilterOffers(bool onlyOffersForMe)
        {
            MyOffersOnly = onlyOffersForMe;
        }

        private async void OnSendOffer(TradeOffer offer, List<PlayerModel> players)
        {
            await PlayerTradesLog.DoTrade(MainPage.Current, offer, players);
        }

        private async void OwnerApprovalChanged(object sender, RoutedEventArgs e)
        {
            if (!(((ToggleSwitch)sender).Tag is TradeOffer offer)) return;
            this.TraceMessage($"{offer}");
            bool isOn = ((ToggleSwitch)sender).IsOn;
            if (offer.Owner.Approved != isOn)
            {
                await TradeApprovalChangedLog.ToggleTrade(MainPage.Current, offer, isOn, offer.Owner.Player);
            }
        }

        private async void PartnerApprovalChanged(object sender, RoutedEventArgs e)
        {
            if (!(((ToggleSwitch)sender).Tag is TradeOffer offer)) return;
            this.TraceMessage($"{offer}");
            bool isOn = ((ToggleSwitch)sender).IsOn;
            if (offer.Partner.Approved != isOn)
            {
                await TradeApprovalChangedLog.ToggleTrade(MainPage.Current, offer, isOn, offer.Partner.Player);
            }
        }
    }
}