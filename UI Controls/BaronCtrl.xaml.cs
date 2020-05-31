﻿using Windows.Foundation;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class BaronCtrl : UserControl
    {
        #region Constructors

        public BaronCtrl()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Methods

        public void MoveAsync(Point to)
        {
            var animationDuration = System.TimeSpan.FromMilliseconds(MainPage.GetAnimationSpeed(AnimationSpeed.Fast));

            _sbMove.Duration = animationDuration;
            _daX.Duration = animationDuration;
            _daY.Duration = animationDuration;
            _daX.To = to.X;
            _daY.To = to.Y;
            _sbMove.Begin();
        }

        #endregion Methods
    }
}
