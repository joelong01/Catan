using Catan.Proxy;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{

    public sealed partial class Harbor : UserControl, INotifyPropertyChanged
    {
        public Harbor()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        readonly SolidColorBrush _blackBrush = CatanColors.GetResourceBrush("Black", Colors.Black);
        readonly SolidColorBrush _whiteBrush = CatanColors.GetResourceBrush("White", Colors.White);
        private TileOrientation _orientation = TileOrientation.FaceDown;
        private bool _useClassic = true;
        private HarborLocation _location = HarborLocation.None;
        private readonly string[] _savePropName = new string[] { "HarborLocation" };

        public double HarborScale { get; set; } = 1.0;

        public static readonly DependencyProperty IndexProperty = DependencyProperty.Register("Index", typeof(int), typeof(Harbor), new PropertyMetadata(0));
        public static readonly DependencyProperty HarborTypeProperty = DependencyProperty.Register("HarborType", typeof(HarborType), typeof(Harbor), new PropertyMetadata(HarborType.Brick));
        public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register("Owner", typeof(PlayerModel), typeof(Harbor), new PropertyMetadata(null, OwnerChanged));
        public PlayerModel Owner
        {
            get => (PlayerModel)GetValue(OwnerProperty);
            set => SetValue(OwnerProperty, value);
        }
        private static void OwnerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as Harbor;
            var newOwner = (PlayerModel)e.NewValue;
            var oldOwner = (PlayerModel)e.OldValue;
            depPropClass?.SetOwner(oldOwner, newOwner);
        }
        //
        //  update the Owner about who owns this harbor
        private void SetOwner(PlayerModel oldOwner, PlayerModel newOwner)
        {
            if (oldOwner != null)
            {
                oldOwner.GameData.RemoveOwnedHarbor(this);
            }

            if (newOwner != null)
            {
                newOwner.GameData.AddOwnedHarbor(this);
            }
        }

        public HarborType HarborType
        {
            get => (HarborType)GetValue(HarborTypeProperty);
            set => SetValue(HarborTypeProperty, value);
        }
        public int Index
        {
            get => (int)GetValue(IndexProperty);
            set => SetValue(IndexProperty, value);
        }

        public string Serialize()
        {
            return this.SerializeObject<Harbor>(_savePropName, "=", "|");
        }

        public override string ToString()
        {
            return string.Format($"Index={Index} HarborLocation={_location} Type={HarborType}");
        }

        public void Deserialize(string s)
        {
            this.DeserializeObject<Harbor>(s, "=", "|");
        }


        public TileOrientation Orientation
        {
            get => _orientation;
            set
            {


                SetOrientationAsync(value, double.MaxValue);
                _orientation = value;

            }

        }
        

        public void Reset()
        {
            if (Owner != null)
            {
                Owner.GameData.RemoveOwnedHarbor(this);
                Owner = null;
            }

        }

        public FrameworkElement AnimationObject => _backGrid;

        public void SetHarborImage(HarborType value)
        {

            string bitmapPath = "ms-appx:Assets/back.jpg";

            if (_useClassic)
            {
                _text.Visibility = Visibility.Collapsed;

                switch (value)
                {
                    case HarborType.ThreeForOne:
                        bitmapPath = "ms-appx:Assets/Old Visuals/old 3 for 1.png";
                        break;
                    case HarborType.Brick:
                        bitmapPath = "ms-appx:Assets/Old Visuals/old 2 for 1 brick.png";
                        break;
                    case HarborType.Ore:
                        bitmapPath = "ms-appx:Assets/Old Visuals/old 2 for 1 Ore.png";
                        break;
                    case HarborType.Sheep:
                        bitmapPath = "ms-appx:Assets/Old Visuals/old 2 for 1 sheep.png";
                        break;
                    case HarborType.Wood:
                        bitmapPath = "ms-appx:Assets/Old Visuals/old 2 for 1 wood.png";
                        break;
                    case HarborType.Wheat:
                        bitmapPath = "ms-appx:Assets/Old Visuals/old 2 for 1 wheat.png";
                        break;
                    default:
                        break;
                }


            }
            else
            {
                _text.Visibility = Visibility.Visible;
                string s = "2 : 1";
                SolidColorBrush br = _whiteBrush;
                switch (HarborType)
                {
                    case HarborType.ThreeForOne:
                        s = "3:1";
                        bitmapPath = "ms-appx:/Assets/money.jpg";
                        break;
                    case HarborType.Brick:
                        bitmapPath = "ms-appx:/Assets/brick.jpg";
                        br = _blackBrush;
                        break;
                    case HarborType.Ore:
                        bitmapPath = "ms-appx:/Assets/ore.jpg";
                        break;
                    case HarborType.Sheep:
                        br = _blackBrush;
                        bitmapPath = "ms-appx:/Assets/sheep.jpg";
                        break;
                    case HarborType.Wood:
                        bitmapPath = "ms-appx:/Assets/wood.jpg";
                        break;
                    case HarborType.Wheat:
                        br = _blackBrush;
                        bitmapPath = "ms-appx:/Assets/wheat.jpg";
                        break;
                    default:
                        break;
                }

                _text.Text = s;
                _text.Foreground = br;
            }


            BitmapImage bitmapImage = new BitmapImage(new Uri(bitmapPath, UriKind.RelativeOrAbsolute));
            ImageBrush brush = new ImageBrush
            {
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                Stretch = Stretch.UniformToFill,
                ImageSource = bitmapImage
            };

            _front.Fill = brush;



        }

        public double TextSize
        {
            get => _text.FontSize;
            set => _text.FontSize = value;
        }


        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;



        public async Task SetOrientation(TileOrientation orientation, double animationDuration = double.MaxValue, double startAfter = 0)
        {
            if (_orientation == orientation)
            {
                return;
            }

            if (animationDuration == double.MaxValue)
            {
                animationDuration = 1000;
            }

            bool flipToFaceUp = (_orientation == TileOrientation.FaceDown) ? true : false;
            _orientation = orientation;

            StaticHelpers.SetupFlipAnimation(flipToFaceUp, _daFlipBack, _daFlipFront, MainPage.GetAnimationSpeed(AnimationSpeed.Normal), 0);

            await _sbFlip.ToTask();

        }

        public void SetOrientationAsync(TileOrientation orientation, double animationDuration = double.MaxValue)
        {
            if (_orientation == orientation)
            {
                return;
            }

            if (animationDuration == double.MaxValue)
            {
                animationDuration = 0;
            }
            else
            {
                animationDuration = MainPage.GetAnimationSpeed(AnimationSpeed.Normal);
            }

            bool flipToFaceUp = (_orientation == TileOrientation.FaceDown) ? true : false;
            _orientation = orientation;

            StaticHelpers.SetupFlipAnimation(flipToFaceUp, _daFlipBack, _daFlipFront, animationDuration, 0);

            _sbFlip.Begin();
        }

        public double ImageRotation
        {
            get => _gridFrontTransform.Rotation;
            set
            {
                _gridFrontTransform.Rotation = value;
                _gridBackTransform.Rotation = value;
            }
        }

        public double ImageZoom
        {
            get => _gridFrontTransform.ScaleX;
            set
            {
                _gridFrontTransform.ScaleX = value;
                _gridFrontTransform.ScaleY = value;
                _gridBackTransform.ScaleX = value;
                _gridBackTransform.ScaleY = value;
            }
        }

        public bool SmallHarbor
        {
            get => (_gridBackTransform.TranslateX == -15);
            set
            {
                if (value)
                {
                    double scale = .75;
                    double translate = -8;
                    _gridFrontTransform.ScaleX = scale;
                    _gridFrontTransform.ScaleY = scale;
                    _gridBackTransform.ScaleX = scale;
                    _gridBackTransform.ScaleY = scale;
                    _gridBackTransform.TranslateX = translate;
                    _gridFrontTransform.TranslateX = translate;

                }
                else
                {
                    _gridFrontTransform.ScaleX = 1.0;
                    _gridFrontTransform.ScaleY = 1.0;
                    _gridBackTransform.ScaleX = 1.0;
                    _gridBackTransform.ScaleY = 1.0;
                    _gridBackTransform.TranslateX = 0;
                    _gridFrontTransform.TranslateX = 0;

                }

            }
        }

        public bool UseClassic
        {
            get => _useClassic;

            set => _useClassic = value;
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

        public Task RotateTask(double angle, double ms)
        {

            _daRotate.To += angle;
            _daRotate.Duration = TimeSpan.FromMilliseconds(ms);
            return _sbRotate.ToTask();
        }

        public CompositeTransform Transform => _gridTransform;

        public bool Flip
        {
            get => (_gridTransform.Rotation == 180);
            set
            {
                if (value)
                {
                    _gridBackTransform.Rotation = 90;
                }
                else
                {
                    _gridTransform.Rotation = 0;
                }
            }
        }

        public double RotateImage
        {
            get => _gridFrontTransform.Rotation;
            set => _gridFrontTransform.Rotation = value;
        }
        //
        //  for information purposes only - no side affects
        public HarborLocation HarborLocation
        {
            get => _location;

            set => _location = value;
        }

        public int TileIndex { get; set; } = 0;

        private void HarborGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

        }
    }
}
