using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CityCtrl : Page
    {
        public CityCtrl()
        {
            this.InitializeComponent();
        }
        public static readonly DependencyProperty CircleFillColorProperty = DependencyProperty.Register("CircleFillColor", typeof(Color), typeof(CityCtrl), new PropertyMetadata(Colors.Blue, CircleFillColorChanged));
        public static readonly DependencyProperty CastleColorProperty = DependencyProperty.Register("CastleColor", typeof(Color), typeof(CityCtrl), new PropertyMetadata(Colors.Black, BorderColorChanged));
        public Color CastleColor
        {
            get { return (Color)GetValue(CastleColorProperty); }
            set { SetValue(CastleColorProperty, value); }
        }
        private static void BorderColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CityCtrl depPropClass = d as CityCtrl;
            Color depPropValue = (Color)e.NewValue;
            depPropClass.SetBorderColor(depPropValue);
        }
        private void SetBorderColor(Color color)
        {

        }

        public Color CircleFillColor
        {
            get { return (Color)GetValue(CircleFillColorProperty); }
            set { SetValue(CircleFillColorProperty, value); }
        }
        private static void CircleFillColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CityCtrl depPropClass = d as CityCtrl;
            Color depPropValue = (Color)e.NewValue;
            depPropClass.SetFillColor(depPropValue);
        }
        private void SetFillColor(Color color)
        {
            CastleColor = StaticHelpers.BackgroundToForegroundColorDictionary[color];
        }
    }
}
