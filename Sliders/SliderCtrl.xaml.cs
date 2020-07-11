using System;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10.Sliders
{
    public enum SlideDirection { Up, Down, Right, Left };

    [ContentProperty(Name = "Child")]
    public sealed partial class SliderCtrl : UserControl
    {
        #region Delegates + Fields + Events + Enums

        public static readonly DependencyProperty ChildProperty = DependencyProperty.Register(nameof(Child), typeof(UIElement), typeof(SliderCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty OpenProperty = DependencyProperty.Register("Open", typeof(bool), typeof(SliderCtrl), new PropertyMetadata(true, OpenChanged));
        public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(PlayerModel), typeof(SliderCtrl), new PropertyMetadata(new PlayerModel()));
        public static readonly DependencyProperty SlideDirectionProperty = DependencyProperty.Register("SlideDirection", typeof(SlideDirection), typeof(SliderCtrl), new PropertyMetadata(SlideDirection.Right, SlideDirectionChanged));
        public static readonly DependencyProperty CaptionProperty = DependencyProperty.Register("Caption", typeof(string), typeof(SliderCtrl), new PropertyMetadata(""));
        public string Caption
        {
            get => (string)GetValue(CaptionProperty);
            set => SetValue(CaptionProperty, value);
        }

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public UIElement Child
        {
            get { return (UIElement)GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
        }

        public bool Open
        {
            get => (bool)GetValue(OpenProperty);
            set => SetValue(OpenProperty, value);
        }

        public PlayerModel Player
        {
            get => (PlayerModel)GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }

        public SlideDirection SlideDirection
        {
            get => (SlideDirection)GetValue(SlideDirectionProperty);
            set => SetValue(SlideDirectionProperty, value);
        }

        private TaskCompletionSource<object> TCS { get; set; } = new TaskCompletionSource<object>();

        #endregion Properties

        #region Constructors + Destructors

        public SliderCtrl()
        {
            this.InitializeComponent();
        }

        #endregion Constructors + Destructors

        #region Methods

        public void Close()
        {
            Open = false;
            TCS.TrySetResult(null);
            TCS = new TaskCompletionSource<object>();
        }

        public Visibility MatchesSlideDirection(SlideDirection direction, SlideDirection match)
        {
            return match == direction ? Visibility.Visible : Visibility.Collapsed;
        }

        public async Task ShowAsync()
        {
            Open = true;
            await TCS.Task;
        }

        private static void OpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as SliderCtrl;
            var depPropValue = (bool)e.NewValue;
            depPropClass?.SetOpen(depPropValue);
        }

        private static void SlideDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as SliderCtrl;
            var depPropValue = (SlideDirection)e.NewValue;
            depPropClass?.SetSlideDirection(depPropValue);
        }

        private double ClosedPosition(double widthOrHeight)
        {
            double ret = 0;

            if (widthOrHeight == Double.NaN) return 0;
            double rectWidth = 25;
            switch (SlideDirection)
            {
                case SlideDirection.Left:
                case SlideDirection.Up:
                    ret = widthOrHeight - rectWidth;
                    break;

                case SlideDirection.Right:
                case SlideDirection.Down:
                    ret = rectWidth - widthOrHeight;
                    break;

                default:
                    break;
            }
            return ret;
        }

        private void OnOpenClose(object sender, RoutedEventArgs e)
        {
            Open = !Open;
        }

        private void SetOpen(bool opened)
        {
            if (this.Width == Double.NaN || this.Height == Double.NaN) return;
            switch (SlideDirection)
            {
                case SlideDirection.Up:
                case SlideDirection.Down:
                    if (opened)
                    {
                        SbOpenUpDown.Begin();
                    }
                    else
                    {
                        SbCloseUpDown.Begin();
                    }
                    break;

                case SlideDirection.Left:
                case SlideDirection.Right:
                    if (opened)
                    {
                        SbOpenRightLeft.Begin();
                    }
                    else
                    {
                        SbCloseRightLeft.Begin();
                    }
                    break;

                default:
                    break;
            }
        }

        //
        //  make the appropriate row/columns disappear based on the direction the control opens
        private void SetSlideDirection(SlideDirection direction)
        {
            double widthOrHeight = 25;
            switch (direction)
            {
                case SlideDirection.Up:
                    LayoutRoot.RowDefinitions[0].Height = new GridLength(widthOrHeight);
                    LayoutRoot.RowDefinitions[2].Height = new GridLength(0);
                    LayoutRoot.ColumnDefinitions[0].Width = new GridLength(0);
                    LayoutRoot.ColumnDefinitions[2].Width = new GridLength(0);
                    break;

                case SlideDirection.Down:
                    LayoutRoot.RowDefinitions[2].Height = new GridLength(widthOrHeight);
                    LayoutRoot.RowDefinitions[0].Height = new GridLength(0);
                    LayoutRoot.ColumnDefinitions[0].Width = new GridLength(0);
                    LayoutRoot.ColumnDefinitions[2].Width = new GridLength(0);
                    break;

                case SlideDirection.Right:
                    LayoutRoot.ColumnDefinitions[2].Width = new GridLength(widthOrHeight);
                    LayoutRoot.RowDefinitions[2].Height = new GridLength(0);
                    LayoutRoot.RowDefinitions[0].Height = new GridLength(0);
                    LayoutRoot.ColumnDefinitions[0].Width = new GridLength(0);
                    break;

                case SlideDirection.Left:
                    LayoutRoot.ColumnDefinitions[0].Width = new GridLength(widthOrHeight);
                    LayoutRoot.RowDefinitions[2].Height = new GridLength(0);
                    LayoutRoot.RowDefinitions[0].Height = new GridLength(0);
                    LayoutRoot.ColumnDefinitions[2].Width = new GridLength(0);
                    break;

                default:
                    break;
            }
        }

        #endregion Methods
    }
}