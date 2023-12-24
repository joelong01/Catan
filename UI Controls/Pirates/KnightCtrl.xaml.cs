using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
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
    public sealed partial class KnightCtrl : UserControl
    {
        public KnightCtrl()
        {
            this.InitializeComponent();
        }

        public PlayerModel CurrentPlayer
        {
            get => ( PlayerModel )GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }

        public PlayerModel Owner
        {
            get => ( PlayerModel )GetValue(OwnerProperty);
            set => SetValue(OwnerProperty, value);
        }

        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(CityCtrl), new PropertyMetadata(new PlayerModel()));

        public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register("Owner", typeof(PlayerModel), typeof(CityCtrl), new PropertyMetadata(null));



        public LinearGradientBrush GetBackgroundBrush(PlayerModel current, PlayerModel owner)
        {
            if (DesignMode.DesignMode2Enabled)
            {
                var gradientStopCollection = new GradientStopCollection
                {
                    new GradientStop() { Color = Colors.Black },
                    new GradientStop() { Color = Colors.White }
                };
                var brush = new LinearGradientBrush(gradientStopCollection, 45);
                brush.StartPoint = new Windows.Foundation.Point(0.5, 0);
                brush.EndPoint = new Windows.Foundation.Point(0.5, 1.0);
                return brush;
            }
            return PlayerBindingFunctions.GetBackgroundBrush(current, owner);
        }

        public Brush GetForegroundBrush(PlayerModel current, PlayerModel owner)
        {
            if (owner != null)
            {
                System.Diagnostics.Debug.WriteLine($"owner: {owner}");
            }
            if (DesignMode.DesignMode2Enabled)
            {
                return new SolidColorBrush(Colors.White);
            }
            return PlayerBindingFunctions.GetForegroundBrush(current, owner);
        }
    }
}
