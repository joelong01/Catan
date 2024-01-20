using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class PrivateDataCtrl : UserControl
    {
        #region Delegates + Fields + Events + Enums

        public static readonly DependencyProperty AvailableDevCardIndexProperty = DependencyProperty.Register("AvailableDevCardIndex", typeof(int), typeof(PrivateDataCtrl), new PropertyMetadata(0));

        public static readonly DependencyProperty NewDevCardsIndexProperty = DependencyProperty.Register("NewDevCardsIndex", typeof(int), typeof(PrivateDataCtrl), new PropertyMetadata(0));

        public static readonly DependencyProperty PlayedDevCardIndexProperty = DependencyProperty.Register("PlayedDevCardIndex", typeof(int), typeof(PrivateDataCtrl), new PropertyMetadata(0));

        public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(PlayerModel), typeof(PrivateDataCtrl), new PropertyMetadata(PlayerModel.DefaultPlayer, PlayerChanged));

        public static readonly DependencyProperty SelectedAvailableDevCardProperty = DependencyProperty.Register("SelectedAvailableDevCard", typeof(DevCardModel), typeof(PrivateDataCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty StolenResourceProperty = DependencyProperty.Register("StolenResource", typeof(ResourceType), typeof(PrivateDataCtrl), new PropertyMetadata(ResourceType.None, StolenResourceChanged));

        private string ActualScore(int publicScore, ObservableCollection<DevCardModel> newDevCards, ObservableCollection<DevCardModel> available)
        {
            int victoryPoints = 0;
            newDevCards.ForEach((c) => { if (c.DevCardType == DevCardType.VictoryPoint) victoryPoints++; });
            available.ForEach((c) => { if (c.DevCardType == DevCardType.VictoryPoint) victoryPoints++; });

            return $"{publicScore + victoryPoints}";
        }

        public ResourceType StolenResource
        {
            get => ( ResourceType )GetValue(StolenResourceProperty);
            set => SetValue(StolenResourceProperty, value);
        }

        private static void StolenResourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PrivateDataCtrl;
            var depPropValue = (ResourceType)e.NewValue;
            depPropClass?.SetStolenResource(depPropValue);
        }

        private void SetStolenResource(ResourceType resourceType)
        {
        }

        private bool StolenResourceEmpty(ResourceType resourceType)
        {
            return resourceType != ResourceType.None;
        }

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public int AvailableDevCardIndex
        {
            get => ( int )GetValue(AvailableDevCardIndexProperty);
            set => SetValue(AvailableDevCardIndexProperty, value);
        }

        public int NewDevCardsIndex
        {
            get => ( int )GetValue(NewDevCardsIndexProperty);
            set => SetValue(NewDevCardsIndexProperty, value);
        }

        public int PlayedDevCardIndex
        {
            get => ( int )GetValue(PlayedDevCardIndexProperty);
            set => SetValue(PlayedDevCardIndexProperty, value);
        }

        public PlayerModel Player
        {
            get => ( PlayerModel )GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }

        public DevCardModel SelectedAvailableDevCard
        {
            get => ( DevCardModel )GetValue(SelectedAvailableDevCardProperty);
            set => SetValue(SelectedAvailableDevCardProperty, value);
        }

        #endregion Properties

        #region Constructors + Destructors

        public PrivateDataCtrl()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        #endregion Constructors + Destructors

        #region Methods

        public static bool EnableMenuForGameState(DevCardType dcType, GameState state)
        {
            if (dcType == DevCardType.VictoryPoint) return false;

            if (dcType == DevCardType.Knight && state == GameState.WaitingForRoll)
            {
                return true;
            }

            return ( state == GameState.WaitingForNext );
        }

        public static string MenuPlayString(DevCardType devCardType)
        {
            string description = devCardType.Description();
            return $"Play {description}?";
        }

        public string XOfYText(int index, int count)
        {
            if (count == 0) return "";
            return $"{index + 1} of {count}";
        }

        private static void PlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PrivateDataCtrl;
            var depPropValue = (PlayerModel)e.NewValue;
            depPropClass?.SetPlayer(depPropValue);
        }

        private bool AtMaxEntitlement(PlayerModel player, Entitlement entitlement)
        {
            switch (entitlement)
            {
                case Entitlement.Settlement:
                    if (player.GameData.Settlements.Count - 1 == MainPage.Current.GameData.MaxSettlements)
                        return true;
                    break;

                case Entitlement.City:
                    if (player.GameData.Cities.Count - 1 == MainPage.Current.GameData.MaxCities)
                        return true;
                    break;

                case Entitlement.Road:
                    if (player.GameData.Roads.Count - 1 == MainPage.Current.GameData.MaxRoads)
                        return true; ;
                    break;

                case Entitlement.Ship:
                    if (player.GameData.Settlements.Count - 1 == MainPage.Current.GameData.MaxSettlements)
                        return true; ;
                    break;

                case Entitlement.BuyKnight:
                    if (player.GameData.CK_Knights.Count - 1 == MainPage.Current.GameData.MaxKnights)
                        return true;
                    break;

                default:
                    break;
            }
            return false;
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

            ResourceCardCollection rc = new ResourceCardCollection(false);
            rc.AddResources(tr);
            TakeCardDlg dlg = new TakeCardDlg()
            {
                To = Player,
                From = MainPage.Current.MainPageModel.Bank,
                SourceOrientation = TileOrientation.FaceUp,
                HowMany = 1,
                Source = rc,
                CountVisible = false,
                Instructions = "Pick the resource for Monopoly",
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
            TradeResources tr = MainPage.Current.MainPageModel.ResourcesLeftInBank;

            ResourceCardCollection rc = new ResourceCardCollection(false);
            rc.AddResources(tr);
            TakeCardDlg dlg = new TakeCardDlg()
            {
                To = Player,
                From = MainPage.Current.MainPageModel.Bank,
                SourceOrientation = TileOrientation.FaceUp,
                HowMany = 2,
                Source = rc,
                CountVisible = true,
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

        private bool EnabledEntitlementPurchase(string entitlementValue, TradeResources tradeResources, GameState gameState)
        {
            if (StaticHelpers.IsInVisualStudioDesignMode)
                return false;

            if (MainPage.Current.CurrentPlayer != MainPage.Current.TheHuman) return false; // you can only buy on your turn

            if (gameState != GameState.WaitingForNext && gameState != GameState.Supplemental)
            {
                return false;
            }

            if (!Enum.TryParse(entitlementValue, out Entitlement entitlement))
            {
                Contract.Assert(false, "bad string in xaml passed to EnableEntitlement");
            }
            Contract.Assert(tradeResources != null);

            bool ret = tradeResources.CanAfford(MainPage.Current.CurrentPlayer, entitlement);
            return ret;
        }

        /// <summary>
        ///         We added a menu to the "Available Dev Cads" collection, and the user picked one to play
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnAvailableCardPressed(object sender, RoutedEventArgs e)
        {
            if (Player.GameData.Resources.ThisTurnsDevCard.DevCardType != DevCardType.None)
            {
                ContentDialog dlg = new ContentDialog()
                {
                    Title = "Play Dev Card",
                    Content = $"You can only play one dev card per turn and you've already played a {Player.GameData.Resources.ThisTurnsDevCard.DevCardType}.",
                    CloseButtonText = "Ok",
                };
                await dlg.ShowAsync();
                return; // you can only play one dev card per turn
            }

            DevCardType devCardType = (DevCardType)((MenuFlyoutItem)sender).Tag;
            Contract.Assert(SelectedAvailableDevCard != null);
            Contract.Assert(SelectedAvailableDevCard.DevCardType == devCardType);
            Contract.Assert(SelectedAvailableDevCard.Played == false);

            GameState state = MainPage.Current.MainPageModel.GameState;

            if (devCardType == DevCardType.YearOfPlenty)
            {
                if (state != GameState.WaitingForNext) return;
                TradeResources tr = await DoYearOfPlenty();
                await PlayDevCardLog.PostLog(MainPage.Current, DevCardType.YearOfPlenty, tr);
                return;
            }

            if (devCardType == DevCardType.Monopoly)
            {
                if (state != GameState.WaitingForNext) return;
                TradeResources tr = await DoMonopoly();
                await PlayDevCardLog.PostLog(MainPage.Current, DevCardType.Monopoly, tr);
                return;
            }

            if (devCardType == DevCardType.RoadBuilding)
            {
                if (state != GameState.WaitingForNext) return;
                await PlayDevCardLog.PostLog(MainPage.Current, DevCardType.RoadBuilding, null);
                return;
            }
            if (devCardType == DevCardType.Knight && ( state == GameState.WaitingForNext || state == GameState.WaitingForRoll ))
            {
                await MustMoveBaronLog.PostLog(MainPage.Current, MoveBaronReason.PlayedDevCard);
            }
        }

        private async void OnBuyCity(object sender, RoutedEventArgs e)
        {
            GameState state = MainPage.Current.MainPageModel.GameState;
            if (state != GameState.WaitingForNext && state != GameState.Supplemental) return;
            if (!Player.GameData.Resources.CanAfford(MainPage.Current.CurrentPlayer, Entitlement.City)) return;
            if (AtMaxEntitlement(Player, Entitlement.City))
            {
                await MainPage.Current.ShowErrorMessage($"You have purchased all available cities.\n\n", "Catan", "");
            }
            else
            {
                await PurchaseLog.PostLog(MainPage.Current, Player, Entitlement.City, MainPage.Current.CurrentGameState);
            }
        }

        private async void OnBuyDevCard(object sender, RoutedEventArgs e)
        {
            GameState state = MainPage.Current.MainPageModel.GameState;
            if (state != GameState.WaitingForNext && state != GameState.Supplemental) return;
            if (!Player.GameData.Resources.CanAfford(MainPage.Current.CurrentPlayer, Entitlement.DevCard)) return;
            await PurchaseLog.PostLog(MainPage.Current, Player, Entitlement.DevCard, MainPage.Current.CurrentGameState);
        }

        private async void OnBuyRoad(object sender, RoutedEventArgs e)
        {
            GameState state = MainPage.Current.MainPageModel.GameState;
            if (state != GameState.WaitingForNext && state != GameState.Supplemental) return;
            if (!Player.GameData.Resources.CanAfford(MainPage.Current.CurrentPlayer, Entitlement.Road)) return;
            if (AtMaxEntitlement(Player, Entitlement.Road))
            {
                await MainPage.Current.ShowErrorMessage($"You have purchased all available roads.", "Catan", "");
            }
            else
            {
                await PurchaseLog.PostLog(MainPage.Current, Player, Entitlement.Road, MainPage.Current.CurrentGameState);
            }
        }

        private async void OnBuySettlement(object sender, RoutedEventArgs e)
        {
            GameState state = MainPage.Current.MainPageModel.GameState;
            if (state != GameState.WaitingForNext && state != GameState.Supplemental) return;
            if (Player.GameData.Resources.CanAfford(MainPage.Current.CurrentPlayer, Entitlement.Settlement))
            {
                if (AtMaxEntitlement(Player, Entitlement.Settlement))
                {
                    await MainPage.Current.ShowErrorMessage($"You have purchased all available Settlements.", "Catan", "");
                }
                else
                {
                    await PurchaseLog.PostLog(MainPage.Current, Player, Entitlement.Settlement, MainPage.Current.CurrentGameState);
                }
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

        private void OnTrade4For1(object sender, RoutedEventArgs e)
        {
            TradeMenu.Items.Clear();
            foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
            {
                var menu = HarborCtrl.BuildResourceTradeMenu(this.Player, 4, resource);
                if (menu != null)
                {
                    TradeMenu.Items.Add(menu);
                }
            }

            //
            //  if they have a menu to display, show it
            if (TradeMenu.Items.Count > 0)
            {
                TradeMenu.ShowAt(this, new Point(0, 0));
            }
        }

        private void SetPlayer(PlayerModel value)
        {
            if (value == null) return;
        }

        #endregion Methods

    }
}