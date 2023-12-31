using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class TakeCardDlg : ContentDialog
    {
        #region Delegates + Fields + Events + Enums

        public static readonly DependencyProperty CountVisibleProperty = DependencyProperty.Register("CountVisible", typeof(bool), typeof(TakeCardDlg), new PropertyMetadata(true, CountVisibleChanged));
        public static readonly DependencyProperty DestinationProperty = DependencyProperty.Register("Destination", typeof(ObservableCollection<ResourceCardModel>), typeof(TakeCardDlg), new PropertyMetadata(null));

        public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(PlayerModel), typeof(TakeCardDlg), new PropertyMetadata(PlayerModel.DefaultPlayer));

        public static readonly DependencyProperty HowManyProperty = DependencyProperty.Register("HowMany", typeof(int), typeof(TakeCardDlg), new PropertyMetadata(1, HowManyChanged));

        public static readonly DependencyProperty InstructionsProperty = DependencyProperty.Register("Instructions", typeof(string), typeof(TakeCardDlg), new PropertyMetadata(""));

        public static readonly DependencyProperty SourceOrientationProperty = DependencyProperty.Register("SourceOrientation", typeof(TileOrientation), typeof(TakeCardDlg), new PropertyMetadata(TileOrientation.FaceDown));

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(ObservableCollection<ResourceCardModel>), typeof(TakeCardDlg), new PropertyMetadata(null));

        public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(PlayerModel), typeof(TakeCardDlg), new PropertyMetadata(PlayerModel.DefaultPlayer));
        public static readonly DependencyProperty ConsolidateCardsProperty = DependencyProperty.Register("ConsolidateCards", typeof(bool), typeof(TakeCardDlg), new PropertyMetadata(true, ConsolidateCardsChanged));
        public bool ConsolidateCards
        {
            get => (bool)GetValue(ConsolidateCardsProperty);
            set => SetValue(ConsolidateCardsProperty, value);
        }
        private static void ConsolidateCardsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as TakeCardDlg;
            var depPropValue = (bool)e.NewValue;
            depPropClass?.SetConsolidateCards(depPropValue);
        }
        private void SetConsolidateCards(bool consolidate)
        {
            if (consolidate)
            {
                Group(Source);
                Group(Destination);
            }
            else
            {
                Flatten(Source);
                Flatten(Destination);
            }
        }


        public bool CountVisible
        {
            get => (bool)GetValue(CountVisibleProperty);
            set => SetValue(CountVisibleProperty, value);
        }

        private static void CountVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as TakeCardDlg;
            var depPropValue = (bool)e.NewValue;
            depPropClass?.SetCountVisible(depPropValue);
        }

        private void SetCountVisible(bool value)
        {
            foreach (var c in Source)
            {
                c.CountVisible = value;
            }
        }

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public ObservableCollection<ResourceCardModel> Destination
        {
            get => (ObservableCollection<ResourceCardModel>)GetValue(DestinationProperty);
            set => SetValue(DestinationProperty, value);
        }

        public PlayerModel From
        {
            get => (PlayerModel)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        public int HowMany
        {
            get => (int)GetValue(HowManyProperty);
            set => SetValue(HowManyProperty, value);
        }

        public string Instructions
        {
            get => (string)GetValue(InstructionsProperty);
            set => SetValue(InstructionsProperty, value);
        }

        public ObservableCollection<ResourceCardModel> Source
        {
            get => (ObservableCollection<ResourceCardModel>)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public TileOrientation SourceOrientation
        {
            get => (TileOrientation)GetValue(SourceOrientationProperty);
            set => SetValue(SourceOrientationProperty, value);
        }

        public PlayerModel To
        {
            get => (PlayerModel)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        #endregion Properties

        #region Constructors + Destructors

        public TakeCardDlg()
        {
            this.InitializeComponent();
        }

        #endregion Constructors + Destructors

        #region Methods

        private static void HowManyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as TakeCardDlg;
            var depPropValue = (int)e.NewValue;
            depPropClass?.SetHowMany(depPropValue);
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (Destination.Count != HowMany)
            {
                args.Cancel = true;
            }
            else
            {
                if (Destination.Count == 1 && Destination[0].Orientation == TileOrientation.FaceDown)
                {
                    Destination[0].Orientation = TileOrientation.FaceUp;
                    await Task.Delay(1000); // give use 1 seconds to feel good about the card they got
                }
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private bool EnableOkButton(int count)
        {
            return count == HowMany;
        }

        private void GridView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            var source = Source;
            var gridView = sender as GridView;

            if (gridView.Name == "GridView_Destination")
            {
                source = Destination;
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

        //private void DoDropByCount(IEnumerable<ResourceCardModel> movedCards, ObservableCollection<ResourceCardModel> sourceCards, ObservableCollection<ResourceCardModel> target)
        //{
        //    foreach (var card in movedCards)
        //    {
        //        if (card.Count != 0) // ignore an attempt to drop a card with 0 resources
        //        {
        //            if (ResourceModelCollectionCount(target) + movedCards.Count > HowMany)
        //            {

        //            }
        //            card.Count--;
        //            card.CountVisible = true;
        //            var targetCard = FindCard(Destination, card.ResourceType);
        //            if (targetCard == null)
        //            {
        //                targetCard = new ResourceCardModel()
        //                {
        //                    ResourceType = card.ResourceType,
        //                    Count = 0,
        //                    CountVisible = true,
        //                    Orientation = this.SourceOrientation
        //                };

        //                target.Add(targetCard);
        //            }
        //            targetCard.Count++;


        //        }
        //    }

        //}

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null)
            {
                this.TraceMessage("Drop will null data");
                return;
            }

            var target = Source;
            var gridView = sender as GridView;

            if (gridView.Name == "GridView_Destination")
            {
                target = Destination;
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
                            Orientation = this.SourceOrientation
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
            if (gridView.Name == "GridView_Destination")
            {
                while (ResourceModelCollectionCount(target) > HowMany)
                {
                    var removed = target[0];
                    target.RemoveAt(0);
                    if (CountVisible)
                    {

                        var sourceCard = FindCard(sourceCards, removed.ResourceType);
                        if (sourceCard != null)
                        {
                            sourceCard.Count++;
                            sourceCard.CountVisible = CountVisible;
                        }

                    }
                    else
                    {
                        sourceCards.Add(removed);
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

        private void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            foreach (var card in Source)
            {
                card.Orientation = SourceOrientation;
            }
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

        #endregion Methods

        private void OnFlatten(object sender, RoutedEventArgs e)
        {
            Flatten(Source);
            Flatten(Destination);
        }

        private void Flatten(ObservableCollection<ResourceCardModel> list)
        {
            List<ResourceCardModel> newList =  new List<ResourceCardModel>();

            foreach (var card in list)
            {
                for (int i = 0; i < card.Count; i++)
                {
                    newList.Add(new ResourceCardModel()
                    {
                        ResourceType = card.ResourceType,
                        Count = 1,
                        CountVisible = false,
                        Orientation = TileOrientation.FaceUp
                    });
                }
            }

            list.Clear();
            list.AddRange(newList);
            CountVisible = false;
        }

        private void Group(ObservableCollection<ResourceCardModel> list)
        {
            TradeResources tr = new TradeResources();
            foreach (var card in list)
            {
                tr.AddResource(card.ResourceType, card.Count);
            }

            var rcc = tr.ToResourceCardCollection();
            list.Clear();
            for (int i= rcc.Count - 1; i>=0; i--)
            {                
                if (rcc[i].Count != 0)
                {
                    rcc[i].Orientation = TileOrientation.FaceUp;
                    list.Add(rcc[i]);
                }

            }

        }

        private void OnGroup(object sender, RoutedEventArgs e)
        {
            Group(Source);
            Group(Destination);
            

        }
    }
}