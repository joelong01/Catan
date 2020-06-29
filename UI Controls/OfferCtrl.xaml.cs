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


        public event SendOfferHandler OnSendOffer;

        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(OfferCtrl), new PropertyMetadata(null));

        public static readonly DependencyProperty EnableSendProperty = DependencyProperty.Register("EnableSend", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(false));

      
        public static readonly DependencyProperty ShowAllButtonProperty = DependencyProperty.Register("ShowAllButton", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));

        public static readonly DependencyProperty ShowDeleteButtonProperty = DependencyProperty.Register("ShowDeleteButton", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));

        public static readonly DependencyProperty ShowPartnerApprovalProperty = DependencyProperty.Register("ShowPartnerApproval", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));

        public static readonly DependencyProperty ShowResetButtonProperty = DependencyProperty.Register("ShowResetButton", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(false));

        public static readonly DependencyProperty ShowResourceButtonsProperty = DependencyProperty.Register("ShowResourceButtons", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));

        public static readonly DependencyProperty ShowSendButtonProperty = DependencyProperty.Register("ShowSendButton", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));

        public static readonly DependencyProperty TheHumanProperty = DependencyProperty.Register("TheHuman", typeof(PlayerModel), typeof(OfferCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty PlayingPlayersProperty = DependencyProperty.Register("PlayingPlayers", typeof(ObservableCollection<PlayerModel>), typeof(OfferCtrl), new PropertyMetadata(null));
        public ObservableCollection<PlayerModel> PlayingPlayers
        {
            get => (ObservableCollection<PlayerModel>)GetValue(PlayingPlayersProperty);
            set => SetValue(PlayingPlayersProperty, value);
        }


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

        

        #endregion Properties



        #region Methods


        
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

            if (this.TheHuman.GameData.Trades.TradeRequest.Offer.GetCount(resourceType) > 0 && this.TheHuman.GameData.Trades.TradeRequest.Desire.GetCount(resourceType) > 0)
            {
                this.TheHuman.GameData.Trades.TradeRequest.Offer.SetResource(resourceType, 0);
            }
        }

        private void OnOfferChanged(TradeResources tradeResources, ResourceType resourceType, int count)
        {
            EnableSend = CheckEnableSendButton();
            if (count == 0) return;

            if (this.TheHuman.GameData.Trades.TradeRequest.Offer.GetCount(resourceType) > 0 && this.TheHuman.GameData.Trades.TradeRequest.Desire.GetCount(resourceType) > 0)
            {
                this.TheHuman.GameData.Trades.TradeRequest.Desire.SetResource(resourceType, 0);
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
            if (this.TheHuman.GameData.Trades.TradeRequest.Offer == null)
            {
                return false;
            }

            if (this.TheHuman.GameData.Trades.TradeRequest.Offer.Count == 0) return false;
            if (this.TheHuman.GameData.Trades.TradeRequest.Desire.Count == 0) return false;

            foreach (var partner in this.TheHuman.GameData.Trades.TradeRequest.TradePartners)
            {
                if (partner.PlayerIdentifier == this.TheHuman.GameData.Trades.TradeRequest.Owner.PlayerIdentifier) continue;
                if (partner.InTrade)
                {
                    return true;
                }
            }
            return false;
        }

        private void OnClickAll(object sender, RoutedEventArgs e)
        {
            this.TheHuman.GameData.Trades.TradeRequest.TradePartners.ForEach((p) => p.InTrade = true);
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
            var offer = this.TheHuman.GameData.Trades.TradeRequest;
            for (int i = offer.TradePartners.Count - 1; i >= 0; i--)
            {
                if (offer.TradePartners[i].InTrade == false)
                {
                    offer.TradePartners.RemoveAt(i);
                }
            }

            OnSendOffer?.Invoke(offer);
        }

      

        private void ResetTradeOffer()
        {
            this.TheHuman.GameData.Trades.TradeRequest.Offer = new TradeResources();
            this.TheHuman.GameData.Trades.TradeRequest.Desire = new TradeResources();
            this.TheHuman.GameData.Trades.TradeRequest.OwnerApproved = false;
            this.TheHuman.GameData.Trades.TradeRequest.PartnerApproved = false;
            this.TheHuman.GameData.Trades.TradeRequest.TradePartners.ForEach((p) => p.InTrade = false);
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

        private async void ApprovalChanged(object sender, RoutedEventArgs e)
        {
            await TradeApprovalChangedLog.ToggleTrade(MainPage.Current, this.TheHuman.GameData.Trades.TradeRequest);
        }

        //
        //  this is Binding converter function for the OfferCtrl.xaml UI for showing whow we are going to trade with
        //  we need to make sure that the PlayerTradeTracker object this is updated with the two way binding is rooted in TheHuman object
        //  we need to take away anybody that the player can't trade with to display.
        public ObservableCollection<PlayerTradeTracker> GetTradePartners(PlayerModel theHuman, PlayerModel currentPlayer, ObservableCollection<PlayerModel> playingPlayers)
        {
            if (MainPage.Current == null) return null;

            if (TheHuman == null) return null;
            if (currentPlayer == null) return null;
            if (playingPlayers == null) return null;
            if (playingPlayers.Count == 0) return null;

            theHuman.GameData.Trades.TradeRequest.TradePartners.Clear(); // this should be called with currentPlayer changes

            if (currentPlayer != TheHuman)
            {
                //
                //  if it ain't your turn, you can only trade with the CurrentPlayer
                theHuman.GameData.Trades.TradeRequest.TradePartners.Add(new PlayerTradeTracker() { InTrade = true, PlayerIdentifier = CurrentPlayer.PlayerIdentifier, PlayerName = CurrentPlayer.PlayerName });
                return theHuman.GameData.Trades.TradeRequest.TradePartners;                
            }
            

            foreach (var player in playingPlayers)
            {
                if (player == TheHuman) continue; //can't trade with yourself
                PlayerTradeTracker tracker = new PlayerTradeTracker()
                {
                    PlayerName = player.PlayerName,
                    InTrade = false,
                    PlayerIdentifier = player.PlayerIdentifier
                };

                theHuman.GameData.Trades.TradeRequest.TradePartners.Add(tracker);
            }
            if (theHuman.GameData.Trades.TradeRequest.Owner == null)
            {
                theHuman.GameData.Trades.TradeRequest.Owner = theHuman;
            }

            return theHuman.GameData.Trades.TradeRequest.TradePartners;
        }


    }
}