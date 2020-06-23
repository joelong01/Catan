using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public delegate void SendOfferHandler(TradeOffer offer);
    public delegate void DeletedHandler(TradeOffer offer);

    public sealed partial class OfferCtrl : UserControl
    {
        #region Delegates + Fields + Events + Enums

        public OfferCtrl()
        {
            this.InitializeComponent();
        }

        public event SendOfferHandler OnSendOffer;
        public event DeletedHandler OnDeleteOffer;

        public static readonly DependencyProperty ShowDeleteButtonProperty = DependencyProperty.Register("ShowDeleteButton", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowPartnerApprovalProperty = DependencyProperty.Register("ShowPartnerApproval", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));
        public static readonly DependencyProperty TradeOfferProperty = DependencyProperty.Register("TradeOffer", typeof(TradeOffer), typeof(OfferCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty ShowSendButtonProperty = DependencyProperty.Register("ShowSendButton", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowResourceButtonsProperty = DependencyProperty.Register("ShowResourceButtons", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowAllButtonProperty = DependencyProperty.Register("ShowAllButton", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));
        public static readonly DependencyProperty TradePartnersProperty = DependencyProperty.Register("TradePartners", typeof(ObservableCollection<PlayerTradeTracker>), typeof(OfferCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty ShowResetButtonProperty = DependencyProperty.Register("ShowResetButton", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(false));
        public bool ShowResetButton
        {
            get => (bool)GetValue(ShowResetButtonProperty);
            set => SetValue(ShowResetButtonProperty, value);
        }

        public ObservableCollection<PlayerTradeTracker> TradePartners
        {
            get => (ObservableCollection<PlayerTradeTracker>)GetValue(TradePartnersProperty);
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
                return players[0].PlayerId == TradeOffer.Owner.PlayerIdentifier;
            }

            return false;
          
        }

        

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

        private Brush PartnerImageBrush(ObservableCollection<PlayerTradeTracker> players)
        {
            if (players == null || players.Count == 0)
            {
                
                return (Brush)App.Current.Resources["ResourceType.Back"];
            }
                

            return PlayerIdToBrush(players[0].PlayerId);
        }

        private void OnSendClicked(object sender, RoutedEventArgs e)
        {
            string json = JsonSerializer.Serialize(TradeOffer);
            TradeOffer offer = JsonSerializer.Deserialize<TradeOffer>(json);

            for (int i=offer.TradePartners.Count - 1; i>=0; i--)
            {
                if (offer.TradePartners[i].InTrade == false)
                {
                    offer.TradePartners.RemoveAt(i);
                }
            }

            OnSendOffer?.Invoke(offer);
        }

        private void OnClickAll(object sender, RoutedEventArgs e)
        {
            this.TraceMessage("this this");
        }

        private void ResetTradeOffer()
        {
            TradeOffer.Offer = new TradeResources();
            TradeOffer.Desire = new TradeResources();
            TradeOffer.OwnerApproved = false;
            TradeOffer.PartnerApproved = false;
            TradeOffer.TradePartners.Clear();
            
        }

        

        private void OnReset(object sender, RoutedEventArgs e)
        {
            ResetTradeOffer();
        }

        private static bool IsTradePartner(PlayerModel player, ObservableCollection<PlayerModel> tradePartners)
        {
            if (tradePartners == null) return false;

            return tradePartners.Contains(player);
        }

       
    }
}