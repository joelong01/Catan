using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using static Catan10.StaticHelpers;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class MerchantCtrl : UserControl, IDragAndDropProgress
    {
        public bool EnableMove { get; set; } = false;
        private TileCtrl StartingTile { get; set; } = null;
        private TileCtrl PreviousTile { get; set; } = null;
        private TileCtrl CurrentTile { get; set; } = null;
        private Point StartPoint { get; set; }

        public MerchantCtrl()
        {
            this.InitializeComponent();
        }
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke", typeof(SolidColorBrush), typeof(MerchantCtrl), new PropertyMetadata(new SolidColorBrush(Colors.White)));

        public SolidColorBrush Stroke
        {
            get => ( SolidColorBrush )GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(double), typeof(MerchantCtrl), new PropertyMetadata(30.0));
        public static readonly DependencyProperty GameControllerProperty = DependencyProperty.Register("GameController", typeof(IGameController), typeof(MerchantCtrl), new PropertyMetadata(null));
        public IGameController GameController
        {
            get => ( IGameController )GetValue(GameControllerProperty);
            set => SetValue(GameControllerProperty, value);
        }
        public double StrokeThickness
        {
            get => ( double )GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        public void MoveAsync(Point to)
        {
            DA_X.To = to.X;
            DA_Y.To = to.Y;
            AnimateMove.Begin();
        }

        public void ShowAsync()
        {
            DA_Opacity.To = 1.0;
            SB_AnimateOpacity.Begin();
        }

        public void HideAsync()
        {
            DA_Opacity.To = 0.0;
            SB_AnimateOpacity.Begin();
        }

        private async void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!EnableMove) return; // this is off, for example, in the Purchase control

            var xform = ((Border)sender).RenderTransform as CompositeTransform;

            StartPoint = new Point(xform.TranslateX, xform.TranslateY);  // need this to deal with PointUp

            // when dragging we stop highlighting any tile and when we are done, we put it back the way it was
            var highlightedTiles = new List<TileCtrl>();
            foreach (var tileIndex in GameController.HighlightedTiles)
            {
                var tile = GameController.TileFromIndex(tileIndex);
                highlightedTiles.Add(tile);
                tile.StopHighlightingTile();
            }
            Border border = sender as Border;
            int zIndex = Canvas.GetZIndex(border);

            Canvas.SetZIndex(border, zIndex + 1000);

            await StaticHelpers.DragAsync(border, e, this);

            Canvas.SetZIndex(border, zIndex);

            highlightedTiles.ForEach(t => t.HighlightTile());

            var endPoint = new Point (xform.TranslateX, xform.TranslateY);

            if (PreviousTile != null)
            {
                PreviousTile.StopHighlightingTile();
            }
            if (CurrentTile != null)
            {
                CurrentTile.StopHighlightingTile();
            }
            //
            //  if they don't have the entitlement, let them move inside the same tile, but return
            //  to starting Tile if it is not where they started
            if (!GameController.CurrentPlayer.GameData.Resources.HasEntitlement(Entitlement.Merchant))
            {
                if (CurrentTile != StartingTile)
                {
                    MoveAsync(StartPoint); // this action is not logged
                    PreviousTile = StartingTile;
                    StartingTile = CurrentTile;
                }
            }
            else
            {
                //  log the change we made
                await MoveMerchantLog.PostLogMessage(GameController, StartPoint, endPoint);
                PreviousTile = CurrentTile;
                StartingTile = CurrentTile;
            }

        }

        /// <summary>
        ///     looks for the TileCtrl underneath the moust and then highlights the tile that the control is moving through.
        ///     does booking to keep track of the tiles the control is dragged through
        /// </summary>
        /// <param name="e"></param>
        /// <param name="mousePosition"></param>

        public void Report(PointerRoutedEventArgs e, Point mousePosition)
        {
            //    this.TraceMessage($"Point {mousePosition}");

            Point mousePositionRelativeToMainPage = e.GetCurrentPoint(MainPage.Current).Position;

            var elementsUnderMouse = VisualTreeHelper.FindElementsInHostCoordinates(mousePositionRelativeToMainPage, MainPage.Current);

            foreach (var element in elementsUnderMouse)
            {
                //    this.TraceMessage($"found {element.GetType().Name}");
                if (element is TileCtrl) // Replace with your specific class
                {

                    var tile = (TileCtrl)element;
                    if (tile != PreviousTile)
                    {
                        if (PreviousTile != null)
                        {
                            PreviousTile.StopHighlightingTile();
                        }
                        if (tile != null)
                        {
                            PreviousTile = tile;
                            CurrentTile = tile;
                            tile.HighlightTile();
                        }
                    }
                    break;
                }
            }
        }
    }
}
