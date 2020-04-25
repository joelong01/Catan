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
            this.InitializeComponent();
        }
        public static readonly DependencyProperty PlayerColorProperty = DependencyProperty.Register("PlayerColor", typeof(SolidColorBrush), typeof(CityCtrl), new PropertyMetadata(CatanColors.GetResourceBrush("Blue", Colors.Blue), PlayerColorChanged));
        public static readonly DependencyProperty CastleColorProperty = DependencyProperty.Register("CastleColor", typeof(SolidColorBrush), typeof(CityCtrl), new PropertyMetadata(CatanColors.GetResourceBrush("Black", Colors.Black)));
        public SolidColorBrush CastleColor
        {
            get => (SolidColorBrush)GetValue(CastleColorProperty);
            set => SetValue(CastleColorProperty, value);
        }

        public SolidColorBrush PlayerColor
        {
            get => (SolidColorBrush)GetValue(PlayerColorProperty);
            set => SetValue(PlayerColorProperty, value);
        }
        private static void PlayerColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CityCtrl depPropClass = d as CityCtrl;
            SolidColorBrush depPropValue = (SolidColorBrush)e.NewValue;
            depPropClass.SetFillColor(depPropValue);
        }
        private void SetFillColor(SolidColorBrush foreground)
        {

            CastleColor = CatanColors.GetForegroundBrush(foreground);
        }
    }
}
