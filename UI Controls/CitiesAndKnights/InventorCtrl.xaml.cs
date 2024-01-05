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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class InventorCtrl : UserControl
    {
        public InventorCtrl()
        {
            this.InitializeComponent();
        }
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register("Stroke", typeof(SolidColorBrush), typeof(InventorCtrl), new PropertyMetadata(new SolidColorBrush(Colors.White)));
        public SolidColorBrush Stroke
        {
            get => ( SolidColorBrush )GetValue(StrokeProperty);
            set => SetValue(StrokeProperty, value);
        }

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(double), typeof(InventorCtrl), new PropertyMetadata(30.0));
        public double StrokeThickness
        {
            get => ( double )GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }
    }
}
