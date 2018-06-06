using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class CatanLineChart : UserControl
    {

        double _dPercentScale = 1.0;
        double _dPercent;
        double _dExpectedProbabilty = 0.0;

        public CatanLineChart()
        {
            this.InitializeComponent();
        }

        public double ExpectedProbability
        {
            get
            {
                return _dExpectedProbabilty;
            }
            set
            {

                _dExpectedProbabilty = value;
                SetupExpectedProbabilityAnimation();            
                _sbPercentProbability.Begin();


            }

        }

        private void SetupExpectedProbabilityAnimation()
        {

            _daPercentProbability.Duration = TimeSpan.FromMilliseconds(MainPage.GetAnimationSpeed(AnimationSpeed.VeryFast));
            _daPercentProbability.BeginTime = TimeSpan.FromMilliseconds(0);
            _daPercentProbability.To = _dExpectedProbabilty / _dPercentScale;

            if (_daPercentProbability.To > 1.0) _daPercentProbability.To = 1.0;
        }

        public Task AnimatePercentToTask(double percent, double duration, double startAfter)
        {
            SetupAnimation(percent, duration, startAfter);
            return _sbPercent.ToTask();
        }

        public void AnimatePercent(double percent, double duration, double startAfter)
        {
            SetupAnimation(percent, duration, startAfter);
            _sbPercent.Begin();
        }

        private void SetupAnimation(double percent, double duration, double startAfter)
        {
            _dPercent = percent;
            _daPercent.Duration = TimeSpan.FromMilliseconds(duration);
            _daPercent.BeginTime = TimeSpan.FromMilliseconds(startAfter);
            _daPercent.To = percent / _dPercentScale;

            if (_daPercent.To > 1.0) _daPercent.To = 1.0;
            
            Percent = String.Format("{0:0.#}%", percent*100);
        }

        public string RollCounts
        {
            get
            {
                return _txtRollCount.Text;
            }
            set
            {
                _txtRollCount.Text = value;
            }
        }
        public string Percent
        {
            get
            {
                return _txtPercent.Text;
            }
            set
            {
                _txtPercent.Text = value;
            }
        }
        
        public Color LineColor
        {
            get
            {
                return ((SolidColorBrush)_boderLine.BorderBrush).Color;
            }
            set
            {
                SolidColorBrush br = new SolidColorBrush(value); ;
                _rectLine.Fill = br;
                _boderLine.BorderBrush = br;

                _rectBackground.Fill = br;
            }

        }
        
        public double MaxPercent
        {
            get
            {
                return _dPercentScale;
            }
            internal set
            {
                _dPercentScale = value;
                AnimatePercent((double)_dPercent, MainPage.GetAnimationSpeed(AnimationSpeed.Normal), 0);
                SetupExpectedProbabilityAnimation();
                _sbPercentProbability.Begin();
               

            }
        }

        public Task[] SetMaxPercentAndGetTasks(double MaxPercent)
        {
            Task[] tasks = new Task[2];
            _dPercentScale = MaxPercent;
            
            SetupExpectedProbabilityAnimation();
            tasks[0] = _sbPercentProbability.ToTask();
            SetupAnimation(_dPercent, MainPage.GetAnimationSpeed(AnimationSpeed.Normal), 0);
            tasks[1] = _sbPercent.ToTask();
            return tasks;
        }
    }
}
