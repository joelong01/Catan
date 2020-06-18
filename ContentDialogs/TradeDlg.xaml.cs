using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Web.AtomPub;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class TradeDlg : ContentDialog
    {
     

        public TradeDlg(PlayerModel currentPlayer, PlayerModel tradePartner)
        {
            this.InitializeComponent();
            CurrentPlayer = currentPlayer;
            TradePartner = tradePartner;

            PlayerResources.AddRange(ResourceCardCollection.Flatten(currentPlayer.GameData.Resources.Current));
            PartnerResources.AddRange(ResourceCardCollection.Flatten(TradePartner.GameData.Resources.Current));

        }

        ObservableCollection<ResourceCardModel> PlayerResources { get; set; } = new ObservableCollection<ResourceCardModel>();
        ObservableCollection<ResourceCardModel> PartnerResources { get; set; } = new ObservableCollection<ResourceCardModel>();
        ObservableCollection<ResourceCardModel> PartnerTradeResources { get; set; } = new ObservableCollection<ResourceCardModel>();
        ObservableCollection<ResourceCardModel> PlayerTradeResources { get; set; } = new ObservableCollection<ResourceCardModel>();

        public static readonly DependencyProperty CountVisibleProperty = DependencyProperty.Register("CountVisible", typeof(bool), typeof(TradeDlg), new PropertyMetadata(true, CountVisibleChanged));        
        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(TradeDlg), new PropertyMetadata(null));
        public static readonly DependencyProperty TradePartnerProperty = DependencyProperty.Register("TradePartner", typeof(PlayerModel), typeof(TradeDlg), new PropertyMetadata(null));
        public bool CountVisible
        {
            get => (bool)GetValue(CountVisibleProperty);
            set => SetValue(CountVisibleProperty, value);
        }

        private static void CountVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as TradeDlg;
            var depPropValue = (bool)e.NewValue;
            depPropClass?.SetCountVisible(depPropValue);
        }

        private void SetCountVisible(bool value)
        {
            
        }

       

       

        public PlayerModel CurrentPlayer
        {
            get => (PlayerModel)GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }

        public PlayerModel TradePartner
        {
            get => (PlayerModel)GetValue(TradePartnerProperty);
            set => SetValue(TradePartnerProperty, value);
        }
     

        private static void HowManyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as TradeDlg;
            var depPropValue = (int)e.NewValue;
            depPropClass?.SetHowMany(depPropValue);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
           
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
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

        private void GridView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            var source = PlayerResources;
            var gridView = sender as GridView;

            if (gridView.Name == "GridView_PartnerResources")
            {
                source = PartnerResources;
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
                this.TraceMessage("Drop will null data");
                return;
            }

            var target = PlayerResources;
            var gridView = sender as GridView;

            if (gridView.Name == "GridView_PartnerResources")
            {
                target = PartnerResources;
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
                if (CountVisible)
                {
                    if (card.Count != 0)
                    {
                        card.Count--;
                        card.CountVisible = false;
                        ResourceCardModel newCard = new ResourceCardModel()
                        {
                            ResourceType = card.ResourceType,
                            Count = 1,
                            CountVisible = false,
                            Orientation = TileOrientation.FaceUp
                        };
                        target.Add(newCard);
                    }
                }
                else
                {
                    bool ret = sourceCards.Remove(card);

                    if (!ret)
                    {
                        throw new ArgumentException("A card to be moved wasn't in the source collection.");
                    }

                    target.Add(card);
                }
                //
                //  if you pull down a card that is more than you deserve, put the first one back into the source              
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
    }
}
