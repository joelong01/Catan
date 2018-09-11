﻿using System;
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

        HarborType myType = HarborType.Brick;
        SolidColorBrush _blackBrush = new SolidColorBrush(Colors.Black);
        SolidColorBrush _whiteBrush = new SolidColorBrush(Colors.White);
        TileOrientation _orientation = TileOrientation.FaceDown;
        bool _useClassic = true;
        HarborLocation _location = HarborLocation.None;

        string[] _savePropName = new string[] { "HarborLocation" };

        public double HarborScale { get; set; } = 1.0;

        public string Serialize()
        {
            return this.SerializeObject<Harbor>(_savePropName, "=", "|");
        }

        public override string ToString()
        {
            return String.Format($"HarborLocation={_location} Visibility={this.Visibility} Type={myType}");
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


                SetOrientationAsync(value, Double.MaxValue);
                _orientation = value;

            }

        }
        public Harbor()
        {
            this.InitializeComponent();
        }

        public HarborType HarborType
        {
            get => myType;
            set =>
                // don't protect! - you set it twice to get the classic image
                // myType set in SetHarborImage();                
                SetHarborImage(value);
        }

        public FrameworkElement AnimationObject => _backGrid;

        public void SetHarborImage(HarborType value)
        {
            myType = value;
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
                switch (myType)
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


        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;



        public async Task SetOrientation(TileOrientation orientation, double animationDuration = Double.MaxValue, double startAfter = 0)
        {
            if (_orientation == orientation)
            {
                return;
            }

            if (animationDuration == Double.MaxValue)
            {
                animationDuration = 1000;
            }

            bool flipToFaceUp = (_orientation == TileOrientation.FaceDown) ? true : false;
            _orientation = orientation;

            StaticHelpers.SetupFlipAnimation(flipToFaceUp, _daFlipBack, _daFlipFront, MainPage.GetAnimationSpeed(AnimationSpeed.Normal), 0);

            await _sbFlip.ToTask();

        }

        public void SetOrientationAsync(TileOrientation orientation, double animationDuration = Double.MaxValue)
        {
            if (_orientation == orientation)
            {
                return;
            }

            if (animationDuration == Double.MaxValue)
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

            set
            {
                _useClassic = value;
                HarborType = myType;
            }
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
