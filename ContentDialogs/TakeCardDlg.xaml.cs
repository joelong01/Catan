using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class TakeCardDlg : ContentDialog
    {
        private static void HowManyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as TakeCardDlg;
            var depPropValue = (int)e.NewValue;
            depPropClass?.SetHowMany(depPropValue);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (Destination.Count != HowMany)
            {
                args.Cancel = true;
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
                bool ret = sourceCards.Remove(card);
                if (!ret)
                {
                    throw new ArgumentException("A card to be moved wasn't in the source collection.");
                }
                target.Add(card);
                //
                //  if you pull down a card that is more than you deserve, put the first one back into the source
                if (target.Count > HowMany)
                {
                    var removed = target[0];
                    target.RemoveAt(0);
                    sourceCards.Add(removed);
                }
            }
            e.Handled = true;
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

        public static readonly DependencyProperty DestinationProperty = DependencyProperty.Register("Destination", typeof(ObservableCollection<ResourceCardModel>), typeof(TakeCardDlg), new PropertyMetadata(null));
        public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(PlayerModel), typeof(TakeCardDlg), new PropertyMetadata(null));
        public static readonly DependencyProperty HowManyProperty = DependencyProperty.Register("HowMany", typeof(int), typeof(TakeCardDlg), new PropertyMetadata(1, HowManyChanged));
        public static readonly DependencyProperty InstructionsProperty = DependencyProperty.Register("Instructions", typeof(string), typeof(TakeCardDlg), new PropertyMetadata(""));
        public static readonly DependencyProperty SourceOrientationProperty = DependencyProperty.Register("SourceOrientation", typeof(TileOrientation), typeof(TakeCardDlg), new PropertyMetadata(TileOrientation.FaceDown));
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(ObservableCollection<ResourceCardModel>), typeof(TakeCardDlg), new PropertyMetadata(null));
        public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(PlayerModel), typeof(TakeCardDlg), new PropertyMetadata(null));

        public TakeCardDlg()
        {
            this.InitializeComponent();
        }
    }
}
