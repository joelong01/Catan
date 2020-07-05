using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10.Sliders
{
    [ContentProperty(Name = "Child")]
    public sealed partial class SliderCtrl : UserControl
    {
        TaskCompletionSource<object> TCS { get; set; } = new TaskCompletionSource<object>();
        public UIElement Child
        {
            get { return (UIElement)GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
        }
        public PlayerModel Player
        {
            get => (PlayerModel)GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }

        public static readonly DependencyProperty ChildProperty = DependencyProperty.Register(nameof(Child), typeof(UIElement), typeof(SliderCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(PlayerModel), typeof(SliderCtrl), new PropertyMetadata(new PlayerModel()));
        public static readonly DependencyProperty OpenProperty = DependencyProperty.Register("Open", typeof(bool), typeof(SliderCtrl), new PropertyMetadata(false, OpenChanged));
        public bool Open
        {
            get => (bool)GetValue(OpenProperty);
            set => SetValue(OpenProperty, value);
        }
        private static void OpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as SliderCtrl;
            var depPropValue = (bool)e.NewValue;
            depPropClass?.SetOpen(depPropValue);
        }
        private void SetOpen(bool value)
        {

        }


        public SliderCtrl()
        {
            this.InitializeComponent();
        }

        private void OnOpenClose(object sender, RoutedEventArgs e)
        {
            Open = !Open;
        }

        public void Close()
        {
            Open = false;
            TCS.TrySetResult(null);
            TCS = new TaskCompletionSource<object>();
        }

        public async Task ShowAsync()
        {
            Open = true;
            await TCS.Task;
        }
    }
}