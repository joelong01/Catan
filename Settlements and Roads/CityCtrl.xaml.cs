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
        public static readonly DependencyProperty FillColorProperty = DependencyProperty.Register("Background", typeof(Color), typeof(CityCtrl), new PropertyMetadata(Colors.Blue, FillColorChanged));
        public static readonly DependencyProperty BorderColorProperty = DependencyProperty.Register("BorderColor", typeof(Color), typeof(CityCtrl), new PropertyMetadata(Colors.Black, BorderColorChanged));
        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
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

        public Color FillColor
        {
            get { return (Color)GetValue(FillColorProperty); }
            set { SetValue(FillColorProperty, value); }
        }
        private static void FillColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CityCtrl depPropClass = d as CityCtrl;
            Color depPropValue = (Color)e.NewValue;
            depPropClass.SetFillColor(depPropValue);
        }
        private void SetFillColor(Color color)
        {
            BorderColor = StaticHelpers.BackgroundToForegroundColorDictionary[color];
        }
    }
}
