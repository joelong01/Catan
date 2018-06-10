using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class SettlementUi : UserControl
    {
        public SettlementUi()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty CircleColorProperty = DependencyProperty.Register("CircleFillColor", typeof(Color), typeof(SettlementUi), new PropertyMetadata(Colors.Green, CircleFillColorChanged));
        public static readonly DependencyProperty CastleColorProperty = DependencyProperty.Register("CastleColor", typeof(Color), typeof(SettlementUi), new PropertyMetadata(Colors.Purple, CastleColorChanged));
        public Color CastleColor
        {
            get { return (Color)GetValue(CastleColorProperty); }
            set { SetValue(CastleColorProperty, value); }
        }
        private static void CastleColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettlementUi depPropClass = d as SettlementUi;
            Color depPropValue = (Color)e.NewValue;
            depPropClass.SetCastleColor(depPropValue);
        }
        private void SetCastleColor(Color color)
        {

        }

        public Color CircleFillColor
        {
            get { return (Color)GetValue(CircleColorProperty); }
            set { SetValue(CircleColorProperty, value); }
        }
        private static void CircleFillColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettlementUi depPropClass = d as SettlementUi;
            Color depPropValue = (Color)e.NewValue;
            depPropClass.SetCircleFillColorChanged(depPropValue);
        }
        private void SetCircleFillColorChanged(Color color)
        {
            CastleColor = StaticHelpers.BackgroundToForegroundColorDictionary[color];
        }

    }
}
