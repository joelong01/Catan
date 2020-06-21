using System.Collections.ObjectModel;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public delegate void SendOfferHandler(TradeOffer offer);

    public sealed partial class OfferCtrl : UserControl
    {
        #region Delegates + Fields + Events + Enums

        public OfferCtrl()
        {
            this.InitializeComponent();
        }

        public SendOfferHandler OnSendOffer;

        public static readonly DependencyProperty ShowDeleteButtonProperty = DependencyProperty.Register("ShowDeleteButton", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowPartnerApprovalProperty = DependencyProperty.Register("ShowPartnerApproval", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));
        public static readonly DependencyProperty TradeOfferProperty = DependencyProperty.Register("TradeOffer", typeof(TradeOffer), typeof(OfferCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty ShowSendButtonProperty = DependencyProperty.Register("ShowSendButton", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowResourceButtonsProperty = DependencyProperty.Register("ShowResourceButtons", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowAllButtonProperty = DependencyProperty.Register("ShowAllButton", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));
        public static readonly DependencyProperty TradePartnersProperty = DependencyProperty.Register("TradePartners", typeof(ObservableCollection<PlayerModel>), typeof(OfferCtrl), new PropertyMetadata(null));

        public ObservableCollection<PlayerModel> TradePartners
        {
            get => (ObservableCollection<PlayerModel>)GetValue(TradePartnersProperty);
            set => SetValue(TradePartnersProperty, value);
        }

        public bool ShowAllButton
        {
            get => (bool)GetValue(ShowAllButtonProperty);
            set => SetValue(ShowAllButtonProperty, value);
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

        #region Constructors + Destructors

       

        #endregion Constructors + Destructors

        #region Methods

        private TradeOffer FindOffer(TradeOffer offer, ObservableCollection<TradeOffer> offers)
        {
            foreach (TradeOffer tradeOffer in offers)
            {
                if ((tradeOffer.TradePartner.PlayerIdentifier == offer.TradePartner.PlayerIdentifier) &&
                    (tradeOffer.Owner.PlayerIdentifier == offer.Owner.PlayerIdentifier) &&
                    (tradeOffer.Desire.EqualValue(offer.Desire)) &&
                    (tradeOffer.Offer.EqualValue(offer.Offer)))
                {
                    return tradeOffer;
                }
            }
            return null;
        }

        private bool OfferInCollection(TradeOffer offer, ObservableCollection<TradeOffer> offers)
        {
            return FindOffer(offer, offers) != null;
        }

        private void OnCreatedByPressed(object sender, PointerRoutedEventArgs e)
        {
            TradeOffer.Owner.GameData.Trades.TradeRequest = new TradeOffer();
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            TradeOffer offer = FindOffer(TradeOffer.Owner.GameData.Trades.TradeRequest, TradeOffer.Owner.GameData.Trades.PotentialTrades);
            if (offer != null)
            {
                this.TraceMessage("this needs to be a message!!");
                TradeOffer.Owner.GameData.Trades.PotentialTrades.Remove(offer);
            }
        }

        /// <summary>
        ///     if you change Desire, check to make sure that Offer and Desire don't share resources
        /// </summary>
        /// <param name="tradeResources"></param>
        /// <param name="resourceType"></param>
        /// <param name="count"></param>
        private void OnDesireChanged(TradeResources tradeResources, ResourceType resourceType, int count)
        {
            if (count == 0) return;

            if (TradeOffer.Offer.GetCount(resourceType) > 0 && TradeOffer.Desire.GetCount(resourceType) > 0)
            {
                TradeOffer.Offer.SetResource(resourceType, 0);
            }
        }

        private void OnOfferChanged(TradeResources tradeResources, ResourceType resourceType, int count)
        {
            if (count == 0) return;

            if (TradeOffer.Offer.GetCount(resourceType) > 0 && TradeOffer.Desire.GetCount(resourceType) > 0)
            {
                TradeOffer.Desire.SetResource(resourceType, 0);
            }
        }

        #endregion Methods

        /// <summary>
        ///     if ownerTest is true, check for ownershipo
        ///     if ownerTest is false, check for tradepartner
        /// </summary>
        /// <param name="player"></param>
        /// <param name="ownerTest"></param>
        /// <returns></returns>
        private bool OwnerHitTest(PlayerModel player, bool ownerTest)
        {
            //if (player == null) return false;
            //if (ownerTest)
            //{
            //    if (MainPage.Current?.TheHuman == TradeOffer.Owner)
            //        return true;

            //    return false;
            //}
            //else
            //{
            //    if (MainPage.Current?.TheHuman == TradeOffer.TradePartner)
            //        return true;

            //    return false;
            //}
            return true;
        }

        private void OnSendClicked(object sender, RoutedEventArgs e)
        {
            OnSendOffer?.Invoke(this.TradeOffer);
        }

        private void OnClickAll(object sender, RoutedEventArgs e)
        {
            foreach (PlayerModel player in GridView_Players.Items)
            {
                player.GameData.InvitedToTrade = true;
            }
        }

        public ObservableCollection<PlayerModel> ExceptMe(ObservableCollection<PlayerModel> list)
        {

            var ret = new ObservableCollection<PlayerModel>();
            if (list == null) return ret;
            ret.AddRange(list);
            if (list.Count > 1)
            {
                //
                //  on a counter offer, there will only be one person and it will be the Human
                ret.Remove(MainPage.Current.TheHuman);
            }
            return ret;
        }

        private void OnClickPlayer(object sender, RoutedEventArgs e)
        {
        }
    }
}