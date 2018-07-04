using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class RoadCtrl : UserControl
    {
        Dictionary<RoadLocation, RoadLocationData> _locationToRoadDataDict = new Dictionary<RoadLocation, RoadLocationData>();
        SolidColorBrush _brush = new SolidColorBrush(Colors.Black);
        SolidColorBrush _hightlightColor = new SolidColorBrush(Colors.Blue);        
        public List<BuildingCtrl> AdjacentBuildings { get; } = new List<BuildingCtrl>();
     
        public List<RoadKey> Keys { get; set; } = new List<RoadKey>();
        public IGameCallback Callback { get; internal set; }
        public int Number { get; internal set; } = 0; // number of roads that have been created for this player
        public List<RoadCtrl> AdjacentRoads { get; } = new List<RoadCtrl>();
        Ship _ship = null;

    
        public PlayerData Owner { get; set; } = null;
        public int Index { get; set; } = -1;

        public RoadCtrl()
        {
            this.InitializeComponent();

            _locationToRoadDataDict[RoadLocation.None] = new RoadLocationData(0, 0, 0);
            _locationToRoadDataDict[RoadLocation.Top] = new RoadLocationData(28, -6.5, 0);
            _locationToRoadDataDict[RoadLocation.TopRight] = new RoadLocationData(69, 17.4, 60.5);
            _locationToRoadDataDict[RoadLocation.BottomRight] = new RoadLocationData(69.6, 65.9, 119.5);
            _locationToRoadDataDict[RoadLocation.Bottom] = new RoadLocationData(28, 90, 180);
            _locationToRoadDataDict[RoadLocation.BottomLeft] = new RoadLocationData(-13.7, 66.8, 240.5);
            _locationToRoadDataDict[RoadLocation.TopLeft] = new RoadLocationData(-13.2, 18.2, -60.5);

        }

        
        public static readonly DependencyProperty LocationProperty = DependencyProperty.Register("Location", typeof(RoadLocation), typeof(RoadCtrl), new PropertyMetadata(RoadLocation.None, LocationChanged));
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Background", typeof(Color), typeof(RoadCtrl), new PropertyMetadata(Colors.Black, ColorChanged));
        public static readonly DependencyProperty TileZeroZeroProperty = DependencyProperty.Register("TileZeroZero", typeof(Point), typeof(RoadCtrl), new PropertyMetadata(new Point(double.NaN, double.NaN), TileZeroZeroChanged));
        public static readonly DependencyProperty RoadTypeProperty = DependencyProperty.Register("RoadType", typeof(RoadType), typeof(RoadCtrl), new PropertyMetadata(RoadType.Single, RoadTypeChanged));
        public static readonly DependencyProperty RoadStateProperty = DependencyProperty.Register("RoadState", typeof(RoadState), typeof(RoadCtrl), new PropertyMetadata(RoadState.Unowned, RoadStateChanged));
        public RoadState RoadState
        {
            get { return (RoadState)GetValue(RoadStateProperty); }
            set { SetValue(RoadStateProperty, value); }
        }
        private static void RoadStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RoadCtrl depPropClass = d as RoadCtrl;
            RoadState depPropValue = (RoadState)e.NewValue;
            depPropClass.SetRoadState(depPropValue);
        }

        //public RoadState RoadState
        //{
        //    get
        //    {
        //        return _roadState;
        //    }
        //    set
        //    {
        //        _roadState = value;

        //        //
        //        //  do not set the color to Transparent as that will make the build color wrong
        //        if (value == RoadState.Unowned)
        //            Show(false);
        //        else
        //            Show(true);

        //        this.Background = Background;

        //    }
        //}

            //
            //  the controls Opacity is what controls the user seeing a build shape
            //  so we can set the visibility of the grids inside the cotnrol
        private void SetRoadState(RoadState roadstate)
        {
            SetColors(_gridRoads);
            switch (roadstate)
            {
                case RoadState.Unowned:
                    _gridRoads.Visibility = Visibility.Visible;
                    _gridShip.Visibility = Visibility.Collapsed;
                    Show(false);
                    break;
                case RoadState.Road:
                    _gridRoads.Visibility = Visibility.Visible;
                    _gridShip.Visibility = Visibility.Collapsed;                 
                    Show(true);
                    break;
                case RoadState.Ship:
                    _gridRoads.Visibility = Visibility.Collapsed;
                    _gridShip.Visibility = Visibility.Visible;
                    if (_ship == null)
                    {
                        _ship = new Ship
                        {
                            Margin = new Thickness(2)
                        };
                        _gridShip.Children.Add(_ship);
                    }
                    Show(true);
                    break;
                default:
                    break;
            }

            UpdateVisuals(new Windows.Foundation.Size(this.ActualWidth, this.ActualHeight));
            
        }

        public RoadType RoadType
        {
            get { return (RoadType)GetValue(RoadTypeProperty); }
            set { SetValue(RoadTypeProperty, value); }
        }
        private static void RoadTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RoadCtrl depPropClass = d as RoadCtrl;
            RoadType depPropValue = (RoadType)e.NewValue;
            depPropClass.SetRoadType(depPropValue);
        }
        private void SetRoadType(RoadType roadtype)
        {
            if (this.ActualHeight * this.ActualWidth == 0) return;

            Size size = new Size(this.Width, this.Height);
           
            UpdateVisuals(size);
        }

        public Point TileZeroZero
        {
            get { return (Point)GetValue(TileZeroZeroProperty); }
            set { SetValue(TileZeroZeroProperty, value); }
        }
        private static void TileZeroZeroChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RoadCtrl depPropClass = d as RoadCtrl;
            Point depPropValue = (Point)e.NewValue;
            depPropClass.SetTileZeroZero(depPropValue);
        }
        private void SetTileZeroZero(Point point)
        {
            //_transform.TranslateX = point.X + _locationToRoadDataDict[Location].Left;
            //_transform.TranslateY = point.Y + _locationToRoadDataDict[Location].Top;

            RoadLocationData data = _locationToRoadDataDict[Location];

            _daToX.To = point.X + data.Left;
            _daToY.To = point.Y + data.Top;
            _sbMove.Begin();

            _daRotate.To = data.Angle;
            _sbRotate.Begin();

            _daAnimateOpacity.To = 0;
            _sbAnimateOpacity.Begin();

        }


        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }
        private static void ColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RoadCtrl depPropClass = d as RoadCtrl;
            Color depPropValue = (Color)e.NewValue;
            depPropClass.SetColor(depPropValue);
        }
        private void SetColor(Color color)
        {
            
            SetColors(_gridRoads);
        }

        public RoadLocation Location
        {
            get { return (RoadLocation)GetValue(LocationProperty); }
            set { SetValue(LocationProperty, value); }
        }
        private static void LocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RoadCtrl depPropClass = d as RoadCtrl;
            RoadLocation depPropValue = (RoadLocation)e.NewValue;
            depPropClass.SetLocation(depPropValue);
        }
        private void SetLocation(RoadLocation roadlocation)
        {
            RoadLocationData data = _locationToRoadDataDict[roadlocation];
            _transform.TranslateX = data.Left;
            _transform.TranslateY = data.Top;
            _transform.Rotation = data.Angle;
        }

        private void SetColors(Grid grid)
        {
            foreach (UIElement el in grid.Children)
            {
                if (el.GetType() == typeof(Polygon))
                {
                    ((Polygon)el).Stroke = MainPage.Current.CurrentPlayer.GameData.Background;
                    ((Polygon)el).Fill = MainPage.Current.CurrentPlayer?.GameData.Background;

                }
                if (el.GetType() == typeof(Line))
                {
                    ((Line)el).Stroke = MainPage.Current.CurrentPlayer?.GameData.Foreground;
                }
            }

            
            _shipPolygon.Fill = MainPage.Current.CurrentPlayer?.GameData.Background;
            _shipPolygon.Stroke = MainPage.Current.CurrentPlayer?.GameData.Foreground;
        
        }

      


        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {

            UpdateVisuals(e.NewSize);

        }

        private void UpdateVisuals(Size newSize)
        {

            double thirtyDegrees = Math.PI / 180.0 * 30;
            double triangleHeight = Math.Tan(thirtyDegrees) * (newSize.Height) * 0.5; // we end in an equalateral triangle.  this is its height
            //double thickness = _doublePolygon.StrokeThickness;
            double thickness = 0.0;

            if (RoadState == RoadState.Ship)
            {
                _gridShip.Visibility = Visibility.Visible;
                _gridRoads.Visibility = Visibility.Collapsed;
                _shipPolygon.Points.Clear();
                //
                //  this is the same polygon as the double Polygon
                _shipPolygon.Points.Add(new Point(thickness * .5, newSize.Height * .5));
                _shipPolygon.Points.Add(new Point(triangleHeight, thickness * .5));
                _shipPolygon.Points.Add(new Point(newSize.Width - triangleHeight, thickness * .5));
                _shipPolygon.Points.Add(new Point(newSize.Width - thickness * .5, newSize.Height * .5));
                _shipPolygon.Points.Add(new Point(newSize.Width - triangleHeight, newSize.Height - thickness * .5));
                _shipPolygon.Points.Add(new Point(triangleHeight, newSize.Height - thickness * .5));
                _ship.Width = newSize.Width;

            }
            else // a road or nothing
            {
                _doublePolygon.Points.Clear();

                _gridShip.Visibility = Visibility.Collapsed;
                _gridRoads.Visibility = Visibility.Visible;
                double height = 0;
                switch (RoadType)
                {
                    case RoadType.Single:
                         height = newSize.Height * 0.5;

                        //< Line Name = "_lineTop" X1 = "4.5" Y1 = "10" X2 = "63.5" Y2 = "10" StrokeThickness = "1" Stroke = "White" StrokeEndLineCap = "Triangle" StrokeStartLineCap = "Triangle" />
                        //< Line  Name = "_lineMiddle" X1 = "4.5" Y1 = "12" X2 = "63.5" Y2 = "12" StrokeThickness = "1" Stroke = "White" StrokeEndLineCap = "Triangle" StrokeStartLineCap = "Triangle" StrokeDashArray = "4" />
                        //< Line Name = "_lineBottom" X1 = "4.5" Y1 = "14" X2 = "63.5" Y2 = "14" StrokeThickness = "1" Stroke = "White" StrokeEndLineCap = "Triangle" StrokeStartLineCap = "Triangle" />

                        _lineTop.X1 = triangleHeight;
                        _lineTop.X2 = newSize.Width - triangleHeight;
                        _lineTop.Y1 = height + 2;
                        _lineTop.Y2 = height + 2;

                        _lineMiddle.X1 = triangleHeight;
                        _lineMiddle.X2 = newSize.Width - triangleHeight;
                        _lineMiddle.Y1 = newSize.Height * .75;
                        _lineMiddle.Y2 = _lineMiddle.Y1;


                        _lineBottom.X1 = triangleHeight;
                        _lineBottom.X2 = newSize.Width - triangleHeight;
                        _lineBottom.Y1 = newSize.Height - 2;
                        _lineBottom.Y2 = _lineBottom.Y1;



                        //          0   1   2       3
                        //Points=".5,8 68,8 63.5,16 4,16"

                        _doublePolygon.Points.Add(new Point(thickness * 0.5, height));
                        _doublePolygon.Points.Add(new Point(newSize.Width, height));
                        _doublePolygon.Points.Add(new Point(newSize.Width - triangleHeight, newSize.Height));
                        _doublePolygon.Points.Add(new Point(triangleHeight, newSize.Height));
                        break;
                    case RoadType.Double:
                         height = newSize.Height;
                        _lineTop.X1 = triangleHeight;
                        _lineTop.X2 = newSize.Width - triangleHeight;
                        _lineTop.Y1 = 2;
                        _lineTop.Y2 = _lineTop.Y1;

                        _lineMiddle.X1 = triangleHeight;
                        _lineMiddle.X2 = newSize.Width - triangleHeight;
                        _lineMiddle.Y1 = newSize.Height * .5;
                        _lineMiddle.Y2 = _lineMiddle.Y1;

                        _lineBottom.X1 = triangleHeight;
                        _lineBottom.X2 = newSize.Width - triangleHeight;
                        _lineBottom.Y1 = newSize.Height - 2;
                        _lineBottom.Y2 = _lineBottom.Y1;


                        //
                        //  these are the points that depend on width and height
                        //Points="1,8 4.6,1 63.4,1 67,8 63.4,15 4.5,16"


                        _doublePolygon.Points.Add(new Point(thickness * .5, newSize.Height * .5));
                        _doublePolygon.Points.Add(new Point(triangleHeight, thickness * .5));
                        _doublePolygon.Points.Add(new Point(newSize.Width - triangleHeight, thickness * .5));
                        _doublePolygon.Points.Add(new Point(newSize.Width - thickness * .5, newSize.Height * .5));
                        _doublePolygon.Points.Add(new Point(newSize.Width - triangleHeight, newSize.Height - thickness * .5));
                        _doublePolygon.Points.Add(new Point(triangleHeight, newSize.Height - thickness * .5));

                        break;
                    default:
                        _gridShip.Visibility = Visibility.Collapsed;
                        _gridRoads.Visibility = Visibility.Collapsed;
                        break;
                }
               
            }
        }

       

        public bool IsOwned
        {
            get
            {
                return (RoadState != RoadState.Unowned);
            }
        }
        internal List<RoadCtrl> OwnedAdjacentRoadsNotCounted(List<RoadCtrl> owned, RoadCtrl blockedFork, out bool adjacentFork)
        {
            List<RoadCtrl> list = new List<RoadCtrl>();
            foreach (RoadCtrl r in AdjacentRoads)
            {
                if (r.IsOwned && r.Color == this.Color)
                {

                    if (r.AdjacentBuildings[0].Owner?.GameData.PlayerColor != this.Color && r.AdjacentBuildings[0].BuildingState != BuildingState.None)
                        continue;
                    if (r.AdjacentBuildings[1].Owner?.GameData.PlayerColor!= this.Color && r.AdjacentBuildings[1].BuildingState != BuildingState.None)
                        continue;
                    if (owned.Contains(r) == false)
                    {
                        list.Add(r);
                    }
                }
            }

            adjacentFork = false;
            if (list.Contains(blockedFork))
            {
                list.Remove(blockedFork);
                adjacentFork = true;
            }

            return list;
        }

        public void Show(bool show, bool valid = true)
        {
            
            double opacity = 1.0;
            if (show)
            {
                
                if (!valid) opacity = 0.25;
                
            }
            else
            {

                opacity = 0.0;
            }

            _daAnimateOpacity.To = opacity;
            _sbAnimateOpacity.SkipToFill();
        }


        private void Road_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Callback?.RoadExited(this, e);

        }

        private void Road_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Callback?.RoadEntered(this, e);



        }
        private void Road_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
          //  this.TraceMessage(this.ToString());
            Callback?.RoadPressed(this, e);
        }

        public string ToString2()
        {
             string s = "";
            foreach (RoadKey key in Keys)
            {
                s += string.Format($"Name:{Name} \n{key.Tile} {key.RoadLocation} ");
            }

            return s + "\n";
        }

        public override string ToString()
        {
            return String.Format($"Index={Index}");
        }



        internal void Reset()
        {
            this.Owner = null;
            RoadState = RoadState.Unowned;
            Show(false);
        }
    }


    public class RoadKeyComparer : IEqualityComparer<RoadKey>
    {
        public bool Equals(RoadKey x, RoadKey y)
        {
            if (x.Tile.Index == y.Tile.Index)
            {
                if (x.RoadLocation == y.RoadLocation)
                {
                    return true;
                }
            }
            return false;
        }

        public int GetHashCode(RoadKey obj)
        {
            return obj.Tile.GetHashCode() * 17 + obj.RoadLocation.GetHashCode();
        }
    }

    public class RoadCloneComparer : IEqualityComparer<RoadCtrl>
    {
        RoadKeyComparer roadKeyComparer = new RoadKeyComparer();
        //
        //  when are two Roads "equal"?  When they have at least one clone that matches
        public bool Equals(RoadCtrl x, RoadCtrl y)
        {


            foreach (RoadKey key in x.Keys)
            {
                if (y.Keys.Contains(key, roadKeyComparer) == true)
                    return true;
            }

            return false;
        }

        public int GetHashCode(RoadCtrl obj)
        {
            return obj.GetHashCode();
        }
    }

    public class RoadKey
    {
        public TileCtrl Tile { get; set; }
        public RoadLocation RoadLocation { get; set; }
        public RoadKey(TileCtrl tile, RoadLocation loc)
        {
            Tile = tile;
            RoadLocation = loc;
        }

        public override string ToString()
        {
            return String.Format($"{Tile} {Tile.Index} {RoadLocation}");
        }
    }
    public class RoadLocationData
    {
        public double Left;
        public double Top;
        public double Angle;
        public RoadLocationData(double left, double top, double angle)
        {
            Left = left;
            Top = top;
            Angle = angle;
        }

        public override string ToString()
        {
            return String.Format($"Left={Left} Top={Top} Angle={Angle}");
        }

    }

    public enum RoadType { Single, Double };
}
