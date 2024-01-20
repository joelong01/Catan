using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using static Catan10.StaticHelpers;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public enum NumberColorTheme { Light, Dark };

    public enum NumberStyle { Default, ResoureCount };

    public sealed partial class CatanNumber : UserControl, INotifyPropertyChanged, IDragAndDropProgress
    {
        private readonly SolidColorBrush _blackBrush = CatanColors.GetResourceBrush("White", Colors.White);
        private readonly SolidColorBrush _redBrush = CatanColors.GetResourceBrush("Red", Colors.Red);
        private readonly SolidColorBrush _whiteBrush = CatanColors.GetResourceBrush("White", Colors.White);
        private bool _showEyes = false;
        private NumberColorTheme myTheme = NumberColorTheme.Light;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static readonly DependencyProperty SwappableProperty = DependencyProperty.Register("Swappable", typeof(bool), typeof(CatanNumber), new PropertyMetadata(false));
        public static readonly DependencyProperty MainPageModelProperty = DependencyProperty.Register("MainPageModel", typeof(MainPageModel), typeof(CatanNumber), new PropertyMetadata(MainPageModel.Default));
        public MainPageModel MainPageModel
        {
            get => ( MainPageModel )GetValue(MainPageModelProperty);
            set => SetValue(MainPageModelProperty, value);
        }
        public bool Swappable
        {
            get => ( bool )GetValue(SwappableProperty);
            set => SetValue(SwappableProperty, value);
        }

        private void SetEllipseColor(SolidColorBrush brush)
        {
            foreach (UIElement el in _oddGrid.Children)
            {
                if (el.GetType() == typeof(Ellipse))
                {
                    ( ( Ellipse )el ).Fill = brush;
                }
            }

            foreach (UIElement el in _evenGrid.Children)
            {
                if (el.GetType() == typeof(Ellipse))
                {
                    ( ( Ellipse )el ).Fill = brush;
                }
            }
        }

        private void UseOddGrid(bool useOdd)
        {
            if (useOdd)
            {
                _oddGrid.Visibility = Visibility.Visible;
                _evenGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                _oddGrid.Visibility = Visibility.Collapsed;
                _evenGrid.Visibility = Visibility.Visible;
            }
        }

        public bool HideSeven { get; set; } = true;

        public int Number
        {
            get => Convert.ToInt32(_txtNumber.Text);
            set
            {
                bool notifyChange = _txtNumber.Text != value.ToString();
                try
                {
                    _txtNumber.Text = value.ToString();

                    foreach (UIElement el in _oddGrid.Children)
                    {
                        el.Visibility = Visibility.Collapsed;
                        if (( ( FrameworkElement )el ).Name == _txtNumber.Name)
                        {
                            this.TraceMessage("Found your damn bug!");
                        }
                    }

                    foreach (UIElement el in _evenGrid.Children)
                    {
                        el.Visibility = Visibility.Collapsed;
                        if (( ( FrameworkElement )el ).Name == _txtNumber.Name)
                        {
                            this.TraceMessage("Found your damn bug!");
                        }
                    }

                    if (NumberStyle == NumberStyle.ResoureCount)
                    {
                        this.Visibility = Visibility.Visible;
                        // just take the number.  all the propability circles are hidden
                        return;
                    }

                    if (value == 6 || value == 8)
                    {
                        Probability = 5;
                        UseOddGrid(true);               // this might look funny since 8 and 6 aren't odd -- but the odds of getting them (5) is odd!
                        _txtNumber.Foreground = _redBrush;
                        SetEllipseColor(_redBrush);

                        foreach (UIElement el in _oddGrid.Children)
                        {
                            el.Visibility = Visibility.Visible;
                        }
                        Swappable = false;
                        return;
                    }

                    if (Theme == NumberColorTheme.Dark)
                    {
                        SetEllipseColor(_blackBrush);
                        _txtNumber.Foreground = _blackBrush;
                    }
                    else
                    {
                        SetEllipseColor(_whiteBrush);
                        _txtNumber.Foreground = _whiteBrush;
                    }

                    if (value == 2 || value == 12)
                    {
                        Swappable = false;
                        Probability = 1;
                        UseOddGrid(true);
                        _oddGrid.Children[2].Visibility = Visibility.Visible;
                    }

                    if (value == 3 || value == 11)
                    {
                        Swappable = true;
                        Probability = 2;
                        UseOddGrid(false);
                        _evenGrid.Children[1].Visibility = Visibility.Visible;
                        _evenGrid.Children[2].Visibility = Visibility.Visible;

                        if (_showEyes)
                        {
                            _rectLeftEye.Visibility = Visibility.Visible;
                            _rectRightEye.Visibility = Visibility.Visible;
                        }
                    }

                    if (value == 4 || value == 10)
                    {
                        Swappable = true;
                        Probability = 3;
                        UseOddGrid(true);
                        _oddGrid.Children[1].Visibility = Visibility.Visible;
                        _oddGrid.Children[2].Visibility = Visibility.Visible;
                        _oddGrid.Children[3].Visibility = Visibility.Visible;
                    }

                    if (value == 5 || value == 9)
                    {
                        Swappable = true;
                        Probability = 4;
                        UseOddGrid(false);
                        _evenGrid.Children[0].Visibility = Visibility.Visible;
                        _evenGrid.Children[1].Visibility = Visibility.Visible;
                        _evenGrid.Children[2].Visibility = Visibility.Visible;
                        _evenGrid.Children[3].Visibility = Visibility.Visible;
                    }

                    if (value == 0)
                    {
                        Swappable = false;
                        this.Visibility = Visibility.Collapsed;
                    }
                    if (value == 7 && HideSeven)
                    {
                        Swappable = false;
                        Probability = 6;
                        this.Visibility = Visibility.Collapsed;
                    }
                }
                catch (Exception)
                {
                }
                finally
                {

                    if (notifyChange)
                    {
                        NotifyPropertyChanged();
                    }
                }
            }
        }

        public NumberStyle NumberStyle { get; set; } = NumberStyle.Default;

        public int Probability { get; set; } = 0;

        public bool ShowEyes
        {
            get => _showEyes;
            set
            {
                _showEyes = value;
                _rectLeftEye.Visibility = Visibility.Collapsed;
                _rectRightEye.Visibility = Visibility.Collapsed;
                if (( Number == 3 || Number == 11 ) && _showEyes)
                {
                    _rectLeftEye.Visibility = Visibility.Visible;
                    _rectRightEye.Visibility = Visibility.Visible;
                }
            }
        }

        // the number (divided by 36) that represents the probability of this number being rolled
        public NumberColorTheme Theme
        {
            get => myTheme;
            set { myTheme = value; NotifyPropertyChanged(); }
        }

        public CatanNumber()
        {
            this.InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void MoveAsync(Point to)
        {
            DA_X.To = to.X;
            DA_Y.To = to.Y;
            AnimateMove.Begin();
        }
        private TileCtrl PreviousTile { get; set; } = null;
        private TileCtrl CurrentTile { get; set; } = null;
        private TileCtrl ParentTile { get; set; } = null;
        public FrameworkElement GetFirstParent(FrameworkElement start, Type stopElementType)
        {
            if (start == null || stopElementType == null)
            {
                throw new ArgumentNullException();
            }

            FrameworkElement parent = start;

            while (parent != null)
            {
                parent = ( FrameworkElement )VisualTreeHelper.GetParent(parent);

                if (parent != null && stopElementType.IsInstanceOfType(parent))
                {
                    return parent;
                }
            }

            return null; // Return null if no parent of the specified type is found
        }
        private async void OnPointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (!Swappable) return;


            var GameController = MainPage.Current as IGameController;
            if (!GameController.CurrentPlayer.GameData.Resources.HasEntitlement(Entitlement.Inventor)) return;


            ParentTile = GetFirstParent(( FrameworkElement )sender, typeof(TileCtrl)) as TileCtrl;


            var xform = ((Grid)sender).RenderTransform as CompositeTransform;

            Point StartPoint = new Point(xform.TranslateX, xform.TranslateY);  // need this to deal with PointUp

            // when dragging we stop highlighting any tile and when we are done, we put it back the way it was
            var highlightedTiles = new List<TileCtrl>();
            foreach (var tileIndex in GameController.HighlightedTiles)
            {
                var tile = GameController.TileFromIndex(tileIndex);
                highlightedTiles.Add(tile);
                tile.StopHighlightingTile();
            }
            Grid grid = sender as Grid;
            int numberZIndex = Canvas.GetZIndex(grid);
            int parentZIndex = Canvas.GetZIndex(grid);

            Canvas.SetZIndex(grid, numberZIndex + 1);
            Canvas.SetZIndex(ParentTile, parentZIndex + 1);

            await StaticHelpers.DragAsync(grid, e, this);

            Canvas.SetZIndex(grid, numberZIndex);
            Canvas.SetZIndex(ParentTile, parentZIndex);

            highlightedTiles.ForEach(t => t.HighlightTile());

            var endPoint = new Point (xform.TranslateX, xform.TranslateY);


            if (CurrentTile != null && CurrentTile.CatanNumber.Swappable && CurrentTile != ParentTile)
            {
                //  this.TraceMessage($"You want to swap {this.Number} for {CurrentTile.Number}");
                await InventorLog.PostLogMessage(MainPage.Current as IGameController, ParentTile, CurrentTile);
            }
            else
            {
                MoveAsync(new Point(0,0));
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

                if (element is TileCtrl) // Replace with your specific class
                {

                    var tile = (TileCtrl)element;
                    if (tile == ParentTile) continue;
                    if (tile != PreviousTile)
                    {

                        if (PreviousTile != null)
                        {
                            PreviousTile.StopHighlightingTile();
                        }
                        if (tile.CatanNumber.Swappable)
                        {
                            tile.HighlightTile();
                        }

                        PreviousTile = tile;
                        CurrentTile = tile;


                    }

                }
            }
        }
    }
}
