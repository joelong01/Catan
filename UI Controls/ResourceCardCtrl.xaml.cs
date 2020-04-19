using System;
using Catan.Proxy;
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

        public static readonly DependencyProperty ResourceTypeProperty = DependencyProperty.Register("ResourceType", typeof(ResourceType), typeof(ResourceCardCtrl), new PropertyMetadata(ResourceType.None, ResourceTypeChanged));
        public static readonly DependencyProperty CountProperty = DependencyProperty.Register("Count", typeof(int), typeof(ResourceCardCtrl), new PropertyMetadata(0, CountChanged));
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(TileOrientation), typeof(ResourceCardCtrl), new PropertyMetadata(TileOrientation.FaceDown, OrientationChanged));
        public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register("ReadOnly", typeof(bool), typeof(ResourceCardCtrl), new PropertyMetadata(false, ReadOnlyChanged));
        public static readonly DependencyProperty DevCardTypeProperty = DependencyProperty.Register("DevCardType", typeof(DevCardType), typeof(ResourceCardCtrl), new PropertyMetadata(DevCardType.Unknown, DevCardTypeChanged));
        public static readonly DependencyProperty HarborTypeProperty = DependencyProperty.Register("HarborType", typeof(HarborType), typeof(ResourceCardCtrl), new PropertyMetadata(HarborType.None, HarborTypeChanged));
        public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register("Owner", typeof(PlayerModel), typeof(ResourceCardCtrl), new PropertyMetadata(null, OwnerChanged));
        public static readonly DependencyProperty HarborVisibilityProperty = DependencyProperty.Register("HarborVisibility", typeof(Visibility), typeof(ResourceCardCtrl), new PropertyMetadata(Visibility.Collapsed, HarborVisibilityChanged));
        public static readonly DependencyProperty CountVisibleProperty = DependencyProperty.Register("CountVisible", typeof(Visibility), typeof(ResourceCardCtrl), new PropertyMetadata(Visibility.Visible));
        public Visibility CountVisible
        {
            get => (Visibility)GetValue(CountVisibleProperty);
            set => SetValue(CountVisibleProperty, value);
        }
        public Visibility HarborVisibility
        {
            get => (Visibility)GetValue(HarborVisibilityProperty);
            set => SetValue(HarborVisibilityProperty, value);
        }
        private static void HarborVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as ResourceCardCtrl;
            var depPropValue = (Visibility)e.NewValue;
            depPropClass?.SetHarborVisibility(depPropValue);
        }
        private void SetHarborVisibility(Visibility value)
        {
            //  Debug.WriteLine($"{Owner.ColorAsString}: ResourceType: {ResourceType} HarborType: {HarborType} Visibility: {value}");
        }

        public PlayerModel Owner
        {
            get => (PlayerModel)GetValue(OwnerProperty);
            set => SetValue(OwnerProperty, value);
        }
        private static void OwnerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as ResourceCardCtrl;
            var newOwner = (PlayerModel)e.NewValue;
            var oldOwner = (PlayerModel)e.OldValue;
            depPropClass?.SetOwner(oldOwner, newOwner);
        }
        private void SetOwner(PlayerModel oldOwner, PlayerModel newOwner)
        {
            if (oldOwner != null)
            {
                oldOwner.GameData.OwnedHarbors.CollectionChanged -= OwnedHarbors_CollectionChanged;
            }

            if (newOwner != null)
            {
                newOwner.GameData.OwnedHarbors.CollectionChanged += OwnedHarbors_CollectionChanged;
            }
        }
        public DevCardType DevCardType
        {
            get => (DevCardType)GetValue(DevCardTypeProperty);
            set => SetValue(DevCardTypeProperty, value);
        }
        private static void DevCardTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as ResourceCardCtrl;
            var depPropValue = (DevCardType)e.NewValue;
            depPropClass?.SetDevCardType(depPropValue);
        }

        private void SetDevCardType(DevCardType value)
        {
            if (value == DevCardType.Unknown) return;
            string bitmapPath = "ms-appx:Assets/back.jpg";
            // VictoryPoint, Knight, YearOfPlenty, RoadBuilding, Monopoly
            switch (value)
            {

                case DevCardType.VictoryPoint:
                    bitmapPath = "ms-appx:Assets/DevCards/VictoryPoint.jpg";
                    break;
                case DevCardType.Knight:
                    bitmapPath = "ms-appx:Assets/DevCards/knight.jpg";
                    break;
                case DevCardType.YearOfPlenty:
                    bitmapPath = "ms-appx:Assets/DevCards/YearOfPlenty.jpg";
                    break;
                case DevCardType.RoadBuilding:
                    bitmapPath = "ms-appx:Assets/DevCards/RoadBuilding.jpg";
                    break;
                case DevCardType.Monopoly:
                    bitmapPath = "ms-appx:Assets/DevCards/Monopoly.jpg";
                    break;
                case DevCardType.Back:
                    bitmapPath = "ms-appx:Assets/DevCards/back.jpg";
                    break;
                default:
                    break;

            }
            BitmapImage bitmapImage = new BitmapImage(new Uri(bitmapPath, UriKind.RelativeOrAbsolute));
            _imgFront.ImageSource = bitmapImage;
            _imgFront.Stretch = Stretch.UniformToFill;
        }


        public bool ReadOnly
        {
            get => (bool)GetValue(ReadOnlyProperty);
            set => SetValue(ReadOnlyProperty, value);
        }
        private static void ReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as ResourceCardCtrl;
            var depPropValue = (bool)e.NewValue;
            depPropClass?.SetReadOnly(depPropValue);
        }
        private void SetReadOnly(bool value)
        {

            _txtCount.IsReadOnly = value;
        }
        //
        //  should only be set by ResourceType
        //
        public HarborType HarborType
        {
            get => (HarborType)GetValue(HarborTypeProperty);
            private set => SetValue(HarborTypeProperty, value);
        }
        private static void HarborTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as ResourceCardCtrl;
            var depPropValue = (HarborType)e.NewValue;
            depPropClass?.SetHarborType(depPropValue);
        }
        private void SetHarborType(HarborType harborType)
        {

        }


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
            get
            {
                try
                {
                    return (int)GetValue(CountProperty);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                try
                {
                    SetValue(CountProperty, value);
                }
                catch
                {
                    SetValue(CountProperty, 0);
                }
            }

        }
        private static void CountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ResourceCardCtrl depPropClass = d as ResourceCardCtrl;
            int depPropValue = (int)e.NewValue;
            depPropClass.SetCount(depPropValue);
        }
        private void SetCount(int value)
        {

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
            if (value == ResourceType.None) return;
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
            StaticHelpers.SetupFlipAnimation(flipToFaceUp, _daFlipBackCard, _daFlipFrontCard, 100, startAfter);
            _sbFlipTile.Begin();
        }

        private void Text_GotFocus(object sender, RoutedEventArgs e)
        {
            ((TextBox)sender)?.SelectAll();
        }
        //
        //  Works like this:  Onwer gets set by XAML in binding
        //  ResourceType gets set in XAML by binding
        //  ResourceType updates HarborType
        //  we suscribe to CollectionChanged so that whenever a Harbor changes Owner, we can update the visibilty flag of the Harbor
        //
        //  

        private void OwnedHarbors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //  left here ofr debugging purposes
            //
            //  ObservableCollection<Harbor> col = (ObservableCollection<Harbor>)sender;


            if (e.NewItems != null && e.NewItems.Count == 1)
            {
                HarborType type = ((Harbor)e.NewItems[0]).HarborType;
                if (StaticHelpers.HarborTypeToResourceType(type) == ResourceType)
                {
                    // Debug.WriteLine($" =======> Making {type} Visible for ResourceType {ResourceType}");
                    HarborVisibility = Visibility.Visible;
                }
            }
            if (e.OldItems != null && e.OldItems.Count == 1)
            {
                HarborType type = ((Harbor)e.OldItems[0]).HarborType;
                if (StaticHelpers.HarborTypeToResourceType(type) == ResourceType)
                {
                    // Debug.WriteLine($" =======> Making {type} Collapsed for ResourceType {ResourceType}");
                    HarborVisibility = Visibility.Collapsed;
                }
            }
        }
    }
}
