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
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class RollHistoryChart : UserControl
    {
 
        List<CatanLineChart> _linesComputer = new List<CatanLineChart>();
        List<CatanLineChart> _linesPlayer = new List<CatanLineChart>();


 
        public RollHistoryChart()
        {
            this.InitializeComponent();
            foreach (FrameworkElement e in LayoutRoot.Children)
            {
                if (e.GetType() == typeof(CatanLineChart))
                {
                    if (e.Name.Contains("c"))
                        _linesComputer.Add((CatanLineChart)e);
                    else
                        _linesPlayer.Add((CatanLineChart)e);
                }
            }
        }

        public void Reset()
        {
            foreach (var line in _linesComputer)
            {
                line.AnimatePercent(0, MainPage.GetAnimationSpeed(AnimationSpeed.Normal), 0);
                line.RollCounts = "0";
            }

            foreach (var line in _linesPlayer)
            {
                line.AnimatePercent(0, MainPage.GetAnimationSpeed(AnimationSpeed.Normal), 0);
                line.RollCounts = "0";
            }

           
        }

        private double _dMaxPercent = 1.0;

        public void UpdateChart(int[] allRollCount, double[] allRollPercent, int[] playerRollCount, double[] playerPercent)
        {
          

            double animationDuration = MainPage.GetAnimationSpeed(AnimationSpeed.Normal);

            for (int i = 0; i < 11; i++)
            {
                double p = allRollPercent[i] ;
                
                _linesComputer[i].AnimatePercent(p, animationDuration, 0);                
                _linesComputer[i].RollCounts = allRollCount[i].ToString();
                p = playerPercent[i] ;                
                _linesPlayer[i].AnimatePercent(p, animationDuration, 0);                
                _linesPlayer[i].RollCounts = playerRollCount[i].ToString();
            }
        }

        private async void Slider_ZoomValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            List<Task> taskList = new List<Task>();
            

            _dMaxPercent = (double)e.NewValue / 100.0;
            for (int i = 0; i < _linesComputer.Count; i++) // when ctor is called the slider is initialzied and Count == 0
            {
                Task[] tasks = _linesComputer[i].SetMaxPercentAndGetTasks(_dMaxPercent);
                taskList.AddRange(tasks);

                tasks = _linesPlayer[i].SetMaxPercentAndGetTasks(_dMaxPercent);
                taskList.AddRange(tasks);

                //_linesComputer[i].MaxPercent = _dMaxPercent;
                //_linesPlayer[i].MaxPercent = _dMaxPercent;
            }

            await Task.WhenAll(taskList);
        }
    }
}
