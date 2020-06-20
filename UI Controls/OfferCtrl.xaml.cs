using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class OfferCtrl : UserControl
    {
        public OfferCtrl()
        {
            this.InitializeComponent();
            MyOffer = new TradeResources();
            MyDesire = new TradeResources();
            PartnerDesire = new TradeResources();
            PartnerOffer = new TradeResources();
        }

        
        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(OfferCtrl), new PropertyMetadata(null, CurrentPlayerChanged));
        public static readonly DependencyProperty TradePartnerProperty = DependencyProperty.Register("TradePartner", typeof(PlayerModel), typeof(OfferCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty MyOfferProperty = DependencyProperty.Register("MyOffer", typeof(TradeResources), typeof(OfferCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty MyDesireProperty = DependencyProperty.Register("MyDesire", typeof(TradeResources), typeof(OfferCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty PartnerOfferProperty = DependencyProperty.Register("PartnerOffer", typeof(TradeResources), typeof(OfferCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty PartnerDesireProperty = DependencyProperty.Register("PartnerDesire", typeof(TradeResources), typeof(OfferCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty ShowDeleteButtonProperty = DependencyProperty.Register("ShowDeleteButton", typeof(bool), typeof(OfferCtrl), new PropertyMetadata(true));
        public bool ShowDeleteButton
        {
            get => (bool)GetValue(ShowDeleteButtonProperty);
            set => SetValue(ShowDeleteButtonProperty, value);
        }

        public TradeResources PartnerDesire
        {
            get => (TradeResources)GetValue(PartnerDesireProperty);
            set => SetValue(PartnerDesireProperty, value);
        }
        public TradeResources PartnerOffer
        {
            get => (TradeResources)GetValue(PartnerOfferProperty);
            set => SetValue(PartnerOfferProperty, value);
        }
        public TradeResources MyDesire
        {
            get => (TradeResources)GetValue(MyDesireProperty);
            set => SetValue(MyDesireProperty, value);
        }
        public TradeResources MyOffer
        {
            get => (TradeResources)GetValue(MyOfferProperty);
            set => SetValue(MyOfferProperty, value);
        }
        public PlayerModel TradePartner
        {
            get => (PlayerModel)GetValue(TradePartnerProperty);
            set => SetValue(TradePartnerProperty, value);
        }
        public PlayerModel CurrentPlayer
        {
            get => (PlayerModel)GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }
        private static void CurrentPlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as OfferCtrl;
            var depPropValue = (PlayerModel)e.NewValue;
            depPropClass?.SetCurrentPlayer(depPropValue);
        }
        private void SetCurrentPlayer(PlayerModel value)
        {

        }

        
        
        private void OnCurrentPlayerPressed(object sender, PointerRoutedEventArgs e)
        {
            MyOffer = new TradeResources();
            MyDesire = new TradeResources();
            PartnerDesire = new TradeResources();
            PartnerOffer = new TradeResources();
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

            if (MyOffer.GetCount(resourceType) > 0 && MyDesire.GetCount(resourceType) > 0)
            {
                MyOffer.SetResource(resourceType, 0);
            }
        }

        private void OnOfferChanged(TradeResources tradeResources, ResourceType resourceType, int count)
        {
            if (count == 0) return;

            if (MyOffer.GetCount(resourceType) > 0 && MyDesire.GetCount(resourceType) > 0)
            {
                MyDesire.SetResource(resourceType, 0);
            }
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {

        }
    }
}
