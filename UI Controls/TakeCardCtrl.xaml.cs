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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class TakeCardCtrl : UserControl
    {
        private static void FromBankChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as TakeCardCtrl;
            var depPropValue = (bool)e.NewValue;
            depPropClass?.SetFromBank(depPropValue);
        }

        private static void HowManyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as TakeCardCtrl;
            var depPropValue = (int)e.NewValue;
            depPropClass?.SetHowMany(depPropValue);
        }

        private bool EnableOkButton(int howMany)
        {
            return howMany == HowMany;
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

        private void OnCancel(object sender, RoutedEventArgs e)
        {
            if (TCS != null && TCS.Task.IsCompleted == false)
            {
                TCS.SetResult(false);
            }
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
                if (target.Count >= HowMany)
                {
                    var removed = target[0];
                    target.RemoveAt(0);
                    sourceCards.Add(removed);
                }
            }
            e.Handled = true;
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            if (TCS != null && TCS.Task.IsCompleted == false)
            {
                Destination.ForEach((c) => c.Orientation = TileOrientation.FaceUp);
                TCS.SetResult(true);
                // await Task.Delay(2000).ContinueWith((t) => TCS.SetResult(true));
            }
        }

        private void SetFromBank(bool value)
        {
            if (value && Source != null)
            {
                Source.ForEach((c) => c.Orientation = TileOrientation.FaceUp);
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

        private TaskCompletionSource<bool> TCS { get; set; } = new TaskCompletionSource<bool>();

        /// <summary>
        ///     Set data and then call GetCards
        /// </summary>
        /// <returns></returns>
        public async Task<(bool, TradeResources)> GetCards()
        {
            this.Visibility = Visibility.Visible;
            if (FaceUp && Source != null)
            {
                Source.ForEach((c) => c.Orientation = TileOrientation.FaceUp);
            }

            TCS = new TaskCompletionSource<bool>();
            var ret = await TCS.Task;

            TradeResources tr = ResourceCardCollection.ToTradeResources(Destination);
            return (ret, tr);
        }

        public TakeCardCtrl()
        {
            this.InitializeComponent();
        }

        public ObservableCollection<ResourceCardModel> Destination
        {
            get => (ObservableCollection<ResourceCardModel>)GetValue(DestinationProperty);
            set => SetValue(DestinationProperty, value);
        }

        public bool FaceUp
        {
            get => (bool)GetValue(FromBankProperty);
            set => SetValue(FromBankProperty, value);
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

        public ObservableCollection<ResourceCardModel> Source
        {
            get => (ObservableCollection<ResourceCardModel>)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public PlayerModel To
        {
            get => (PlayerModel)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        public static readonly DependencyProperty DestinationProperty = DependencyProperty.Register("Destination", typeof(ObservableCollection<ResourceCardModel>), typeof(TakeCardCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty FromBankProperty = DependencyProperty.Register("FromBank", typeof(bool), typeof(TakeCardCtrl), new PropertyMetadata(false, FromBankChanged));
        public static readonly DependencyProperty FromProperty = DependencyProperty.Register("From", typeof(PlayerModel), typeof(TakeCardCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty HowManyProperty = DependencyProperty.Register("HowMany", typeof(int), typeof(TakeCardCtrl), new PropertyMetadata(1, HowManyChanged));
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(ObservableCollection<ResourceCardModel>), typeof(TakeCardCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty ToProperty = DependencyProperty.Register("To", typeof(PlayerModel), typeof(TakeCardCtrl), new PropertyMetadata(null));
    }
}
