using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Catan.Proxy;

using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Catan10.CatanService;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class TileCtrl : UserControl, INotifyPropertyChanged, IEqualityComparer<TileCtrl>
    {
        private TileOrientation ActualOrientTation
        {
            get
            {
                return (_ppHexFront.RotationY != 0) ? TileOrientation.FaceDown : TileOrientation.FaceUp;
            }
        }

        private ResourceType _actingResourceType = ResourceType.Back;
        private bool _hideNumber = false;
        private int _index = -1;
        private ResourceType _normalResourceType = ResourceType.None;
        private ITileControlCallback _tileControlCallback = null;

        private static void CurrentPlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as TileCtrl;
            var depPropValue = (PlayerModel)e.NewValue;
            depPropClass?.SetCurrentPlayer(depPropValue);
        }

        private static void ResourceTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as TileCtrl;
            var depPropValue = (ResourceType)e.NewValue;
            depPropClass?.SetResourceType(depPropValue);
        }

        private static void ShownResourceTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as TileCtrl;
            var depPropValue = (ResourceType)e.NewValue;
            depPropClass?.SetShownResourceType(depPropValue);
        }

        private static void TileDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as TileCtrl;
            var depPropValue = (TileData)e.NewValue;
            depPropClass?.SetTileData(depPropValue);
        }

        private static void TileOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as TileCtrl;
            var depPropValue = (TileOrientation)e.NewValue;
            depPropClass?.SetTileOrientation(depPropValue);
        }

        private void AnimateFadeCompleted(object sender, object e)
        {
            _sbAnimateOpacityReverse.Begin();
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnTileLeftTapped(object sender, TappedRoutedEventArgs e)
        {
        }

        private void OnTileRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            _tileControlCallback?.TileRightTapped(this, e);
        }

        //
        //  called whenver the current player changes
        private void SetCurrentPlayer(PlayerModel current)
        {
            this.StopHighlightingTile();
        }

        private void SetResourceType(ResourceType value)
        {
            //
            //    don't protect _resourceType == value because we use that
            //    when we change the type of tiles we are using
            _actingResourceType = value;
            _normalResourceType = value;
            ShowMainResourceType(_normalResourceType);
        }

        private void SetShownResourceType(ResourceType value)
        {
        }

        private void SetTileData(TileData tileData)
        {
        }

        private void SetTileOrientation(TileOrientation value)
        {
            SetTileOrientationAsync(value);
        }

        private void ShowMainResourceType(ResourceType resourceType)
        {
            if (!HideNumber)
            {
                _number.Visibility = Visibility.Visible;
            }
            _number.Theme = NumberColorTheme.Dark;

            if (_actingResourceType == ResourceType.Sea)
            {
                Number = 0;
                RandomGoldEligible = false;
            }
            else
            {
                RandomGoldEligible = true;
            }

            _number.ShowEyes = false;
            ShownResourceType = resourceType;
        }

        internal void Reset()
        {
            OwnedBuilding.Clear();
            StopHighlightingTile();
        }

        internal void ResetOpacity()
        {
            _daAnimateOpacity.To = 1.0;
            ResourceTileGrid.Opacity = 1.0;
            _sbAnimateOpacity.SkipToFill();
        }

        public TileCtrl()
        {
            this.InitializeComponent();
            this.DataContext = this;
            _ppHexFront.RotationY = 90;
        }

        public Harbor AdjacentHarbor
        {
            get => (Harbor)GetValue(AdjacentHarborProperty);
            set => SetValue(AdjacentHarborProperty, value);
        }

        public int Col { get; set; } = -1;

        public PlayerModel CurrentPlayer
        {
            get => (PlayerModel)GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }

        public bool HasBaron { get; set; } = false;

        public bool HasPirateShip { get; set; } = false;

        //
        //  needed to make the fancy distribution animation work right
        public Grid HexGrid => ResourceTileGrid;

        public double HexThickness => _hexFront.StrokeThickness;

        public bool HideNumber
        {
            get => _hideNumber;
            set
            {
                _hideNumber = value;
                _number.Visibility = _hideNumber ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        /// <summary>
        ///     the reason this is not a property to get/set the highlight is that the highlight brush
        ///     will change every time the player changes
        /// </summary>
        public bool Highlighted { get; private set; } = false;

        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                NotifyPropertyChanged();
            }
        }

        public int Number
        {
            get => _number.Number;
            set
            {
                _number.Number = value;
                //
                //  this is for reshuffle -- without this the last tile that was a Baron doesn't get a number
                if (value == 7 || value == 0 || HideNumber)
                {
                    _number.Visibility = Visibility.Collapsed;
                }
                else
                {
                    _number.Visibility = Visibility.Visible;
                }
            }
        }

        public NumberStyle NumberStyle
        {
            get => _number.NumberStyle;
            set => _number.NumberStyle = value;
        }

        public List<BuildingCtrl> OwnedBuilding { get; } = new List<BuildingCtrl>();

        public int Pips
        {
            get
            {
                switch (Number)
                {
                    case 12:
                    case 2:
                        return 1;

                    case 11:
                    case 3:
                        return 2;

                    case 4:
                    case 10:
                        return 3;

                    case 5:
                    case 9:
                        return 4;

                    case 6:
                    case 8:
                        return 5;

                    default:
                        return 0; //7
                }
            }
        }

        // this are the settlements that pay if this tile's number is rolled
        public int Probability => _number.Probability;

        public bool RandomGoldEligible { get; set; } = true;

        public ResourceType ResourceType
        {
            get
            {
                if (TemporarilyGold)
                {
                    return ResourceType.GoldMine;
                }

                return (ResourceType)GetValue(ResourceTypeProperty);
            }
            set => SetValue(ResourceTypeProperty, value);
        }

        // you don't want the RandomGold to land on a sea tile
        // a tile can temporarily be gold
        //
        //  the row and column this tile is in the CatanHexPanel
        public int Row { get; set; } = -1;

        public bool Selected
        {
            get => _border.Visibility == Visibility.Visible;
            set => _border.Visibility = (value == true) ? Visibility.Visible : Visibility.Collapsed;
        }

        public bool ShowIndex
        {
            get => _txtIndex.Visibility == Visibility.Visible;
            set => _txtIndex.Visibility = (value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public ResourceType ShownResourceType
        {
            get => (ResourceType)GetValue(ShownResourceTypeProperty);
            set => SetValue(ShownResourceTypeProperty, value);
        }

        public bool TemporarilyGold
        {
            get
            {
                return ShownResourceType == ResourceType.GoldMine;
            }
            set
            {
                //
                //  update only if it changes
                if (value != TemporarilyGold)
                {
                    ShownResourceType = (value) ? ResourceType.GoldMine : _normalResourceType;
                }
            }
        }

        public TileData TileData
        {
            get => (TileData)GetValue(TileDataProperty);
            set => SetValue(TileDataProperty, value);
        }

        public TileOrientation TileOrientation
        {
            get => (TileOrientation)GetValue(TileOrientationProperty);
            set => SetValue(TileOrientationProperty, value);
        }

        public bool UseClassic
        {
            get
            {
                return true; ;
            }

            set
            {
            }
        }

        public UIElement Visual => this;

        public int ZIndex
        {
            get => Canvas.GetZIndex(this);
            set =>
                //this.TraceMessage($"Setting {this.ResourceType} of {this.Number} to zIndex of {value}");
                Canvas.SetZIndex(this, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //
        //  keep track of harbors near this tile.  this is set in the XAML
        public static readonly DependencyProperty AdjacentHarborProperty = DependencyProperty.Register("AdjacentHarbor", typeof(Harbor), typeof(TileCtrl), new PropertyMetadata(null));

        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(TileCtrl), new PropertyMetadata(new PlayerModel(), CurrentPlayerChanged));

        public static readonly DependencyProperty ResourceTypeProperty = DependencyProperty.Register("ResourceType", typeof(ResourceType), typeof(TileCtrl), new PropertyMetadata(ResourceType.None, ResourceTypeChanged));

        public static readonly DependencyProperty ShownResourceTypeProperty = DependencyProperty.Register("ShownResourceType", typeof(ResourceType), typeof(TileCtrl), new PropertyMetadata(ResourceType.Sheep, ShownResourceTypeChanged));

        public static readonly DependencyProperty TileDataProperty = DependencyProperty.Register("TileData", typeof(TileData), typeof(TileCtrl), new PropertyMetadata(new TileData(), TileDataChanged));

        public static readonly DependencyProperty TileOrientationProperty = DependencyProperty.Register("TileOrientation", typeof(TileOrientation), typeof(TileCtrl), new PropertyMetadata(TileOrientation.FaceUp, TileOrientationChanged));

        public void AnimateFade(double opacity, List<Task> tasks)
        {
            double fast = MainPage.GetAnimationSpeed(AnimationSpeed.Fast);
            double slow = fast + MainPage.GetAnimationSpeed(AnimationSpeed.Slow);
            _daAnimateOpacity.Duration = TimeSpan.FromMilliseconds(fast); // this is how long you take to fade, not how long you stay faded
            _daAnimateOpacity.To = opacity;
            tasks.Add(_sbAnimateOpacity.ToTask());
            _daAnimateOpacityReverse.Duration = TimeSpan.FromMilliseconds(fast); // this is how long you take to fade, not how long you stay faded
            _daAnimateOpacityReverse.To = 1.0;
            _sbAnimateOpacityReverse.BeginTime = TimeSpan.FromSeconds(slow); // this is the delat
            tasks.Add(_sbAnimateOpacityReverse.ToTask());
        }

        public void AnimateFadeAsync(double opacity)
        {
            // CancelFade();
            //double fast = 100; //MainPage.GetAnimationSpeed(AnimationSpeed.Fast);
            //double slow = 1000; // fast + MainPage.GetAnimationSpeed(AnimationSpeed.Slow);
            //_sbAnimateOpacity.Duration = TimeSpan.FromMilliseconds(fast);
            //_sbAnimateOpacityReverse.Duration = TimeSpan.FromMilliseconds(fast);

            //_daAnimateOpacity.Duration = TimeSpan.FromMilliseconds(0);  //TimeSpan.FromMilliseconds(fast); // this is how long you take to fade, not how long you stay faded
            //_daAnimateOpacity.To = opacity;
            //_daAnimateOpacity.From  = 1.0;

            //_daAnimateOpacityReverse.Duration = TimeSpan.FromMilliseconds(fast); // this is how long you take to fade, not how long you stay faded
            //_daAnimateOpacityReverse.To = 1.0;
            //_sbAnimateOpacityReverse.BeginTime = TimeSpan.FromSeconds(slow); // this is the delat
            CancelFade();
            _sbAnimateOpacity.Begin();
        }

        public Task AnimateMoveTask(Point to, double ms, double startAfter)
        {
            _daToX.To = to.X;
            _daToY.To = to.Y;
            _daToX.Duration = new Duration(TimeSpan.FromMilliseconds(ms));
            _daToY.Duration = _daToX.Duration;

            _daToX.BeginTime = TimeSpan.FromMilliseconds(startAfter);
            _daToY.BeginTime = TimeSpan.FromMilliseconds(startAfter);
            return _sbMoveTile.ToTask();
        }

        public Brush BrushifyResourceType(ResourceType resourceType)
        {
            string key = "ResourceType." + resourceType.ToString();
            return (Brush)App.Current.Resources[key];
        }

        public void CancelFade()
        {
            _sbAnimateOpacityReverse.Seek(TimeSpan.FromSeconds(0));
            _sbAnimateOpacity.Seek(TimeSpan.FromSeconds(0));
            this.Opacity = 1.0;
        }

        public bool Equals(TileCtrl x, TileCtrl y)
        {
            if (x.ResourceType == y.ResourceType && x.Index == y.Index)
            {
                return true;
            }

            return false;
        }

        public int GetHashCode(TileCtrl obj)
        {
            return obj.GetHashCode();
        }

        public void HighlightTile() // you should never see Hotpink!
        {
            _hexFront.Stroke = CurrentPlayer.BackgroundBrush;
            Highlighted = true;
        }

        public Brush LoadTileImage(ResourceType resource)
        {
            string key = "TileType." + resource.ToString();
            return (Brush)App.Current.Resources[key];
        }

        public void ResetTileRotation()
        {
            _daRotateTile.To = 0;
            _daRotateTile.Duration = TimeSpan.FromMilliseconds(MainPage.GetAnimationSpeed(AnimationSpeed.SuperFast));
            _sbRotate.Begin();
        }

        public void Rotate(double angle, List<Task> tasks, bool reletive, double duration = double.MaxValue)
        {
            if (duration == double.MaxValue)
            {
                duration = MainPage.GetAnimationSpeed(AnimationSpeed.Fast);
            }

            if (reletive)
            {
                _daRotateTile.To += angle;
            }
            else
            {
                if (_daRotateTile.To / 360 == angle)
                {
                    return;
                }

                _daRotateTile.To = angle;
            }
            _daRotateTile.Duration = TimeSpan.FromMilliseconds(duration);
            tasks.Add(_sbRotate.ToTask());
        }

        public Task RotateTask(double angle, double ms)
        {
            _daRotateTile.To += angle;
            _daRotateTile.Duration = TimeSpan.FromMilliseconds(ms);
            return _sbRotate.ToTask();
        }

        public void SetTileCallback(ITileControlCallback tileControlCallback)
        {
            _tileControlCallback = tileControlCallback;
        }

        public async Task SetTileOrientation(TileOrientation orientation, double animationDuration = double.MaxValue, double startAfter = 0)
        {
            if (ActualOrientTation == orientation) return;

            if (animationDuration == double.MaxValue)
            {
                animationDuration = 0;
            }
            else
            {
                animationDuration = MainPage.GetAnimationSpeed(AnimationSpeed.Normal);
            }
            TileOrientation = orientation;

            StaticHelpers.SetupFlipAnimation(orientation == TileOrientation.FaceUp, _daFlipBackTile, _daFlipFrontTile, animationDuration, 0);

            List<Task> taskList = new List<Task>
            {
                _sbFlipTile.ToTask()
            };

            await Task.WhenAll(taskList);
        }

        public void SetTileOrientation(TileOrientation orientation, List<Task> taskList, double animationDuration = double.MaxValue)
        {
            //   if (ActualOrientTation == orientation) return;

            StaticHelpers.SetupFlipAnimation(orientation == TileOrientation.FaceUp, _daFlipBackTile, _daFlipFrontTile, MainPage.GetAnimationSpeed(AnimationSpeed.Normal), 0);

            taskList.Add(_sbFlipTile.ToTask());
        }

        public void SetTileOrientationAsync(TileOrientation orientation, double animationDuration = double.MaxValue, double startAfter = 0)
        {
            //  if (ActualOrientTation == orientation) return;
            _sbFlipTile.SkipToFill();

            if (animationDuration == double.MaxValue)
            {
                animationDuration = 0;
            }
            else
            {
                animationDuration = MainPage.GetAnimationSpeed(AnimationSpeed.Normal);
            }

            StaticHelpers.SetupFlipAnimation(orientation == TileOrientation.FaceUp, _daFlipBackTile, _daFlipFrontTile, animationDuration, 0);

            _sbFlipTile.Begin();
        }

        public void Show(TileDisplay toShow)
        {
            switch (toShow)
            {
                case TileDisplay.Normal:
                    ShowMainResourceType(_normalResourceType);

                    break;

                case TileDisplay.Gold:

                    ShowMainResourceType(ResourceType.GoldMine);
                    break;

                default:
                    break;
            }
        }

        public void StopHighlightingTile() // you should never see Hotpink!
        {
            if (!Highlighted) return;

            if (this.ResourceType == ResourceType.Sea)
            {
                _hexFront.Stroke = (Brush)App.Current.Resources["ResourceType.Sea"];
            }
            else
            {
                _hexFront.Stroke = (Brush)App.Current.Resources["bmMaple"];
            }
            Highlighted = false;
        }

        public override string ToString()
        {
            return string.Format($"{_actingResourceType}={Number};idx={Index}");
        }
    }

    public class TileData : INotifyPropertyChanged
    {
        private int _col = -1;
        private HarborLocation _harborLocation = HarborLocation.None;
        private HarborType _harborType = HarborType.None;
        private bool _hasBaron = false;
        private bool _hasPirateShip = false;
        private int _index = -1;
        private int _number = 0;
        private List<BuildingCtrl> _ownedBuildings = new List<BuildingCtrl>();
        private bool _randomTile = true;
        private ResourceType _resourceType = ResourceType.Sea;
        private int _row = -1;
        private bool _showIndex = false;
        private TileOrientation _tileOrientation = TileOrientation.FaceDown;
        private bool _useClassic = true;
        private int _zIndex = 0;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int Col
        {
            get
            {
                return _col;
            }
            set
            {
                if (value != _col)
                {
                    _col = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public HarborLocation HarborLocation
        {
            get
            {
                return _harborLocation;
            }
            set
            {
                if (value != _harborLocation)
                {
                    _harborLocation = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public HarborType HarborType
        {
            get
            {
                return _harborType;
            }
            set
            {
                if (value != _harborType)
                {
                    _harborType = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool HasBaron
        {
            get
            {
                return _hasBaron;
            }
            set
            {
                if (value != _hasBaron)
                {
                    _hasBaron = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool HasPirateShip
        {
            get
            {
                return _hasPirateShip;
            }
            set
            {
                if (value != _hasPirateShip)
                {
                    _hasPirateShip = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                if (value != _index)
                {
                    _index = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int Number
        {
            get
            {
                return _number;
            }
            set
            {
                if (value != _number)
                {
                    _number = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public List<BuildingCtrl> OwnedBuildings
        {
            get
            {
                return _ownedBuildings;
            }
            set
            {
                if (value != _ownedBuildings)
                {
                    _ownedBuildings = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool RandomTile
        {
            get
            {
                return _randomTile;
            }
            set
            {
                if (value != _randomTile)
                {
                    _randomTile = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ResourceType ResourceType
        {
            get
            {
                return _resourceType;
            }
            set
            {
                if (value != _resourceType)
                {
                    _resourceType = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int Row
        {
            get
            {
                return _row;
            }
            set
            {
                if (value != _row)
                {
                    _row = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool ShowIndex
        {
            get
            {
                return _showIndex;
            }
            set
            {
                if (value != _showIndex)
                {
                    _showIndex = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public TileOrientation TileOrientation
        {
            get
            {
                return _tileOrientation;
            }
            set
            {
                if (value != _tileOrientation)
                {
                    _tileOrientation = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool UseClassic
        {
            get
            {
                return _useClassic;
            }
            set
            {
                if (value != _useClassic)
                {
                    _useClassic = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int zIndex
        {
            get
            {
                return _zIndex;
            }
            set
            {
                if (value != _zIndex)
                {
                    _zIndex = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static TileData Deserialize(string json)
        {
            return CatanSignalRClient.Deserialize<TileData>(json);
        }

        public string Serialize(bool oneLine)
        {
            return CatanSignalRClient.Serialize(this, !oneLine);
        }
    }
}
