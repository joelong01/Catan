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
            if (!EnableMove) return;

            if (!GameController.CurrentPlayer.GameData.Resources.HasEntitlement(Entitlement.Merchant))
            {
                this.TraceMessage($"{GameController.CurrentPlayer.PlayerName} does not have Entitlement.Merchant");
                return;
            }
            var xform = ((Border)sender).RenderTransform as CompositeTransform;

            Point startPoint = new Point (xform.TranslateX, xform.TranslateY);

            var highlightedTiles = new List<TileCtrl>();
            foreach (var tileIndex in GameController.HighlightedTiles)
            {
                highlightedTiles.Add(GameController.TileFromIndex(tileIndex));
            }
            Border border = sender as Border;
            int zIndex = Canvas.GetZIndex(border);

            Canvas.SetZIndex(border, zIndex + 1000);

            await StaticHelpers.DragAsync(border, e, this);

            Canvas.SetZIndex(border, zIndex);

            highlightedTiles.ForEach(t => t.HighlightTile());

            var endPoint = new Point (xform.TranslateX, xform.TranslateY);
          
            //  log the change we made
            await MoveMerchantLog.PostLogMessage(GameController, startPoint, endPoint);
        }


        public void PointerUp(Point value)
        {
            if (_previousTile != null)
            {
                _previousTile.StopHighlightingTile();
            }
            if (_currentTile != null)
            {
                _currentTile.StopHighlightingTile();
            }
            _previousTile = null;
        }
        TileCtrl _previousTile = null;
        TileCtrl _currentTile = null;
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
                    if (tile != _previousTile)
                    {
                        if (_previousTile != null)
                        {
                            _previousTile.StopHighlightingTile();
                        }
                        if (tile != null)
                        {
                            _previousTile = tile;
                            _currentTile = tile;
                            tile.HighlightTile();
                        }
                    }
                    break;
                }
            }
        }
    }
}
