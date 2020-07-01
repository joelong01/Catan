using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Windows.UI;
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
        public static readonly DependencyProperty SelectedTradeProperty = DependencyProperty.Register("SelectedOffer", typeof(TradeOffer), typeof(TradeCtrl), new PropertyMetadata(null));
        public TradeOffer SelectedOffer
        {
            get => (TradeOffer)GetValue(SelectedTradeProperty);
            set => SetValue(SelectedTradeProperty, value);
        }
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


        private async void OnSendOffer(TradeOffer offer, List<PlayerModel> players)
        {
            await PlayerTradesLog.DoTrade(MainPage.Current, offer, players);

        }

        private void OnDone(object sender, RoutedEventArgs e)
        {

        }
        
        //  returns true if the passed in player is TheHuman
        //  this needs to be static because it is bound in a DataTemplate
        public static bool IsHuman(PlayerModel player)
        {
            if (player == null) return false;
            return (player == MainPage.Current?.TheHuman);
            
        }


        //  returns true if the passed in player is the CurrentPlayer
        //  this needs to be static because it is bound in a DataTemplate
        public static bool IsCurrentPlayer(PlayerModel player)
        {
            if (player == null) return false;
            return (player == MainPage.Current?.CurrentPlayer);

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


        private async void ApprovalChanged(object sender, RoutedEventArgs e)
        {

            if (!(((ToggleSwitch)sender).Tag is TradeOffer offer)) return;
            PlayerModel approver = offer.Partner.Player;
            
            if (TheHuman == offer.Owner.Player)
            {
                approver = offer.Owner.Player;
            }
            this.TraceMessage($"{offer}");

            await TradeApprovalChangedLog.ToggleTrade(MainPage.Current, offer, ((ToggleSwitch)sender).IsOn, approver);
        }

        
        
    }
}