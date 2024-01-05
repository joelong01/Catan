using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class EntryGateCtrl : UserControl
    {
        public EntryGateCtrl()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        public static readonly DependencyProperty StrokeColorProperty = DependencyProperty.Register("StrokeColor", typeof(SolidColorBrush), typeof(EntryGateCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty FillColorProperty = DependencyProperty.Register("FillColor", typeof(SolidColorBrush), typeof(DiceCtrl), new PropertyMetadata(null));
         public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register("StrokeThickness", typeof(double), typeof(EntryGateCtrl), new PropertyMetadata(3.0));
        public double StrokeThickness
        {
            get => ( double )GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }
        public SolidColorBrush FillColor
        {
            get => ( SolidColorBrush )GetValue(FillColorProperty);
            set => SetValue(FillColorProperty, value);
        }
        public SolidColorBrush StrokeColor
        {
            get => ( SolidColorBrush )GetValue(StrokeColorProperty);
            set => SetValue(StrokeColorProperty, value);
        }
 

    }
}
