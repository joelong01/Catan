using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class ResourceCardCtrl : UserControl
    {
        public ResourceCardCtrl()
        {
            this.InitializeComponent();

        }

        public static readonly DependencyProperty ResourceTypeProperty = DependencyProperty.Register("ResourceType", typeof(ResourceType), typeof(ResourceCardCtrl), new PropertyMetadata(ResourceType.Sheep, ResourceTypeChanged));
        public static readonly DependencyProperty CountProperty = DependencyProperty.Register("Count", typeof(int), typeof(ResourceCardCtrl), new PropertyMetadata(0, CountChanged));
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(TileOrientation), typeof(ResourceCardCtrl), new PropertyMetadata(TileOrientation.FaceDown, OrientationChanged));
        public TileOrientation Orientation
        {
            get => (TileOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }
        private static void OrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ResourceCardCtrl depPropClass = d as ResourceCardCtrl;
            TileOrientation depPropValue = (TileOrientation)e.NewValue;
            depPropClass.SetOrientation(depPropValue);
        }
        private void SetOrientation(TileOrientation value)
        {
            SetOrientationAsync(value);
        }

        public int Count
        {
            get => (int)GetValue(CountProperty);
            set => SetValue(CountProperty, value);
        }
        private static void CountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ResourceCardCtrl depPropClass = d as ResourceCardCtrl;
            int depPropValue = (int)e.NewValue;
            depPropClass.SetCount(depPropValue);
        }
        private void SetCount(int value)
        {
            _txtCount.Text = value.ToString();
        }

        public ResourceType ResourceType
        {
            get => (ResourceType)GetValue(ResourceTypeProperty);
            set => SetValue(ResourceTypeProperty, value);
        }
        private static void ResourceTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ResourceCardCtrl depPropClass = d as ResourceCardCtrl;
            ResourceType depPropValue = (ResourceType)e.NewValue;
            depPropClass.SetResourceType(depPropValue);
        }
        private void SetResourceType(ResourceType value)
        {
            string bitmapPath = "ms-appx:Assets/back.jpg";

            switch (value)
            {
                case ResourceType.Sheep:
                    bitmapPath = "ms-appx:Assets/SquareImages/sheep.png";
                    break;
                case ResourceType.Wood:
                    bitmapPath = "ms-appx:Assets/SquareImages/wood.png";
                    break;
                case ResourceType.Ore:
                    bitmapPath = "ms-appx:Assets/SquareImages/ore.png";
                    break;
                case ResourceType.Wheat:
                    bitmapPath = "ms-appx:Assets/SquareImages/wheat.png";
                    break;
                case ResourceType.Brick:
                    bitmapPath = "ms-appx:Assets/SquareImages/brick.png";
                    break;
                case ResourceType.Desert:
                case ResourceType.None:
                    bitmapPath = "ms-appx:Assets/SquareImages/back.png";
                    break;
                case ResourceType.GoldMine:
                    bitmapPath = "ms-appx:Assets/SquareImages/gold.png";
                    break;

                default:
                    break;

            }
            BitmapImage bitmapImage = new BitmapImage(new Uri(bitmapPath, UriKind.RelativeOrAbsolute));
            _imgFront.ImageSource = bitmapImage;
            _imgFront.Stretch = Stretch.UniformToFill;

        }

        public void SetOrientationAsync(TileOrientation orientation, double startAfter = 0)
        {
            bool flipToFaceUp = (orientation == TileOrientation.FaceUp) ? true : false;
            StaticHelpers.SetupFlipAnimation(flipToFaceUp, _daFlipBackCard, _daFlipFrontCard, MainPage.GetAnimationSpeed(AnimationSpeed.Normal), startAfter);
            _sbFlipTile.Begin();
        }

    }
}
