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
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{

    public sealed partial class RoadCtrl : UserControl
    {
        ITileControlCallback _callback = null;
        double _baseOpacity = 0.0;
        SolidColorBrush _brush = new SolidColorBrush(Colors.Blue);
        public Color Color
        {
            get
            {
                return _brush.Color;
            }
            set
            {
                _brush = new SolidColorBrush(value);
                _rect.Fill = _brush;

            }
        }


        public bool IsOwned
        {
            get
            {
                return LayoutRoot.Opacity == 1.0;
            }
        }

        public RoadCtrl()
        {
            this.InitializeComponent();
            if (StaticHelpers.IsInVisualStudioDesignMode)
            {
                LayoutRoot.Opacity = 1.0;
            }
        }

        public void SetCallback(ITileControlCallback cb)
        {
            _callback = cb;
        }
        private void Rectangle_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (_callback == null)
                return;

            if (!_callback.CanBuild())
            {
                e.Handled = false;
                return;
            }

            if (_baseOpacity == 1.0) return;
            LayoutRoot.Opacity = 1.0;

        }

        private void Rectangle_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!_callback.CanBuild())
            {
                e.Handled = false;
                return;
            }

            _baseOpacity = 1.0 - _baseOpacity;
            if (_baseOpacity == 1.0)
            {
                _callback.AddRoad(this);
            }
            else
            {
                _callback.RemoveRoad(this);
            }

        }

        public void ShowRoad(Color color)
        {

            this.Color = color;
            LayoutRoot.Opacity = _baseOpacity;

        }
        public void HideRoad()
        {

            _baseOpacity = 0.0;
            LayoutRoot.Opacity = _baseOpacity;
        }

        private void Rectangle_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (_callback == null) return;
            if (!_callback.CanBuild())
            {
                e.Handled = false;
                return;
            }

            LayoutRoot.Opacity = _baseOpacity;
        }
    }
}
