using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public int StepsBeforeInvasion { get; } = 7;

        private int currentCount = 0; // this is the count of steps it goes from 0 - StepsBeforeInvasion

        public InvasionCtrl()
        {
            this.InitializeComponent();
        }
        //
        //  this is the number of invastions.
        public static readonly DependencyProperty InvasionCountProperty = DependencyProperty.Register("InvasionCount", typeof(int), typeof(InvasionCtrl), new PropertyMetadata(0));
        public int InvasionCount
        {
            get => ( int )GetValue(InvasionCountProperty);
            set => SetValue(InvasionCountProperty, value);
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


        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register("Angle", typeof(double), typeof(InvasionCtrl), new PropertyMetadata(0.0, AngleChanged));
        public double Angle
        {
            get => ( double )GetValue(AngleProperty);
            set => SetValue(AngleProperty, value);
        }

        private static void AngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as InvasionCtrl;
            var depPropValue = (double)e.NewValue;
            depPropClass?.SetAngle(depPropValue);

        }
        private void SetAngle(double _v)
        {
            SB_RotateShip.Begin();
        }
        public void StartOver()
        {
            this.TraceMessage("Starting Over");
            Angle = 0;
            
        }

        public int Next()
        {
            currentCount++;
            double angle =  ( double )( currentCount % 8 ) * 45;
            if (angle > 0)
            {
                Angle = angle;
            }
            if (currentCount % StepsBeforeInvasion == 0)
            {
                InvasionCount++;
            }
            return currentCount % 8;
        }

        public int Previous()
        {
            Debug.Assert(currentCount > 0);
            currentCount--;

            if (currentCount == 0)
            {
                InvasionCount--;
            }

            Angle = ( double )( currentCount % 8 ) * 45;
            return currentCount % 8;
        }

        public void HideBaron()
        {
            CTRL_Baron.HideAsync();
        }

        internal void ShowBaron()
        {
            CTRL_Baron.ShowAsync();
        }
    }
}
