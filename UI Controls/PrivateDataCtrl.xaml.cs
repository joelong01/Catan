﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class PrivateDataCtrl : UserControl
    {
        #region Properties + Fields 

        #endregion Properties + Fields 

        #region Constructors

        #endregion Constructors

        #region Delegates  + Events + Enums

        #endregion Delegates  + Events + Enums

        #region Methods

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

                case Entitlement.Knight:
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

            bool ret = tradeResources.CanAfford(entitlement);
            return ret;
        }

        /// <summary>
        ///         We added a menu to the "Available Dev Cads" collection, and the user picked one to play
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            if (devCardType == DevCardType.Knight)
            {
                /**
                 *  1. send a message that a knight was played
                 *  2. change the state so that the UI says "Much move baron"
                 *  3. pick the target tile 
                 *  4. Send a message to move Baron to the target tile
                 *  5. show the UI to pick one of the victim's cards
                 *  6. Send a message to update the resources for each player
                 */

                
                 
            }
        }

        private async void OnBuyCity(object sender, RoutedEventArgs e)
        {
            if (!Player.GameData.Resources.CanAfford(Entitlement.City)) return;
            if (AtMaxEntitlement(Player, Entitlement.City))
            {
                await StaticHelpers.ShowErrorText($"You have purchased all available cities.", "Catan");
            }
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
            if (AtMaxEntitlement(Player, Entitlement.Road))
            {
                await StaticHelpers.ShowErrorText($"You have purchased all available roads.", "Catan");
            }
            await PurchaseLog.PostLog(MainPage.Current, Player, Entitlement.Road);
        }

        private async void OnBuySettlement(object sender, RoutedEventArgs e)
        {
            if (Player.GameData.Resources.CanAfford(Entitlement.Settlement))
            {
                if (AtMaxEntitlement(Player, Entitlement.Settlement))
                {
                    await StaticHelpers.ShowErrorText($"You have purchased all available Settlements.", "Catan");
                }
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

        #endregion Methods

        #region Destructors

        #endregion Destructors

        #region Indexers

        #endregion Indexers

        #region Structs

        #endregion Structs

        #region Classes

        #endregion Classes

        #region Interfaces

        #endregion Interfaces

        public int AvailableDevCardIndex
        {
            get => (int)GetValue(AvailableDevCardIndexProperty);
            set => SetValue(AvailableDevCardIndexProperty, value);
        }

        public int NewDevCardsIndex
        {
            get => (int)GetValue(NewDevCardsIndexProperty);
            set => SetValue(NewDevCardsIndexProperty, value);
        }

        public int PlayedDevCardIndex
        {
            get => (int)GetValue(PlayedDevCardIndexProperty);
            set => SetValue(PlayedDevCardIndexProperty, value);
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

        public static readonly DependencyProperty AvailableDevCardIndexProperty = DependencyProperty.Register("AvailableDevCardIndex", typeof(int), typeof(PrivateDataCtrl), new PropertyMetadata(0));
        public static readonly DependencyProperty NewDevCardsIndexProperty = DependencyProperty.Register("NewDevCardsIndex", typeof(int), typeof(PrivateDataCtrl), new PropertyMetadata(0));

        public static readonly DependencyProperty PlayedDevCardIndexProperty = DependencyProperty.Register("PlayedDevCardIndex", typeof(int), typeof(PrivateDataCtrl), new PropertyMetadata(0));
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

        public string XOfYText(int index, int count)
        {
            int i = index + 1;
            return $"{i} of {count}";
        }
    }
}
