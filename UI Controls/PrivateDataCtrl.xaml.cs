using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

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

        private async Task<TradeResources> DoMonopoly()
        {
            TradeResources tr = new TradeResources()
            {
                Wood = 1,
                Wheat = 1,
                Brick = 1,
                Ore = 1,
                Sheep = 1
            };

            ResourceCardCollection rc = new ResourceCardCollection();
            rc.InitalizeResources(tr);
            TakeCardDlg dlg = new TakeCardDlg()
            {
                To = Player,
                From = MainPage.Current.MainPageModel.Bank,
                SourceOrientation = TileOrientation.FaceUp,
                HowMany = 1,
                Source = rc,
                Instructions = "Take 2 cards from the bank.",
                Destination = new ObservableCollection<ResourceCardModel>(),
            };

            var ret = await dlg.ShowAsync();
            if (ret == ContentDialogResult.Primary)
            {
                return ResourceCardCollection.ToTradeResources(dlg.Destination);
            }

            return null;
        }

        private async Task<TradeResources> DoYearOfPlenty()
        {
            TradeResources tr = new TradeResources()
            {
                Wood = 2,
                Wheat = 2,
                Brick = 2,
                Ore = 2,
                Sheep = 2
            };

            ResourceCardCollection rc = new ResourceCardCollection();
            rc.InitalizeResources(tr);
            TakeCardDlg dlg = new TakeCardDlg()
            {
                To = Player,
                From = MainPage.Current.MainPageModel.Bank,
                SourceOrientation = TileOrientation.FaceUp,
                HowMany = 2,
                Source = rc,
                Instructions = "Take 2 cards from the bank.",
                Destination = new ObservableCollection<ResourceCardModel>(),
            };

            var ret = await dlg.ShowAsync();
            if (ret == ContentDialogResult.Primary)
            {
                return ResourceCardCollection.ToTradeResources(dlg.Destination);
            }

            return null;
        }

        private bool EnabledEntitlementPurchase(string entitlementValue, PlayerResources playerResources)
        {
            if (StaticHelpers.IsInVisualStudioDesignMode)
                return false;

            if (MainPage.Current.CurrentGameState != GameState.WaitingForNext && MainPage.Current.CurrentGameState != GameState.Supplemental)
                return false;

            if (!Enum.TryParse(entitlementValue, out Entitlement entitlement))
            {
                Contract.Assert(false, "back string in xaml passed to EnableEntitlement");
            }
            Contract.Assert(playerResources != null);

            return playerResources.CanAfford(entitlement);
        }

        private async void OnAvailableCardPressed(object sender, RoutedEventArgs e)
        {
            DevCardType devCardType = (DevCardType)((MenuFlyoutItem)sender).Tag;
            Contract.Assert(SelectedAvailableDevCard != null);
            Contract.Assert(SelectedAvailableDevCard.DevCardType == devCardType);
            Contract.Assert(SelectedAvailableDevCard.Played == false);

            if (devCardType == DevCardType.YearOfPlenty)
            {
                TradeResources tr = await DoYearOfPlenty();
                await PlayDevCardLog.PostLog(MainPage.Current, DevCardType.YearOfPlenty, tr);
                return;
            }

            if (devCardType == DevCardType.Monopoly)
            {
                TradeResources tr = await DoMonopoly();
                await PlayDevCardLog.PostLog(MainPage.Current, DevCardType.Monopoly, tr);
                return;
            }

            if (devCardType == DevCardType.RoadBuilding)
            {
                await PlayDevCardLog.PostLog(MainPage.Current, DevCardType.RoadBuilding, null);
                return;
            }
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

        public PlayerModel Player
        {
            get => (PlayerModel)GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }

        public DevCardModel SelectedAvailableDevCard
        {
            get => (DevCardModel)GetValue(SelectedAvailableDevCardProperty);
            set => SetValue(SelectedAvailableDevCardProperty, value);
        }

        public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(PlayerModel), typeof(PrivateDataCtrl), new PropertyMetadata(new PlayerModel(), PlayerChanged));

        public static readonly DependencyProperty SelectedAvailableDevCardProperty = DependencyProperty.Register("SelectedAvailableDevCard", typeof(DevCardModel), typeof(PrivateDataCtrl), new PropertyMetadata(null));

        public PrivateDataCtrl()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        public static string MenuPlayString(DevCardType devCardType)
        {
            string description = devCardType.Description();
            return $"Play {description}?";
        }
    }
}
