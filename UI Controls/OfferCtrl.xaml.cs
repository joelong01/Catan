using System;
using System.Collections.ObjectModel;
using System.Text.Json;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public delegate void DeletedHandler(TradeOffer offer);

    public delegate void SendOfferHandler(TradeOffer offer);

    public sealed partial class OfferCtrl : UserControl
    {
        #region Delegates + Fields + Events + Enums

        public event DeletedHandler OnDeleteOffer;

        public event SendOfferHandler OnSendOffer;

        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(OfferCtrl), new PropertyMetadata(null));

        public static readonly DependencyProperty EnableSendProperty = DependencyProperty.Register("EnableSend", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(false));

        public static readonly DependencyProperty ListLayoutProperty = DependencyProperty.Register("ListLayout", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(false));

        public static readonly DependencyProperty ShowAllButtonProperty = DependencyProperty.Register("ShowAllButton", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));

        public static readonly DependencyProperty ShowDeleteButtonProperty = DependencyProperty.Register("ShowDeleteButton", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));

        public static readonly DependencyProperty ShowPartnerApprovalProperty = DependencyProperty.Register("ShowPartnerApproval", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));

        public static readonly DependencyProperty ShowResetButtonProperty = DependencyProperty.Register("ShowResetButton", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(false));

        public static readonly DependencyProperty ShowResourceButtonsProperty = DependencyProperty.Register("ShowResourceButtons", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));

        public static readonly DependencyProperty ShowSendButtonProperty = DependencyProperty.Register("ShowSendButton", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));

        public static readonly DependencyProperty TheHumanProperty = DependencyProperty.Register("TheHuman", typeof(PlayerModel), typeof(OfferCtrl), new PropertyMetadata(null));

        public static readonly DependencyProperty TradeOfferProperty = DependencyProperty.Register("TradeOffer", typeof(TradeOffer), typeof(OfferCtrl), new PropertyMetadata(null));

        public static readonly DependencyProperty TradePartnersProperty = DependencyProperty.Register("TradePartners", typeof(ObservableCollection<PlayerTradeTracker>), typeof(OfferCtrl), new PropertyMetadata(null));

        public PlayerModel CurrentPlayer
        {
            get => (PlayerModel)GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }

        public bool EnableSend
        {
            get => (bool)GetValue(EnableSendProperty);
            set => SetValue(EnableSendProperty, value);
        }

        public bool ListLayout
        {
            get => (bool)GetValue(ListLayoutProperty);
            set => SetValue(ListLayoutProperty, value);
        }

        public bool ShowAllButton
        {
            get => (bool)GetValue(ShowAllButtonProperty);
            set => SetValue(ShowAllButtonProperty, value);
        }

        public bool ShowResetButton
        {
            get => (bool)GetValue(ShowResetButtonProperty);
            set => SetValue(ShowResetButtonProperty, value);
        }

        public bool ShowResourceButtons
        {
            get => (bool)GetValue(ShowResourceButtonsProperty);
            set => SetValue(ShowResourceButtonsProperty, value);
        }

        public bool ShowSendButton
        {
            get => (bool)GetValue(ShowSendButtonProperty);
            set => SetValue(ShowSendButtonProperty, value);
        }

        public PlayerModel TheHuman
        {
            get => (PlayerModel)GetValue(TheHumanProperty);
            set => SetValue(TheHumanProperty, value);
        }

        public ObservableCollection<PlayerTradeTracker> TradePartners
        {
            get => (ObservableCollection<PlayerTradeTracker>)GetValue(TradePartnersProperty);
            set => SetValue(TradePartnersProperty, value);
        }

        public OfferCtrl()
        {
            this.InitializeComponent();
        }

        public Visibility Not(bool flag)
        {
            if (!flag)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public bool ShowDeleteButton
        {
            get => (bool)GetValue(ShowDeleteButtonProperty);
            set => SetValue(ShowDeleteButtonProperty, value);
        }

        public bool ShowPartnerApproval
        {
            get => (bool)GetValue(ShowPartnerApprovalProperty);
            set => SetValue(ShowPartnerApprovalProperty, value);
        }

        public TradeOffer TradeOffer
        {
            get => (TradeOffer)GetValue(TradeOfferProperty);
            set => SetValue(TradeOfferProperty, value);
        }

        #endregion Properties



        #region Methods

        private void OnCreatedByPressed(object sender, PointerRoutedEventArgs e)
        {
            TradeOffer.Owner.GameData.Trades.TradeRequest = new TradeOffer();
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            OnDeleteOffer?.Invoke(this.TradeOffer);
        }

        /// <summary>
        ///     if you change Desire, check to make sure that Offer and Desire don't share resources
        /// </summary>
        /// <param name="tradeResources"></param>
        /// <param name="resourceType"></param>
        /// <param name="count"></param>
        private void OnDesireChanged(TradeResources tradeResources, ResourceType resourceType, int count)
        {
            EnableSend = CheckEnableSendButton();

            if (count == 0)
            {
                return;
            }

            if (TradeOffer.Offer.GetCount(resourceType) > 0 && TradeOffer.Desire.GetCount(resourceType) > 0)
            {
                TradeOffer.Offer.SetResource(resourceType, 0);
            }
        }

        private void OnOfferChanged(TradeResources tradeResources, ResourceType resourceType, int count)
        {
            EnableSend = CheckEnableSendButton();
            if (count == 0) return;

            if (TradeOffer.Offer.GetCount(resourceType) > 0 && TradeOffer.Desire.GetCount(resourceType) > 0)
            {
                TradeOffer.Desire.SetResource(resourceType, 0);
            }
        }

        #endregion Methods

        public static Brush PlayerIdToBrush(Guid id)
        {
            foreach (var player in MainPage.Current.MainPageModel.PlayingPlayers)
            {
                if (player.PlayerIdentifier == id)
                {
                    return player.ImageBrush;
                }
            }
            id.TraceMessage($"back image brush for {id}");
            return (Brush)App.Current.Resources["ResourceType.Back"];
        }

        private static bool IsTradePartner(PlayerModel player, ObservableCollection<PlayerModel> tradePartners)
        {
            if (tradePartners == null) return false;

            return tradePartners.Contains(player);
        }

        private bool CheckEnableSendButton()
        {
            if (this.TradeOffer == null)
            {
                return false;
            }

            if (this.TradeOffer.Offer.Count == 0) return false;
            if (this.TradeOffer.Desire.Count == 0) return false;

            foreach (var partner in this.TradeOffer.TradePartners)
            {
                if (partner.PlayerIdentifier == this.TradeOffer.Owner.PlayerIdentifier) continue;
                if (partner.InTrade)
                {
                    return true;
                }
            }
            return false;
        }

        private void OnClickAll(object sender, RoutedEventArgs e)
        {
            TradeOffer.TradePartners.ForEach((p) => p.InTrade = true);
            EnableSend = CheckEnableSendButton();
        }

        private void OnClickTradingPartner(object sender, RoutedEventArgs e)
        {
            EnableSend = CheckEnableSendButton();
        }

        private void OnReset(object sender, RoutedEventArgs e)
        {
            ResetTradeOffer();
        }

        private void OnSendClicked(object sender, RoutedEventArgs e)
        {
            string json = JsonSerializer.Serialize(TradeOffer);
            TradeOffer offer = JsonSerializer.Deserialize<TradeOffer>(json);

            for (int i = offer.TradePartners.Count - 1; i >= 0; i--)
            {
                if (offer.TradePartners[i].InTrade == false)
                {
                    offer.TradePartners.RemoveAt(i);
                }
            }

            OnSendOffer?.Invoke(offer);
        }

        private bool OwnerHitTest(PlayerModel player)
        {
            if (TradeOffer == null) return false;
            return (TradeOffer.Owner == player);
        }

        private bool PartnerHitTest(ObservableCollection<PlayerTradeTracker> players)
        {
            if (players == null || players.Count == 0) return false;
            if (TradeOffer.Owner != null)
            {
                return players[0].PlayerIdentifier == TradeOffer.Owner.PlayerIdentifier;
            }

            return false;
        }

        private Brush PartnerImageBrush(ObservableCollection<PlayerTradeTracker> players)
        {
            if (players == null || players.Count == 0)
            {
                return (Brush)App.Current.Resources["ResourceType.Back"];
            }

            return PlayerIdToBrush(players[0].PlayerIdentifier);
        }

        private void ResetTradeOffer()
        {
            TradeOffer.Offer = new TradeResources();
            TradeOffer.Desire = new TradeResources();
            TradeOffer.OwnerApproved = false;
            TradeOffer.PartnerApproved = false;
            TradeOffer.TradePartners.ForEach((p) => p.InTrade = false);
        }

        public ObservableCollection<PlayerTradeTracker> TradeWith(ObservableCollection<PlayerTradeTracker> list)
        {

            var ret = new ObservableCollection<PlayerTradeTracker>();
            if (list == null) return ret;
            foreach (var p in list)
            {
                //
                //  if you are the current player then you can trade with anyody but yourself
                if (CurrentPlayer == TheHuman && p.PlayerIdentifier != TheHuman.PlayerIdentifier)
                {
                    ret.Add(p);
                }

                // if you are not the current player, the only person you can trade with is the current player
                else if (CurrentPlayer != TheHuman && p.PlayerIdentifier != CurrentPlayer.PlayerIdentifier )
                {
                    ret.Add(p);
                    break;
                }
            }


            return ret;
        }
    }
}