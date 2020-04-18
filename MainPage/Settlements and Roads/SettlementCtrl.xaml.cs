using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class SettlementCtrl : UserControl
    {
        public SettlementCtrl()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty PlayerColorProperty = DependencyProperty.Register("PlayerColor", typeof(Color), typeof(SettlementCtrl), new PropertyMetadata(Colors.Blue, PlayerColorChanged));
        public static readonly DependencyProperty CastleColorProperty = DependencyProperty.Register("CastleColor", typeof(Color), typeof(CityCtrl), new PropertyMetadata(Colors.Black));
        public Color CastleColor
        {
           get => (Color)GetValue(CastleColorProperty);
           private set => SetValue(CastleColorProperty, value);
        }


        public Color PlayerColor
        {
            get => (Color)GetValue(PlayerColorProperty);
            set => SetValue(PlayerColorProperty, value);
        }
        private static void PlayerColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettlementCtrl depPropClass = d as SettlementCtrl;
            Color depPropValue = (Color)e.NewValue;
            depPropClass.SetPlayerColor(depPropValue);
        }
        private void SetPlayerColor(Color color)
        {
            CastleColor = StaticHelpers.BackgroundToForegroundColorDictionary[color];
        }

    }
}
