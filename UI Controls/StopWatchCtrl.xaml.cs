using System;

using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class StopWatchCtrl : UserControl
    {
        readonly DispatcherTimer _timer = new DispatcherTimer();
        DateTime _start;
        TimeSpan _totalTime;


        readonly SolidColorBrush _green = CatanColors.GetResourceBrush("Green", Colors.Green);
        readonly SolidColorBrush _red = CatanColors.GetResourceBrush("Red", Colors.Red);
        readonly SolidColorBrush _yellow = CatanColors.GetResourceBrush("Yellow", Colors.Yellow);

        new public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Color), typeof(StopWatchCtrl), new PropertyMetadata(Colors.Black, BackgroundChanged));
        new public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Color), typeof(StopWatchCtrl), new PropertyMetadata(Colors.White, ForegroundChanged));
        public static readonly DependencyProperty ColorCodeTimerProperty = DependencyProperty.Register("ColorCodeTimer", typeof(bool), typeof(StopWatchCtrl), new PropertyMetadata(true));
        public bool ColorCodeTimer
        {
            get => (bool)GetValue(ColorCodeTimerProperty);
            set => SetValue(ColorCodeTimerProperty, value);
        }


        new public Color Foreground
        {
            get => (Color)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }
        private static void ForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StopWatchCtrl depPropClass = d as StopWatchCtrl;
            Color depPropValue = (Color)e.NewValue;
            depPropClass.SetForeground(depPropValue);
        }
        private void SetForeground(Color value)
        {
            _tbTime.Foreground = new SolidColorBrush(value);
        }

        new public Color Background
        {
            get => (Color)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }
        private static void BackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StopWatchCtrl depPropClass = d as StopWatchCtrl;
            Color depPropValue = (Color)e.NewValue;
            depPropClass.SetBackground(depPropValue);
        }
        private void SetBackground(Color value)
        {
            LayoutRoot.Background = new SolidColorBrush(value);
        }


        public StopWatchCtrl()
        {
            this.InitializeComponent();

            _timer.Tick += Timer_Tick;
            _timer.Interval = TimeSpan.FromMilliseconds(100);
        }

        private void Timer_Tick(object sender, object e)
        {
            TimeSpan delta = DateTime.Now - _start;
            _totalTime += delta;
            _tbTime.Text = _totalTime.ToString(@"mm\:ss");
            _start = DateTime.Now;
            if (ColorCodeTimer)
            {
                if (_totalTime.TotalSeconds < 60)
                {
                    _tbTime.Foreground = _green;
                }
                else if (_totalTime.TotalSeconds < 120)
                {
                    _tbTime.Foreground = _yellow;
                }
                else if (_totalTime.TotalSeconds > 180)
                {
                    _tbTime.Foreground = _red;
                }
            }

        }




        public TimeSpan TotalTime
        {
            get => _totalTime;
            set
            {
                _totalTime = value;
                _tbTime.Text = _totalTime.ToString(@"mm\:ss");

            }

        }

        public void StartTimer()
        {
            _start = DateTime.Now;
            _timer.Start();

        }

        public void StopTimer()
        {
            _timer.Stop();
        }

        public DateTime StartTime
        {
            get => _start;
            set => _start = value;
        }
    }
}
