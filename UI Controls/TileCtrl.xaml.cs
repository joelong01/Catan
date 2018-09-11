using System;
using System.Collections.Generic;
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
    public class TileData
    {
        public TileOrientation TileOrientation { get; set; } = TileOrientation.FaceDown;

        public bool HasBaron { get; set; } = false;
        public bool HasPirateShip { get; set; } = false;

        public int Number { get; set; } = 0;
        public ResourceType ResourceType { get; set; } = ResourceType.Sea;
        public int zIndex { get; set; } = 0;
        public int Index { get; set; } = -1;
        public bool RandomTile { get; set; } = true;
        public bool ShowIndex { get; set; } = false;



        private readonly List<string> _savedProperties = new List<string> { "Number", "ResourceType", "TileOrientation", "zIndex", "Index", "RandomTile", "HexOrder", "ShowIndex" };
        public string Serialize(bool oneLine)
        {
            return StaticHelpers.SerializeObject<TileData>(this, _savedProperties, "=", "|");
        }
        public bool Deserialize(string s, bool oneLine)
        {

            StaticHelpers.DeserializeObject<TileData>(this, s, "=", "|");
            return true;
        }

    }



    public sealed partial class TileCtrl : UserControl, INotifyPropertyChanged, IEqualityComparer<TileCtrl>
    {

        private List<string> _savedProperties = new List<string> { "Number", "ResourceType", "HarborLocation", "HarborType", "TileOrientation", "zIndex", "Index", "HarborLocations", "RandomTile", "HexOrder", "UseClassic", "ShowIndex", "HasPirateShip", "HasBaron" };
        SolidColorBrush _blackBrush = new SolidColorBrush(Colors.Black);
        SolidColorBrush _whiteBrush = new SolidColorBrush(Colors.White);
        TileOrientation _tileOrientation = TileOrientation.FaceDown;
        public List<BuildingCtrl> Buildings { get; } = new List<BuildingCtrl>();
        public bool HasBaron { get; set; } = false;
        public bool HasPirateShip { get; set; } = false;

        public List<BuildingCtrl> OwnedBuilding { get; } = new List<BuildingCtrl>(); // this are the settlements that pay if this tile's number is rolled
        public bool RandomTile { get; set; } = true;
        bool _useClassic = true;
        int _index = -1;
        ITileControlCallback _tileControlCallback = null;
        ResourceType _resourceType = ResourceType.Back;
        //
        //  the row and column this tile is in the CatanHexPanel
        public int Row { get; set; } = -1;
        public int Col { get; set; } = -1;

        public TileCtrl()
        {
            this.InitializeComponent();
            _ppHexFront.RotationY = 90;



        }
        public int Probability => _number.Probability;
        public UIElement Visual => this;

        public NumberStyle NumberStyle
        {
            get => _number.NumberStyle;
            set => _number.NumberStyle = value;
        }





        public void SetTileCallback(ITileControlCallback tileControlCallback)
        {

            _tileControlCallback = tileControlCallback;

        }



        internal void Reset()
        {
            foreach (BuildingCtrl s in Buildings)
            {
                s.Reset();
            }

            OwnedBuilding.Clear();
        }


        public string Serialize(bool oneLine)
        {
            return StaticHelpers.SerializeObject<TileCtrl>(this, _savedProperties, "=", "|");
        }
        public bool Deserialize(string s, bool oneLine)
        {

            StaticHelpers.DeserializeObject<TileCtrl>(this, s, "=", "|");
            return true;
        }
        public int zIndex
        {
            get => Canvas.GetZIndex(this);
            set =>
                //this.TraceMessage($"Setting {this.ResourceType} of {this.Number} to zIndex of {value}");
                Canvas.SetZIndex(this, value);
        }

        public override string ToString()
        {
            return String.Format($"{_resourceType}={Number};idx={Index}");
        }
        public TileOrientation TileOrientation
        {
            get => _tileOrientation;
            set => SetTileOrientationAsync(value);
        }

        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                NotifyPropertyChanged();

            }
        }




        public ResourceType ResourceType
        {
            get => _resourceType;
            set
            {
                //
                //    don't protect _resourceType == value because we use that 
                //    when we change the type of tiles we are using
                _resourceType = value;
                if (!HideNumber)
                {
                    _number.Visibility = Visibility.Visible;
                }
                _number.Theme = NumberColorTheme.Dark;

                if (_resourceType == ResourceType.Sea)
                {
                    Number = 0;
                    RandomTile = false;
                }
                else
                {
                    RandomTile = true;
                }

                _number.ShowEyes = false;

                string bitmapPath = "ms-appx:Assets/back.jpg";

                switch (value)
                {
                    case ResourceType.Sheep:
                        bitmapPath = _useClassic ? "ms-appx:Assets/Old Visuals/old sheep.png" : "ms-appx:Assets/sheep.jpg";
                        break;
                    case ResourceType.Wood:
                        bitmapPath = _useClassic ? "ms-appx:Assets/Old Visuals/old wood.png" : "ms-appx:Assets/wood.jpg";
                        break;
                    case ResourceType.Ore:
                        bitmapPath = _useClassic ? "ms-appx:Assets/Old Visuals/old ore.png" : "ms-appx:Assets/ore.jpg";
                        break;
                    case ResourceType.Wheat:
                        bitmapPath = _useClassic ? "ms-appx:Assets/Old Visuals/old wheat.png" : "ms-appx:Assets/wheat.jpg";
                        break;
                    case ResourceType.Brick:
                        bitmapPath = _useClassic ? "ms-appx:Assets/Old Visuals/old brick.png" : "ms-appx:Assets/brick.jpg";
                        break;
                    case ResourceType.None:
                        bitmapPath = "ms-appx:Assets/back.jpg";
                        break;
                    case ResourceType.Desert:
                        bitmapPath = _useClassic ? "ms-appx:Assets/Old Visuals/old desert.png" : "ms-appx:Assets/desert.jpg";
                        break;
                    case ResourceType.GoldMine:
                        bitmapPath = "ms-appx:Assets/Old Visuals/gold mine 2.png";
                        break;

                    default:
                        break;

                }
                BitmapImage bitmapImage = new BitmapImage(new Uri(bitmapPath, UriKind.RelativeOrAbsolute));
                _hexFrontBrush.ImageSource = bitmapImage;
                _hexFrontBrush.Stretch = Stretch.UniformToFill;
                if (value == ResourceType.Sea)
                {
                    _hexFront.Stroke = _hexFrontBrush;

                }
                else
                {
                    _hexFront.Stroke = new SolidColorBrush(Colors.BurlyWood);
                }

                NotifyPropertyChanged();


            }

        }

        public void AnimateFade(double opacity, List<Task> tasks)
        {
            _daAnimateOpacity.Duration = TimeSpan.FromMilliseconds(MainPage.GetAnimationSpeed(AnimationSpeed.Fast)); // this is how long you take to fade, not how long you stay faded
            _daAnimateOpacity.To = opacity;
            tasks.Add(_sbAnimateOpacity.ToTask());

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
        public Task RotateTask(double angle, double ms)
        {
            _daRotateTile.To += angle;
            _daRotateTile.Duration = TimeSpan.FromMilliseconds(ms);
            return _sbRotate.ToTask();
        }
        public async Task SetTileOrientation(TileOrientation orientation, double animationDuration = Double.MaxValue, double startAfter = 0)
        {
            if (_tileOrientation == orientation)
            {
                return;
            }

            if (animationDuration == Double.MaxValue)
            {
                animationDuration = 1000;
            }

            bool flipToFaceUp = (_tileOrientation == TileOrientation.FaceDown) ? true : false;
            _tileOrientation = orientation;

            StaticHelpers.SetupFlipAnimation(flipToFaceUp, _daFlipBackTile, _daFlipFrontTile, MainPage.GetAnimationSpeed(AnimationSpeed.Normal), 0);

            List<Task> taskList = new List<Task>
            {
                _sbFlipTile.ToTask()
            };


            await Task.WhenAll(taskList);



        }
        public void SetTileOrientationAsync(TileOrientation orientation, double animationDuration = Double.MaxValue, double startAfter = 0)
        {
            if (_tileOrientation == orientation)
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

            bool flipToFaceUp = (_tileOrientation == TileOrientation.FaceDown) ? true : false;

            _tileOrientation = orientation;

            StaticHelpers.SetupFlipAnimation(flipToFaceUp, _daFlipBackTile, _daFlipFrontTile, animationDuration, 0);

            _sbFlipTile.Begin();



        }

        public void SetTileOrientation(TileOrientation orientation, List<Task> taskList, double animationDuration = Double.MaxValue)
        {
            if (orientation == _tileOrientation)
            {
                return;
            }

            bool flipToFaceUp = (_tileOrientation == TileOrientation.FaceDown) ? true : false;
            _tileOrientation = orientation;

            StaticHelpers.SetupFlipAnimation(flipToFaceUp, _daFlipBackTile, _daFlipFrontTile, MainPage.GetAnimationSpeed(AnimationSpeed.Normal), 0);

            taskList.Add(_sbFlipTile.ToTask());



        }

        public void Rotate(double angle, List<Task> tasks, bool reletive, double duration = Double.MaxValue)
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

        bool _hideNumber = false;
        public bool HideNumber
        {
            get => _hideNumber;
            set
            {
                _hideNumber = value;
                _number.Visibility = _hideNumber ? Visibility.Collapsed : Visibility.Visible;
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


        public void ResetTileRotation()
        {
            _daRotateTile.To = 0;
            _daRotateTile.Duration = TimeSpan.FromMilliseconds(MainPage.GetAnimationSpeed(AnimationSpeed.SuperFast));
            _sbRotate.Begin();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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

        internal void ResetOpacity()
        {
            _daAnimateOpacity.To = 1.0;
            ResourceTileGrid.Opacity = 1.0;
        }


        //
        //  needed to make the fancy distribution animation work right
        public Grid HexGrid => ResourceTileGrid;

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

        public double HexThickness => _hexFront.StrokeThickness;



        private void OnTileLeftTapped(object sender, TappedRoutedEventArgs e)
        {

        }


        private void OnTileRightTapped(object sender, RightTappedRoutedEventArgs e)
        {

            _tileControlCallback?.TileRightTapped(this, e);

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
    }

}
