using System;
using System.Collections.Generic;
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
    public sealed partial class BoardMeasurementCtrl : UserControl
    {
        public static readonly DependencyProperty PipCountProperty = DependencyProperty.Register("PipCount", typeof(TradeResources), typeof(BoardMeasurementCtrl), new PropertyMetadata(new TradeResources()));
        public TradeResources PipCount
        {
            get => (TradeResources)GetValue(PipCountProperty);
            set => SetValue(PipCountProperty, value);
        }

        public BoardMeasurementCtrl()
        {
            this.InitializeComponent();
        }
    }
}
