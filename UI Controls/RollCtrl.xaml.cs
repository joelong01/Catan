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
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class RollCtrl : UserControl
    {
        MersenneTwister twist = new MersenneTwister();

        public RollCtrl()
        {
            this.InitializeComponent();
            FlipClose.Begin();
        }
        public static readonly DependencyProperty DiceOneProperty = DependencyProperty.Register("DiceOne", typeof(string), typeof(RollCtrl), new PropertyMetadata("\U00012680"));
        public string DiceOne
        {
            get => (string)GetValue(DiceOneProperty);
            set => SetValue(DiceOneProperty, value);
        }

        public static readonly DependencyProperty DiceTwoProperty = DependencyProperty.Register("DiceTwo", typeof(string), typeof(RollCtrl), new PropertyMetadata("\U00012680"));
        public string DiceTwo
        {
            get => (string)GetValue(DiceTwoProperty);
            set => SetValue(DiceTwoProperty, value);
        }
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(TileOrientation), typeof(RollCtrl), new PropertyMetadata(TileOrientation.FaceDown, OrientationChanged));
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
            if (orientation == TileOrientation.FaceDown) FlipClose.Begin();
            if (orientation == TileOrientation.FaceUp) FlipOpen.Begin();

            //SetupFlipAnimation(orientation, DoubleAnimation_FlipBackTile, DoubleAnimation_FlipFrontTile, MainPage.GetAnimationSpeed(AnimationSpeed.VeryFast), 0);
            //StoryBoard_Flip.Begin();
        }

        private int _roll = 0;
        public int Roll => _roll;
        private string GetDiceString(int val)
        {
            string key = $"Dice_{val}";
            return (string)App.Current.Resources[key];
        }
        public void Randomize()
        {
            int index = twist.Next(1, 6);
            _roll = index;
            DiceOne = GetDiceString(index);
            index = twist.Next(1, 6);
            _roll += index;
            DiceTwo = GetDiceString(index);            
        }

        public Task GetFlipTask(TileOrientation orientation)
        {
            //var animationTime = SetupFlipAnimation(orientation, DoubleAnimation_FlipBackTile, DoubleAnimation_FlipFrontTile, MainPage.GetAnimationSpeed(AnimationSpeed.Normal), 0);
            //StoryBoard_Flip.Duration = TimeSpan.FromMilliseconds(animationTime);
            //return StoryBoard_Flip.ToTask();

            if (orientation == TileOrientation.FaceDown) return FlipClose.ToTask();
            if (orientation == TileOrientation.FaceUp) return FlipOpen.ToTask();
            throw new InvalidEnumArgumentException();
        }

        private double SetupFlipAnimation(TileOrientation orientation, DoubleAnimation back, DoubleAnimation front, double animationTimeInMs, double startAfter = 0)
        {

            if (orientation == TileOrientation.FaceUp)
            {
                back.To = -90;
                front.To = 0;
                front.Duration = TimeSpan.FromMilliseconds(animationTimeInMs);
                back.Duration = TimeSpan.FromMilliseconds(animationTimeInMs);
                back.BeginTime = TimeSpan.FromMilliseconds(startAfter);
                front.BeginTime = TimeSpan.FromMilliseconds(startAfter + animationTimeInMs);
            }
            else
            {
                back.To = 0;
                front.To = 90;
                back.Duration = TimeSpan.FromMilliseconds(animationTimeInMs);
                front.Duration = TimeSpan.FromMilliseconds(animationTimeInMs);
                front.BeginTime = TimeSpan.FromMilliseconds(startAfter);
                back.BeginTime = TimeSpan.FromMilliseconds(startAfter + animationTimeInMs);

            }
            return animationTimeInMs;

        }

    }
}
