using System;

using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class SailboatCtrl : UserControl
    {
        #region Methods

        private static void CircleColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SailboatCtrl depPropClass = d as SailboatCtrl;
            Color depPropValue = (Color)e.NewValue;
            depPropClass?.SetCircleColor(depPropValue);
        }

        private void SetCircleColor(Color color)
        {
            Uri imgUri = null;
            if (color == Colors.White)
            {
                imgUri = new Uri("ms-appx:///Assets/sailboat_dark.png", UriKind.RelativeOrAbsolute);
            }
            else
            {
                imgUri = new Uri("ms-appx:///Assets/sailboat_light.png", UriKind.RelativeOrAbsolute);
            }

            BitmapImage bitmapImage = new BitmapImage(imgUri);
            ImageBrush_Sailboat.ImageSource = bitmapImage;
        }

        #endregion Methods

        #region Fields

        public static readonly DependencyProperty CircleColorProperty = DependencyProperty.Register("CircleColor", typeof(Color), typeof(SailboatCtrl), new PropertyMetadata(Colors.White, CircleColorChanged));

        #endregion Fields

        #region Constructors

        public SailboatCtrl()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        public Color CircleColor
        {
            get => (Color)GetValue(CircleColorProperty);
            set => SetValue(CircleColorProperty, value);
        }

        #endregion Properties
    }
}
