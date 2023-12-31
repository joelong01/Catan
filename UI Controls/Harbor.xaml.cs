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
        #region Delegates + Fields + Events + Enums

        public event PropertyChangedEventHandler PropertyChanged;

        public static readonly DependencyProperty HarborTypeProperty = DependencyProperty.Register("HarborType", typeof(HarborType), typeof(Harbor), new PropertyMetadata(HarborType.Brick));
        public static readonly DependencyProperty IndexProperty = DependencyProperty.Register("Index", typeof(int), typeof(Harbor), new PropertyMetadata(0));
        public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register("Owner", typeof(PlayerModel), typeof(Harbor), new PropertyMetadata(PlayerModel.DefaultPlayer, OwnerChanged));
        private readonly SolidColorBrush _blackBrush = CatanColors.GetResourceBrush("Black", Colors.Black);

        private readonly string[] _savePropName = new string[] { "HarborLocation" };

        private readonly SolidColorBrush _whiteBrush = CatanColors.GetResourceBrush("White", Colors.White);

        private HarborLocation _location = HarborLocation.None;

        private TileOrientation _orientation = TileOrientation.FaceDown;

        private bool _useClassic = true;

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public FrameworkElement AnimationObject => _backGrid;

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

        //
        //  for information purposes only - no side affects
        public HarborLocation HarborLocation
        {
            get => _location;

            set => _location = value;
        }

        public double HarborScale { get; set; } = 1.0;

        public HarborType HarborType
        {
            get => (HarborType)GetValue(HarborTypeProperty);
            set => SetValue(HarborTypeProperty, value);
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

        public int Index
        {
            get => (int)GetValue(IndexProperty);
            set => SetValue(IndexProperty, value);
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

        public PlayerModel Owner
        {
            get => (PlayerModel)GetValue(OwnerProperty);
            set => SetValue(OwnerProperty, value);
        }

        public double RotateImage
        {
            get => _gridFrontTransform.Rotation;
            set => _gridFrontTransform.Rotation = value;
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

        public double TextSize
        {
            get => _text.FontSize;
            set => _text.FontSize = value;
        }

        public int TileIndex { get; set; } = 0;

        public CompositeTransform Transform => _gridTransform;

        public bool UseClassic
        {
            get => _useClassic;

            set => _useClassic = value;
        }

        #endregion Properties

        #region Constructors + Destructors

        public Harbor()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        #endregion Constructors + Destructors

        #region Methods

        public static MenuFlyoutSubItem BuildResourceTradeMenu(PlayerModel owner, int cost, ResourceType give)
        {
            //
            // this is the only state you can do a meritime trade in
            if (MainPage.GameController.CurrentGameState != GameState.WaitingForNext) return null;

            //
            //  can't trade if you don't have enough resources
            if (owner.GameData.Resources.CurrentResources.GetCount(give) < cost) return null;

            //
            //  local function to handle the menu click event we create below
            //
            async void LocalHandleMenuClick(object sender, RoutedEventArgs e)
            {
                MenuFlyoutItem item = sender as MenuFlyoutItem;

                (ResourceType Get, ResourceType Give) = (ValueTuple<ResourceType, ResourceType>)item.Tag;

                TradeResources tr = new TradeResources();
                tr.AddResource(Get, 1);
                tr.AddResource(Give, -cost);

                await ResourceExchangeLog.PostLog(MainPage.GameController, tr);
            }

            //
            //  build the menu
            MenuFlyoutSubItem subItem = subItem = new MenuFlyoutSubItem()
            {
                Text = $"{cost} {give} for 1 ..."
            };
            foreach (ResourceType get in Enum.GetValues(typeof(ResourceType)))
            {
                if (TradeResources.GrantableResources(get) && get != ResourceType.GoldMine && get != give)
                {
                    //
                    //  use the tag to store the resources needed to build the TradeResources in the handler
                    MenuFlyoutItem item = new MenuFlyoutItem()
                    {
                        Text = $"{get}",
                        Tag = (get, give)
                    };
                    subItem.Items.Add(item);
                    item.Click += LocalHandleMenuClick;
                }
            }
            return subItem;
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

        public void Reset()
        {
            if (Owner != null)
            {
                Owner.GameData.RemoveOwnedHarbor(this);
                Owner = null;
            }
        }

        public Task RotateTask(double angle, double ms)
        {
            _daRotate.To += angle;
            _daRotate.Duration = TimeSpan.FromMilliseconds(ms);
            return _sbRotate.ToTask();
        }

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

        //
        //  5/5/2020: the Orientation and the _orientation were out of sync.  So we couldn't check the value and return if it was set correctly.
        //
        public void SetOrientationAsync(TileOrientation orientation, double animationDuration = double.MaxValue)
        {
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

        public override string ToString()
        {
            return string.Format($"Index={Index} HarborLocation={_location} Type={HarborType}");
        }

        private static void OwnerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as Harbor;
            var newOwner = (PlayerModel)e.NewValue;
            var oldOwner = (PlayerModel)e.OldValue;
            depPropClass?.SetOwner(oldOwner, newOwner);
        }

        private void HarborGrid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnHarborRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (Owner == null) return;

            TradeMenu.Items.Clear();

            if (this.HarborType == HarborType.ThreeForOne)
            {
                //
                //  if it is 3:1, find all the resource types with more than 3 resources and add them to a trade menu
                foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
                {
                    var menu = BuildResourceTradeMenu(this.Owner, 3, resource);
                    if (menu != null)
                    {
                        TradeMenu.Items.Add(menu);
                    }
                }
            }
            else
            {
                // if it is 2:1, add a menu only for the harbor resource type
                ResourceType resource = StaticHelpers.HarborTypeToResourceType(this.HarborType);
                var menu = BuildResourceTradeMenu(this.Owner, 2, resource);
                if (menu != null)
                {
                    TradeMenu.Items.Add(menu);
                }
            }
            //
            //  if they have a menu to display, show it
            if (TradeMenu.Items.Count > 0)
            {
                TradeMenu.ShowAt(this, new Point(0, 0));
            }
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

        #endregion Methods
    }
}