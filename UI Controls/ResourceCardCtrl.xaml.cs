using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        public static readonly DependencyProperty HarborTypeProperty = DependencyProperty.Register("HarborType", typeof(HarborType), typeof(ResourceCardCtrl), new PropertyMetadata(HarborType.None, HarborTypeChanged));
        public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register("Owner", typeof(PlayerModel), typeof(ResourceCardCtrl), new PropertyMetadata(null, OwnerChanged));
        public static readonly DependencyProperty HarborVisibilityProperty = DependencyProperty.Register("HarborVisibility", typeof(Visibility), typeof(ResourceCardCtrl), new PropertyMetadata(Visibility.Collapsed, HarborVisibilityChanged));
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
            if (e.OldItems!=null && e.OldItems.Count == 1)
            {
                HarborType type = ((Harbor)e.OldItems[0]).HarborType;
                if (StaticHelpers.HarborTypeToResourceType(type) == ResourceType)
                {
                    // Debug.WriteLine($" =======> Making {type} Collapsed for ResourceType {ResourceType}");
                    HarborVisibility = Visibility.Collapsed;
                }
            }
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
                    HarborType = HarborType.Sheep;
                    break;
                case ResourceType.Wood:
                    bitmapPath = "ms-appx:Assets/SquareImages/wood.png";
                    HarborType = HarborType.Wood;
                    break;
                case ResourceType.Ore:
                    bitmapPath = "ms-appx:Assets/SquareImages/ore.png";
                    HarborType = HarborType.Ore;
                    break;
                case ResourceType.Wheat:
                    bitmapPath = "ms-appx:Assets/SquareImages/wheat.png";
                    HarborType = HarborType.Wheat;
                    break;
                case ResourceType.Brick:
                    bitmapPath = "ms-appx:Assets/SquareImages/brick.png";
                    HarborType = HarborType.Brick;
                    break;
                case ResourceType.Desert:
                case ResourceType.None:
                    bitmapPath = "ms-appx:Assets/SquareImages/back.png";
                    HarborType = HarborType.None;
                    break;
                case ResourceType.GoldMine:
                    bitmapPath = "ms-appx:Assets/SquareImages/gold.png";
                    HarborType = HarborType.None;
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
