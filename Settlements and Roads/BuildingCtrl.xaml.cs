using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{


    /// <summary>
    ///     The states that a building can be in
    /// </summary>
    public enum BuildingState { None, Settlement, City, Pips };
  
    public sealed partial class BuildingCtrl : UserControl
    {
        double _baseOpacity = 0.0;
        public int Index { get; set; } = -1; // the Index into the Settlement list owned by the HexPanel...so we can save it and set it later
        CityCtrl _city = null;
        SettlementCtrl _settlement = null;

        SolidColorBrush _brush = new SolidColorBrush(Colors.Blue);
        public Dictionary<BuildingLocation, TileCtrl> BuildingToTileDictionary { get; set; } = new Dictionary<BuildingLocation, TileCtrl>();

        public List<RoadCtrl> AdjacentRoads { get; } = new List<RoadCtrl>();

        public List<BuildingCtrl> AdjacentBuildings { get; } = new List<BuildingCtrl>();

        //
        //  this the list of Tile/SettlmentLocations that are the same for this settlement
        public List<BuildingKey> Clones = new List<BuildingKey>();
        public Point LayoutPoint { get; set; }
        public CompositeTransform Transform { get { return (CompositeTransform)this.RenderTransform; } }

        public IGameCallback Callback { get; internal set; }

        PlayerData _playerData = null;
        public static readonly DependencyProperty BuildingStateProperty = DependencyProperty.Register("BuildingState", typeof(BuildingState), typeof(BuildingCtrl), new PropertyMetadata(BuildingState.None, BuildingStateChanged));
        public BuildingState BuildingState
        {
            get { return (BuildingState)GetValue(BuildingStateProperty); }
            set { SetValue(BuildingStateProperty, value); }
        }
        private static void BuildingStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BuildingCtrl depPropClass = d as BuildingCtrl;
            BuildingState depPropValue = (BuildingState)e.NewValue;
            depPropClass.SetBuildingState(depPropValue);
        }
      
        /// <summary>
        ///     the Visibility is bound to the property (and converted from the State with a value converter)
        ///     but the City/Settlement doesn't exist until we hit the state to save the startup cost.  so 
        ///     we create them here.
        /// </summary>
        /// <param name="value"></param>
        private void SetBuildingState(BuildingState value)
        {
            switch (value)
            {
                case BuildingState.Settlement:
                    if (_settlement == null)
                    {
                        _settlement = new SettlementCtrl();
                        _vbSettlement.Child = _settlement;
                        UpdateColors();
                    }                  
                    break;
                case BuildingState.City:
                    if (_city == null)
                    {
                        _city = new CityCtrl();
                        _vbCity.Child = _city;
                        UpdateColors();
                    }
                    break;
                default:
                    break;
            }
        }



        public PlayerData Owner
        {
            get
            {
                return _playerData;
            }
            set
            {
                if (_playerData != value)
                {
                    //   this.TraceMessage($"\nOwner for {this} set to {value}");
                    _playerData = value;
                }
            }
        }
        public int Pips
        {
            get
            {
                int pips = 0;
                foreach (var kvp in BuildingToTileDictionary)
                {
                    pips += kvp.Value.Pips;

                }
                return pips;

            }
        }
        public Color Color
        {
            get
            {
                return _brush.Color;
            }
            set
            {
                _brush = new SolidColorBrush(value);

                if (value == Colors.White)
                    _txtError.Foreground = new SolidColorBrush(Colors.Red);
                else
                    _txtError.Foreground = new SolidColorBrush(Colors.White);

                UpdateColors();

            }
        }

        internal void Reset()
        {
            Owner = null;
            Show(BuildingState.None);

        }

        public override string ToString()
        {
            return String.Format($"Index={Index};Type={BuildingState};Location={this.SettlementLocation};Background={Color};Pips={Pips}");
        }
       

        public bool IsCity
        {
            get
            {
                return BuildingState == BuildingState.City;
            }
        }

        public bool IsSettlement
        {
            get
            {
                return BuildingState == BuildingState.Settlement;
            }
        }

        public BuildingCtrl()
        {
            this.InitializeComponent();
            _gridBuildEllipse.Opacity = _baseOpacity;
         
            _buildEllipse.Fill = new SolidColorBrush(Colors.BurlyWood);
            if (StaticHelpers.IsInVisualStudioDesignMode)
            {
                _gridBuildEllipse.Opacity = 1.0;
            }


            this.Width = 30;
            this.Height = 30;
            this.BuildingState = BuildingState.None;
            //this..Show(BuildingState.Settlement);
            Canvas.SetZIndex(this, 20);
            this.RenderTransformOrigin = new Point(.5, .5);
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Top;
            CompositeTransform transform = new CompositeTransform
            {
                ScaleX = 1.0,
                ScaleY = 1.0
            };

            this.RenderTransform = transform;
            


        }
        private void Building_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //  OutputKeyInfo();
            Callback?.BuildingPointerPressed(this, e);
        }

        private void OutputKeyInfo()
        {
            string s = "";
            foreach (var key in Clones)
            {
                s += String.Format($"\n\tTile:{key.Tile} at {key.Location}");
            }
            s += "\n";
            this.TraceMessage(s);
        }

        private void Building_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Callback?.BuildingExited(this, e);
            // BuildingCtrl.HideBuildEllipse();
        }

        private void Building_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            //BuildingCtrl.ShowBuildEllipse();
            Callback?.BuildingEntered(this, e);

        }

        public BuildingLocation SettlementLocation { get; set; } = BuildingLocation.None;

        public void ShowBuildEllipse(bool canBuild = true, string colorAsString = "", string msg = "X")
        {
            _txtError.Text = msg;

          

            double opacity = 1.0;
            if (!canBuild) opacity = .25;

            _gridBuildEllipse.Opacity = opacity;

            
            if (colorAsString != "")
            {
                _buildEllipse.Fill = new SolidColorBrush(StaticHelpers.StringToColorDictionary[colorAsString]);
                _gridBuildEllipse.Opacity = 1.0;

            }

        }

        public bool BuildEllipseVisible
        {
            get
            {
                return _gridBuildEllipse.Opacity > 0;

            }
        }


        public void HideBuildEllipse()
        {
            _gridBuildEllipse.Opacity = _baseOpacity;
        }

        private void UpdateColors()
        {
            if (_settlement != null)
            {
                _settlement.CircleFillColor = _brush.Color;
            }
            if (_city != null)
            {
                _city.CircleFillColor = _brush.Color;
            }


            //_polySettlement.Fill = _brush;
            //_polySettlement.Stroke = _brush;

            //_polyCity.Fill = _brush;
            //_polyCity.Stroke = _brush;
            //_shade.Fill = _brush;
            _buildEllipse.Fill = _brush;




        }







        public void Show(BuildingState type)
        {
            _baseOpacity = type == BuildingState.None ? 0.0 : 1.0;


     

            //
            //  make the ellipse we use to show PointerEnter/Leaved locations
            _gridBuildEllipse.Opacity = _baseOpacity;
            if (type == BuildingState.None)
                _buildEllipse.Fill = new SolidColorBrush(Color);
            else
                _buildEllipse.Fill = new SolidColorBrush(Colors.Transparent);

          

            BuildingState = type;

        }

        public int ScoreValue
        {
            get
            {
                switch (BuildingState)
                {
                    case BuildingState.None:
                        return 0;
                    case BuildingState.Settlement:
                        return 1;
                    case BuildingState.City:
                        return 2;
                    default:
                        return 999;

                }
            }
        }

        internal void SetCallback(IGameCallback callback)
        {
            Callback = callback;
        }

        public void AddKey(TileCtrl tile, BuildingLocation loc)
        {
            BuildingKey key = new BuildingKey(tile, loc);
            foreach (var clone in Clones)
            {
                //
                //  need to do this by value because .Contains looks for the same pointer value
                if (clone.Tile.Index == tile.Index && clone.Location == loc)
                {
                    // this.TraceMessage($"{tile} @ {loc} already in Clones list!");
                    return;
                }
            }

            Clones.Add(key);

        }


    }

    public class KeyComparer : IEqualityComparer<BuildingKey>
    {
        //
        //  Note:  Once the board is created, we never change the Tiles or the Location of a Settlement...
        public bool Equals(BuildingKey x, BuildingKey y)
        {
            if (x.Tile.Index == y.Tile.Index)
            {
                if (x.Location == y.Location)
                {
                    return true;
                }
            }
            return false;
        }

        public int GetHashCode(BuildingKey obj)
        {
            return obj.Tile.GetHashCode() * 17 + obj.Location.GetHashCode();
        }
    }

    /// <summary>
    ///  This is for the "clones" support. the issue is that you can have multiple Tile/BuildingLocation pairs that map to the same
    ///  visual location for a Building.  We want to have exactly one building per location, so that datastructure allows us to find
    ///  duplicates and map the tuple (Tile,Location) to a unique Building
    /// </summary>
    public class BuildingKey
    {
        public TileCtrl Tile { get; set; }

        public BuildingLocation Location { get; set; }

        public BuildingKey(TileCtrl t, BuildingLocation loc)
        {
            Tile = t;
            Location = loc;
        }

        public override string ToString()
        {
            return String.Format($"[{Tile}. IDX={Tile.Index} @ {Location}]");
        }


    }

}
