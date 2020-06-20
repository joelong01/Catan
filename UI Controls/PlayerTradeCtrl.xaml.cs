using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public sealed partial class PlayerTradeCtrl : UserControl
    {
        public PlayerTradeCtrl()
        {
            this.InitializeComponent();
        }
        public PlayerTradeCtrl(PlayerModel currentPlayer, PlayerModel tradePartner)
        {
            this.InitializeComponent();
            CurrentPlayer = currentPlayer;
            TradePartner = tradePartner;

            PlayerResources.AddRange(ResourceCardCollection.Flatten(currentPlayer.GameData.Resources.Current));
            PartnerResources.AddRange(ResourceCardCollection.Flatten(TradePartner.GameData.Resources.Current));

            PlayerResources.ForEach((c) => c.Orientation = TileOrientation.FaceUp);
            PartnerResources.ForEach((c) => c.Orientation = TileOrientation.FaceDown);

        }

        ResourceCardCollection PlayerResources { get; set; } = new ResourceCardCollection(false);
        ResourceCardCollection PartnerResources { get; set; } = new ResourceCardCollection(false);
        ResourceCardCollection PartnerTradeResources { get; set; } = new ResourceCardCollection(false);
        ResourceCardCollection PlayerTradeResources { get; set; } = new ResourceCardCollection(false);


        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(PlayerTradeCtrl), new PropertyMetadata(null, CurrentPlayerChanged));
        public static readonly DependencyProperty TradePartnerProperty = DependencyProperty.Register("TradePartner", typeof(PlayerModel), typeof(PlayerTradeCtrl), new PropertyMetadata(null, TradePartnerChanged));
        public static readonly DependencyProperty CountVisibleProperty = DependencyProperty.Register("CountVisible", typeof(bool), typeof(PlayerTradeCtrl), new PropertyMetadata(true, CountVisibleChanged));
        public static readonly DependencyProperty PlayingPlayersProperty = DependencyProperty.Register("PlayingPlayers", typeof(ObservableCollection<PlayerModel>), typeof(PlayerTradeCtrl), new PropertyMetadata(null));
       
        public ObservableCollection<PlayerModel> PlayingPlayers
        {
            get => (ObservableCollection<PlayerModel>)GetValue(PlayingPlayersProperty);
            set => SetValue(PlayingPlayersProperty, value);
        }
        public bool CountVisible
        {
            get => (bool)GetValue(CountVisibleProperty);
            set => SetValue(CountVisibleProperty, value);
        }
        private static void CountVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PlayerTradeCtrl;
            var depPropValue = (bool)e.NewValue;
            depPropClass?.SetCountVisible(depPropValue);
        }

        public PlayerModel TradePartner
        {
            get => (PlayerModel)GetValue(TradePartnerProperty);
            set => SetValue(TradePartnerProperty, value);
        }
        private static void TradePartnerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PlayerTradeCtrl;
            var depPropValue = (PlayerModel)e.NewValue;
            depPropClass?.SetTradePartner(depPropValue);
        }
        private void SetTradePartner(PlayerModel tradePartner)
        {
            PartnerResources.Clear();
            PartnerResources.AddRange(ResourceCardCollection.Flatten(tradePartner.GameData.Resources.Current));
            PartnerResources.AllUp();
            PartnerTradeResources.Clear();

        }

        public PlayerModel CurrentPlayer
        {
            get => (PlayerModel)GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }
        private static void CurrentPlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PlayerTradeCtrl;
            var depPropValue = (PlayerModel)e.NewValue;
            depPropClass?.SetCurrentPlayer(depPropValue);
        }
        private void SetCurrentPlayer(PlayerModel newPlayer)
        {
            newPlayer.GameData.Resources.PropertyChanged += Resources_PropertyChanged;
            newPlayer.GameData.Resources.Current.PropertyChanged += OnCurrentResourcesChanged;
            UpdatePlayerResources(newPlayer.GameData.Resources.Current);


        }
        int count;
        private void Resources_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Current")
            {
                count++;
                if (count == 100) { this.TraceMessage("now"); }
                PlayerResources resources = sender as PlayerResources;
                UpdatePlayerResources(resources.Current);
                resources.PropertyChanged += Resources_PropertyChanged;
            }
        }

        private void UpdatePlayerResources(TradeResources resources)
        {

            PlayerResources.Clear();
            PlayerResources.AddResources(resources);
            PlayerResources.AllUp();
        }

        private void OnCurrentResourcesChanged(object sender, PropertyChangedEventArgs e)
        {
            TradeResources resources = sender as TradeResources;
            UpdatePlayerResources(resources);

        }

        private void SetCountVisible(bool value)
        {
            if (value)
            {
                PlayerResources = PlayerResources.Consolidate();
            }
            else
            {
                PlayerResources = PlayerResources.Flatten();

            }
            PlayerResources.ForEach((c) => c.CountVisible = value);
            PlayerResources.AllUp();
        }


        private ResourceCardModel FindCard(ICollection<ResourceCardModel> list, ResourceType resourceType)
        {
            foreach (var card in list)
            {
                if (card.ResourceType == resourceType)
                {
                    return card;
                }
            }
            return null;
        }

        public ObservableCollection<PlayerModel> ExceptMe(ObservableCollection<PlayerModel> list)
        {
            var ret = new ObservableCollection<PlayerModel>();
            ret.AddRange(list);
            ret.Remove(MainPage.Current.TheHuman);
            return ret;
        }

        private void GridView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            var source = PlayerResources;
            var gridView = sender as GridView;

            if (gridView.Name == "GridView_PlayerTrade")
            {
                source = PlayerTradeResources;
            }
            List<ResourceCardModel> movedCards = new List<ResourceCardModel>();

            foreach (ResourceCardModel p in e.Items)
            {
                p.CountVisible = false;
                movedCards.Add(p);
            }
            if (movedCards.Count == 0) return;

            e.Data.Properties.Add("movedCards", movedCards);
            e.Data.Properties.Add("source", source);
        }

        private void OnDrageEnter(object target, DragEventArgs e)
        {
            SetThickness(target, 3);
        }

        private void OnDragLeave(object sender, DragEventArgs e)
        {
            SetThickness(sender, 1);
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
            e.DragUIOverride.IsGlyphVisible = false;
            e.DragUIOverride.IsCaptionVisible = false;
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null)
            {
                this.TraceMessage("Drop with null data");
                return;
            }

            var target = PlayerTradeResources;
            var gridView = sender as GridView;

            if (gridView.Name == "GridView_Player")
            {
                target = PlayerResources;
            }

            var source = e.Data.Properties["source"];
            if (source == target)
            {
                e.Handled = false;
                return;
            }
            IEnumerable<ResourceCardModel> movedCards = e.Data.Properties["movedCards"] as IEnumerable<ResourceCardModel>;
            ObservableCollection<ResourceCardModel> sourceCards = e.Data.Properties["source"] as ObservableCollection<ResourceCardModel>;
            foreach (var card in movedCards)
            {
                card.CountVisible = true;

                if (card.Count != 0)
                {
                    card.Count--;

                    if (card.Count == 0)
                    {
                        sourceCards.Remove(card);
                    }
                    bool found = false;
                    foreach (var c in target)
                    {
                        if (c.ResourceType == card.ResourceType)
                        {
                            c.Count++;
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        ResourceCardModel newCard = new ResourceCardModel()
                        {
                            ResourceType = card.ResourceType,
                            Count = 1,
                            CountVisible = true,
                            Orientation = TileOrientation.FaceUp
                        };
                        target.Add(newCard);
                    }
                }

            }

            e.Handled = true;
        }



        private int ResourceModelCollectionCount(ICollection<ResourceCardModel> list)
        {
            int count = 0;
            foreach (var card in list)
            {
                count += card.Count;
            }
            return count;
        }
        private void SetHowMany(int value)
        {
        }

        private void SetThickness(object target, double thickness)
        {
            if (target.GetType() == typeof(Grid))
            {
                ((Grid)target).BorderThickness = new Thickness(thickness);
            }
            else if (target.GetType() == typeof(GridView))
            {
                ((GridView)target).BorderThickness = new Thickness(thickness);
            }
        }

        private void OnApprove(object sender, RoutedEventArgs e)
        {

        }

        private void OnPlayerPressed(object sender, PointerRoutedEventArgs e)
        {
            Ellipse ellipse = sender as Ellipse;
            TradePartner = (PlayerModel)ellipse.Tag;

        }
    }
}
