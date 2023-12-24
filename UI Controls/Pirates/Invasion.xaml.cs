using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class InvasionCtrl : UserControl
    {

        private int currentCount = 0;

        public InvasionCtrl()
        {
            this.InitializeComponent();
        }
        public static readonly DependencyProperty MainPageModelProperty = DependencyProperty.Register("MainPageModel", typeof(MainPageModel), typeof(TraditionalRollCtrl), new PropertyMetadata(new MainPageModel()));
        public MainPageModel MainPageModel
        {
            get => ( MainPageModel )GetValue(MainPageModelProperty);
            set => SetValue(MainPageModelProperty, value);
        }
        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(TraditionalRollCtrl), new PropertyMetadata(null));
        public PlayerModel CurrentPlayer
        {
            get => ( PlayerModel )GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }
 

        public void Next()
        {
            currentCount++;
            currentCount = currentCount % 6;
            double angle = currentCount * 60;
            PirateShipAnimation.To = angle;
            SB_RotateShip.Begin();
        }
    }
}
