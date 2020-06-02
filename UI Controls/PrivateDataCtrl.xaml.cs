using System;
using System.Diagnostics.Contracts;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class PrivateDataCtrl : UserControl
    {
        private static void PlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PrivateDataCtrl;
            var depPropValue = (PlayerModel)e.NewValue;
            depPropClass?.SetPlayer(depPropValue);
        }

        private bool EnabledEntitlementPurchase(string entitlementValue, PlayerResources playerResources)
        {
            if (MainPage.Current.CurrentGameState != GameState.WaitingForNext && MainPage.Current.CurrentGameState != GameState.Supplemental)
                return false;

            if (!Enum.TryParse(entitlementValue, out Entitlement entitlement))
            {
                Contract.Assert(false, "back string in xaml passed to EnableEntitlement");
            }
            Contract.Assert(playerResources != null);

            return playerResources.CanAfford(entitlement);
        }

        private async void OnBuyCity(object sender, RoutedEventArgs e)
        {
            if (!Player.GameData.Resources.CanAfford(Entitlement.City)) return;
            await PurchaseLog.PostLog(MainPage.Current, Player, Entitlement.City);
        }

        private async void OnBuyDevCard(object sender, RoutedEventArgs e)
        {
            if (!Player.GameData.Resources.CanAfford(Entitlement.DevCard)) return;
            await PurchaseLog.PostLog(MainPage.Current, Player, Entitlement.DevCard);
        }

        private async void OnBuyRoad(object sender, RoutedEventArgs e)
        {
            if (!Player.GameData.Resources.CanAfford(Entitlement.Road)) return;
            await PurchaseLog.PostLog(MainPage.Current, Player, Entitlement.Road);
        }

        private async void OnBuySettlement(object sender, RoutedEventArgs e)
        {
            if (Player.GameData.Resources.CanAfford(Entitlement.Settlement))
            {
                await PurchaseLog.PostLog(MainPage.Current, Player, Entitlement.Settlement);
            }
        }

        private async void OnPointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            UIElement uiElement = sender as UIElement;
            int zIndex = Canvas.GetZIndex(uiElement);
            if (e.GetCurrentPoint(uiElement).Position.Y < 30)
            {
                Canvas.SetZIndex(uiElement, zIndex + 1000);

                if (sender.GetType() == typeof(Grid))
                {
                    Grid grid = sender as Grid;
                    await StaticHelpers.DragAsync(grid, e);
                }

                Canvas.SetZIndex(uiElement, zIndex);
            }
        }

        private void OnTrade(object sender, RoutedEventArgs e)
        {
        }

        private void SetPlayer(PlayerModel value)
        {
            if (value == null) return;
        }

        public PrivateDataCtrl()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        public PlayerModel Player
        {
            get => (PlayerModel)GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }

        public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(PlayerModel), typeof(PrivateDataCtrl), new PropertyMetadata(new PlayerModel(), PlayerChanged));
    }
}
