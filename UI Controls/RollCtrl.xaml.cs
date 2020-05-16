using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class RollCtrl : UserControl
    {
        
    

        public RollCtrl()
        {
            this.InitializeComponent();
            this.DataContext = this;
            FlipClose.Begin();
        }
        
        MersenneTwister Twist { get; } = new MersenneTwister((int)DateTime.Now.Ticks);
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(TileOrientation), typeof(RollCtrl), new PropertyMetadata(TileOrientation.FaceDown, OrientationChanged));
        public static readonly DependencyProperty DiceOneProperty = DependencyProperty.Register("DiceOne", typeof(int), typeof(RollCtrl), new PropertyMetadata(2));
        public static readonly DependencyProperty DiceTwoProperty = DependencyProperty.Register("DiceTwo", typeof(int), typeof(RollCtrl), new PropertyMetadata(5));
        public int DiceOne
        {
            get => (int)GetValue(DiceOneProperty);
            set => SetValue(DiceOneProperty, value);
        }
        
        public int DiceTwo
        {
            get => (int)GetValue(DiceTwoProperty);
            set => SetValue(DiceTwoProperty, value);
        }
        public TileOrientation Orientation
        {
            get => (TileOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }
        private static void OrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as RollCtrl;
            var depPropValue = (TileOrientation)e.NewValue;
            depPropClass?.SetOrientation(depPropValue);            
        }
        private void SetOrientation(TileOrientation orientation)
        {
            if (PlaneProjection_FaceDown.RotationY == 0 && orientation == TileOrientation.FaceUp)
            {
                FlipOpen.Begin();
            }
            else if(PlaneProjection_FaceDown.RotationY != 0  && orientation == TileOrientation.FaceDown)
            {
                FlipClose.Begin();
            }
         
        }

        public void Randomize()
        {
            DiceOne = Twist.Next(1, 7);            
            DiceTwo = Twist.Next(1, 7);
                        
        }

        public Task GetFlipTask(TileOrientation orientation)
        {
            
            if (orientation == TileOrientation.FaceDown) return FlipClose.ToTask();
            if (orientation == TileOrientation.FaceUp) return FlipOpen.ToTask();
            throw new InvalidEnumArgumentException();
        }

    }
}
