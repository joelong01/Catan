using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{

    public sealed partial class OldHarbor : UserControl, INotifyPropertyChanged
    {

        HarborType myType = HarborType.Brick;
        SolidColorBrush _blackBrush = new SolidColorBrush(Colors.Black);
        SolidColorBrush _whiteBrush = new SolidColorBrush(Colors.White);
        TileOrientation _orientation = TileOrientation.FaceDown;
        bool _useClassic = false;



        public TileOrientation Orientation
        {
            get { return _orientation; }
            set
            {


                SetOrientationAsync(value, Double.MaxValue);
                _orientation = value;

            }

        }
        public OldHarbor()
        {
            this.InitializeComponent();
        }

        public HarborType HarborType
        {
            get { return myType; }
            set
            {
                SetHarborImage(value);

            }
        }

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
            ImageBrush brush = new ImageBrush();
            brush.AlignmentX = AlignmentX.Left;
            brush.AlignmentY = AlignmentY.Top;
            brush.Stretch = Stretch.UniformToFill;
            brush.ImageSource = bitmapImage;

            _front.Fill = brush;



        }

        public double TextSize
        {
            get
            {
                return _text.FontSize;
            }
            set
            {
                _text.FontSize = value;
            }
        }


        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;



        public async Task SetOrientation(TileOrientation orientation, double animationDuration = Double.MaxValue, double startAfter = 0)
        {
            if (_orientation == orientation)
                return;


            if (animationDuration == Double.MaxValue)
                animationDuration = 1000;

            bool flipToFaceUp = (_orientation == TileOrientation.FaceDown) ? true : false;
            _orientation = orientation;

            StaticHelpers.SetupFlipAnimation(flipToFaceUp, _daFlipBack, _daFlipFront, MainPage.GetAnimationSpeed(AnimationSpeed.Normal), 0);

            await _sbFlip.ToTask();

        }

        public void SetOrientationAsync(TileOrientation orientation, double animationDuration = Double.MaxValue)
        {
            if (_orientation == orientation)
                return;


            if (animationDuration == Double.MaxValue)
                animationDuration = 0;
            else
                animationDuration = MainPage.GetAnimationSpeed(AnimationSpeed.Normal);

            bool flipToFaceUp = (_orientation == TileOrientation.FaceDown) ? true : false;
            _orientation = orientation;

            StaticHelpers.SetupFlipAnimation(flipToFaceUp, _daFlipBack, _daFlipFront, animationDuration, 0);

            _sbFlip.Begin();
        }

        public double ImageRotation
        {
            get
            {
                return _gridFrontTransform.Rotation;
            }
            set
            {
                _gridFrontTransform.Rotation = value;
                _gridBackTransform.Rotation = value;
            }
        }

        public double ImageZoom
        {
            get
            {
                return _gridFrontTransform.ScaleX;
            }
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
            get
            {
                return (_gridBackTransform.TranslateX == -15);
            }
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
            get
            {
                return _useClassic;
            }

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

        public CompositeTransform Transform
        {
            get
            {
                return _gridTransform;
            }
        }
        internal static async Task FancyDistribution2(List<OldHarbor> harbors)
        {

            //  await Fancy3(harbors);

            List<Task> list = new List<Task>();
            double ms = MainPage.GetAnimationSpeed(AnimationSpeed.Fast);

            for (int i = 0; i < harbors.Count; i++)
            {
                int j = i + 1;
                if (j == harbors.Count) j = 0;

                GeneralTransform gt = harbors[j].TransformToVisual(harbors[i]);
                Point pt = gt.TransformPoint(new Point(0, 0));
                Task task = harbors[i].AnimateMoveTask(pt, ms, i * ms);
                list.Add(task);              
            }

            await Task.WhenAll(list);
            list.Clear();

            for (int i = harbors.Count - 1; i >= 0; i--)
            {
                int j = i - 1;
                if (j == -1) j = harbors.Count - 1;

                GeneralTransform gt = harbors[j].TransformToVisual(harbors[i]);
                Point pt = gt.TransformPoint(new Point(0, 0));
                Task task = harbors[i].AnimateMoveTask(pt, ms, i * ms);
                list.Add(task);

            }
            await Task.WhenAll(list);
            list.Clear();

            int counter = 0;
            ms = ms = MainPage.GetAnimationSpeed(AnimationSpeed.Fast);
            foreach (var h in harbors)
            {
                Task task = h.SetOrientation(TileOrientation.FaceUp, ms, counter * ms);
                counter++;
                list.Add(task);
            }

            await Task.WhenAll(list);

        }

        static async Task Fancy3(List<OldHarbor> harbors)
        {
            List<Task> list = new List<Task>();
            double ms = MainPage.GetAnimationSpeed(AnimationSpeed.Fast);
            int i = 0;

            foreach (var h in harbors)
            {
                GeneralTransform gt = harbors[0].TransformToVisual(h);
                Point pt = gt.TransformPoint(new Point(0, 0));
                Task task = h.AnimateMoveTask(pt, ms, i * ms);
                i++;
                Canvas.SetZIndex(h, 1000 - i);
                list.Add(task);
            }

            await Task.WhenAll(list);
            list.Clear();
            i = 0;
            Random r = new Random(DateTime.Now.Millisecond);
            ms = MainPage.GetAnimationSpeed(AnimationSpeed.Fast);
            foreach (var h in harbors)
            {
                Task task = h.RotateTask(r.Next(1, 15) * 360, ms);
                list.Add(task);
            }
            await Task.WhenAll(list);
            list.Clear();
            i = 0;
            ms = MainPage.GetAnimationSpeed(AnimationSpeed.Fast);
            foreach (var h in harbors)
            {
                Task task = h.AnimateMoveTask(new Point(0, 0), ms, i * ms);
                i++;
                list.Add(task);
            }
            await Task.WhenAll(list);
            list.Clear();
            i = 0;
            ms = ms = MainPage.GetAnimationSpeed(AnimationSpeed.Fast);
            foreach (var h in harbors)
            {
                Task task = h.SetOrientation(TileOrientation.FaceUp, ms, i * ms);
                i++;
                list.Add(task);
            }

            await Task.WhenAll(list);
            foreach (var h in harbors)
            {
                Canvas.SetZIndex(h, 0);
            }
        }


        internal static async Task FancyDistribution(List<OldHarbor> harbors, TileCtrl centerTile)
        {
            List<Task> list = new List<Task>();
            double ms = MainPage.GetAnimationSpeed(AnimationSpeed.Fast);
            int i = 0;

            foreach (var h in harbors)
            {
                GeneralTransform gt = centerTile.TransformToVisual(h);
                Point pt = gt.TransformPoint(new Point(centerTile.ActualWidth / 2, centerTile.ActualHeight / 2));
                Task task = h.AnimateMoveTask(pt, ms, i * ms);
                i++;
                Canvas.SetZIndex(h, 1000 - i);
                list.Add(task);
            }

            await Task.WhenAll(list);
            list.Clear();
            i = 0;
            Random r = new Random(DateTime.Now.Millisecond);
            ms = MainPage.GetAnimationSpeed(AnimationSpeed.Fast);
            foreach (var h in harbors)
            {
                Task task = h.RotateTask(r.Next(1, 5) * 360, ms);
                list.Add(task);
            }
            await Task.WhenAll(list);
            list.Clear();
            i = 0;
            ms = MainPage.GetAnimationSpeed(AnimationSpeed.Fast);
            foreach (var h in harbors)
            {
                Task task = h.AnimateMoveTask(new Point(0, 0), ms, i * ms);
                i++;
                list.Add(task);
            }
            await Task.WhenAll(list);
            list.Clear();
            i = 0;
            ms = ms = MainPage.GetAnimationSpeed(AnimationSpeed.Fast);
            foreach (var h in harbors)
            {
                Task task = h.SetOrientation(TileOrientation.FaceUp, ms, i * ms);
                i++;
                list.Add(task);
            }

            await Task.WhenAll(list);
            foreach (var h in harbors)
            {
                Canvas.SetZIndex(h, 0);
            }
        }

        private void HarborGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

        }
    }
}
