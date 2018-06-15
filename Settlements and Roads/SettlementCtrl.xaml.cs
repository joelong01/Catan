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



    public enum SettlementType { None, Settlement, City };
    public class KeyComparer : IEqualityComparer<SettlementKey>
    {
        //
        //  Note:  Once the board is created, we never change the Tiles or the Location of a Settlement...
        public bool Equals(SettlementKey x, SettlementKey y)
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

        public int GetHashCode(SettlementKey obj)
        {
            return obj.Tile.GetHashCode() * 17 + obj.Location.GetHashCode();
        }
    }

    public class SettlementKey
    {
        public TileCtrl Tile { get; set; }

        public SettlementLocation Location { get; set; }

        public SettlementKey(TileCtrl t, SettlementLocation loc)
        {
            Tile = t;

            Location = loc;

        }

        public override string ToString()
        {
            return String.Format($"[{Tile}. IDX={Tile.Index} @ {Location}]");
        }


    }

    public sealed partial class SettlementCtrl : UserControl
    {
        double _baseOpacity = 0.0;
        public int Index { get; set; } = -1; // the Index into the Settlement list owned by the HexPanel...so we can save it and set it later
        CityCtrl _city = null;
        SettlementUi _settlement = null;

        SolidColorBrush _brush = new SolidColorBrush(Colors.Blue);
        public Dictionary<SettlementLocation, TileCtrl> SettlementToTileDict { get; set; } = new Dictionary<SettlementLocation, TileCtrl>();

        public List<RoadCtrl> AdjacentRoads { get; } = new List<RoadCtrl>();

        public List<SettlementCtrl> AdjacentSettlements { get; } = new List<SettlementCtrl>();

        //
        //  this the list of Tile/SettlmentLocations that are the same for this settlement
        public List<SettlementKey> Clones = new List<SettlementKey>();
        public Point LayoutPoint { get; set; }
        public CompositeTransform Transform { get { return (CompositeTransform)this.RenderTransform; } }

        public IGameCallback Callback { get; internal set; }

        PlayerData _playerData = null;
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
                foreach (var kvp in SettlementToTileDict)
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
            Show(SettlementType.None);

        }

        public override string ToString()
        {
            return String.Format($"Index={Index};Type={SettlementType};Location={this.SettlementLocation};Background={Color};Pips={Pips}");
        }
        public SettlementType SettlementType
        {
            get
            {
                if (_canvasSettlement.Visibility == Visibility.Visible)
                    return SettlementType.Settlement;

                if (_canvasCity.Visibility == Visibility.Visible)
                    return SettlementType.City;

                return SettlementType.None;

            }
            set
            {
                switch (value)
                {
                    case SettlementType.None:
                        _canvasSettlement.Visibility = Visibility.Collapsed;
                        _canvasCity.Visibility = Visibility.Collapsed;
                        break;
                    case SettlementType.Settlement:
                        if (_settlement == null)
                        {
                            _settlement = new SettlementUi();
                            _vbSettlement.Child = _settlement;
                            UpdateColors();
                        }
                        _canvasSettlement.Visibility = Visibility.Visible;
                        _canvasCity.Visibility = Visibility.Collapsed;
                        break;
                    case SettlementType.City:
                        if (_city == null)
                        {
                            _city = new CityCtrl();
                            _vbCity.Child = _city;
                            UpdateColors();
                        }
                        _canvasSettlement.Visibility = Visibility.Collapsed;
                        _canvasCity.Visibility = Visibility.Visible;
                        break;
                    default:
                        break;
                }
            }
        }

        public bool IsCity
        {
            get
            {
                return SettlementType == SettlementType.City;
            }
        }

        public bool IsSettlement
        {
            get
            {
                return SettlementType == SettlementType.Settlement;
            }
        }

        public SettlementCtrl()
        {
            this.InitializeComponent();
            _gridBuildEllipse.Opacity = _baseOpacity;
            _canvasCity.Visibility = Visibility.Collapsed;
            _canvasSettlement.Visibility = Visibility.Collapsed;
            _buildEllipse.Fill = new SolidColorBrush(Colors.BurlyWood);
            if (StaticHelpers.IsInVisualStudioDesignMode)
            {
                _gridBuildEllipse.Opacity = 1.0;
            }


            this.Width = 30;
            this.Height = 30;
            this.SettlementType = SettlementType.None;
            //this..Show(SettlementType.Settlement);
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
            this.PointerEntered += Settlement_PointerEntered;
            this.PointerExited += Settlement_PointerExited;
            this.PointerPressed += Settlement_PointerPressed;


        }
        private void Settlement_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //  OutputKeyInfo();
            Callback?.SettlementPointerPressed(this, e);
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

        private void Settlement_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Callback?.SettlementExited(this, e);
            // SettlementCtrl.HideBuildEllipse();
        }

        private void Settlement_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            //SettlementCtrl.ShowBuildEllipse();
            Callback?.SettlementEntered(this, e);

        }

        public SettlementLocation SettlementLocation { get; set; } = SettlementLocation.None;

        public void ShowBuildEllipse(bool canBuild = true, string colorAsString = "", string msg = "X")
        {
            _txtError.Text = msg;

            if (_canvasCity.Visibility == Visibility.Visible)
                return;

            if (_canvasSettlement.Visibility == Visibility.Visible)
                return;

            double opacity = 1.0;
            if (!canBuild) opacity = .25;

            _gridBuildEllipse.Opacity = opacity;

            _txtError.Visibility = canBuild ? Visibility.Collapsed : Visibility.Visible;
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







        public void Show(SettlementType type)
        {
            _baseOpacity = type == SettlementType.None ? 0.0 : 1.0;


            _canvasCity.Visibility = Visibility.Collapsed;
            _canvasSettlement.Visibility = Visibility.Collapsed;

            //
            //  make the ellipse we use to show PointerEnter/Leaved locations
            _gridBuildEllipse.Opacity = _baseOpacity;
            if (type == SettlementType.None)
                _buildEllipse.Fill = new SolidColorBrush(Color);
            else
                _buildEllipse.Fill = new SolidColorBrush(Colors.Transparent);

            if (type == SettlementType.City)
            {
                _canvasSettlement.Visibility = Visibility.Visible;
            }

            if (type == SettlementType.Settlement)
            {

                _canvasSettlement.Visibility = Visibility.Visible;
            }

            SettlementType = type;

        }

        public int ScoreValue
        {
            get
            {
                switch (SettlementType)
                {
                    case SettlementType.None:
                        return 0;
                    case SettlementType.Settlement:
                        return 1;
                    case SettlementType.City:
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

        public void AddKey(TileCtrl tile, SettlementLocation loc)
        {
            SettlementKey key = new SettlementKey(tile, loc);
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
}
