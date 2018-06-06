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

        public static readonly DependencyProperty FillColorProperty = DependencyProperty.Register("Background", typeof(Color), typeof(SettlementUi), new PropertyMetadata(Colors.Blue, FillColorChanged));
        public static readonly DependencyProperty BorderColorProperty = DependencyProperty.Register("BorderColor", typeof(Color), typeof(SettlementUi), new PropertyMetadata(Colors.Black, BorderColorChanged));
        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }
        private static void BorderColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettlementUi depPropClass = d as SettlementUi;
            Color depPropValue = (Color)e.NewValue;
            depPropClass.SetBorderColor(depPropValue);
        }
        private void SetBorderColor(Color color)
        {

        }

        public Color FillColor
        {
            get { return (Color)GetValue(FillColorProperty); }
            set { SetValue(FillColorProperty, value); }
        }
        private static void FillColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettlementUi depPropClass = d as SettlementUi;
            Color depPropValue = (Color)e.NewValue;
            depPropClass.SetFillColor(depPropValue);
        }
        private void SetFillColor(Color color)
        {
            BorderColor = StaticHelpers.BackgroundToForegroundColorDictionary[color];
        }

    }
}
