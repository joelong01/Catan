using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class SailboatCtrl : UserControl
    {
        public SailboatCtrl()
        {
            this.InitializeComponent();
        }
        public static readonly DependencyProperty CircleColorProperty = DependencyProperty.Register("CircleColor", typeof(Color), typeof(SailboatCtrl), new PropertyMetadata("White", CircleColorChanged));
        public Color CircleColor
        {
            get => (Color)GetValue(CircleColorProperty);
            set => SetValue(CircleColorProperty, value);
        }
        private static void CircleColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as SailboatCtrl;
            var depPropValue = (Color)e.NewValue;
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

    }
}
