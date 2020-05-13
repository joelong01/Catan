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
    public sealed partial class DiceCtrl : UserControl
    {
        private string[] diceNames = new string[] {"one", "two", "three", "four", "five", "six"};

        public DiceCtrl()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register("Number", typeof(int), typeof(DiceCtrl), new PropertyMetadata(2));
        public static readonly DependencyProperty FillColorProperty = DependencyProperty.Register("FillColor", typeof(SolidColorBrush), typeof(DiceCtrl), new PropertyMetadata(new SolidColorBrush(Colors.HotPink)));
        public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register("Thickness", typeof(Thickness), typeof(DiceCtrl), new PropertyMetadata(new Thickness(5)));
        public Thickness Thickness
        {
            get => (Thickness)GetValue(ThicknessProperty);
            set => SetValue(ThicknessProperty, value);
        }
        public SolidColorBrush FillColor
        {
            get => (SolidColorBrush)GetValue(FillColorProperty);
            set => SetValue(FillColorProperty, value);
        }
        public int Number
        {
            get => (int)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }
        

    }
}
