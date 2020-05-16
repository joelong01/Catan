using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CityCtrl : UserControl
    {
        public CityCtrl()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }
        public static readonly DependencyProperty CircleColorProperty = DependencyProperty.Register("CircleColor", typeof(Brush), typeof(CityCtrl), new PropertyMetadata(new SolidColorBrush(Colors.HotPink)));
        public static readonly DependencyProperty CastleColorProperty = DependencyProperty.Register("CastleColor", typeof(Brush), typeof(CityCtrl), new PropertyMetadata(new SolidColorBrush(Colors.HotPink)));
        public Brush CastleColor
        {
            get => (Brush)GetValue(CastleColorProperty);
            set => SetValue(CastleColorProperty, value);
        }

        public Brush CircleColor
        {
            get => (Brush)GetValue(CircleColorProperty);
            set => SetValue(CircleColorProperty, value);
        }
       
    }
}
