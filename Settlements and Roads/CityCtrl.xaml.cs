using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
        public static readonly DependencyProperty PlayerColorProperty = DependencyProperty.Register("PlayerColor", typeof(Color), typeof(CityCtrl), new PropertyMetadata(Colors.Blue, PlayerColorChanged));
        public static readonly DependencyProperty CastleColorProperty = DependencyProperty.Register("CastleColor", typeof(Color), typeof(CityCtrl), new PropertyMetadata(Colors.Black));
        public Color CastleColor
        {
            get => (Color)GetValue(CastleColorProperty);
            set => SetValue(CastleColorProperty, value);
        }

        public Color PlayerColor
        {
            get => (Color)GetValue(PlayerColorProperty);
            set => SetValue(PlayerColorProperty, value);
        }
        private static void PlayerColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
