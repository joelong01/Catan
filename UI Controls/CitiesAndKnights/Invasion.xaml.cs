using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public class InvasionData : INotifyPropertyChanged
    {
        public int INVASION_STEPS { get; } = 7;
        int _totalInvasions = 0;
        int _currentStep = 0;
        bool _showBaron = true;
        public bool ShowBaron
        {
            get
            {
                return _showBaron;
            }
            set
            {
                if (_showBaron != value)
                {
                    _showBaron = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public int CurrentStep
        {
            get
            {
                return _currentStep;
            }
        }

        public int NextStep()
        {
            _currentStep++;

            if (_currentStep > INVASION_STEPS)
            {
                _currentStep = 0;
            }
            NotifyPropertyChanged("CurrentStep");
            return _currentStep;
        }

        public int PreviousStep()
        {
            _currentStep--;
            if (_currentStep == INVASION_STEPS)
            {
                if (_totalInvasions > 0)
                {
                    _totalInvasions--;
                }
            }
            if (_currentStep < 0) _currentStep = 0;
            NotifyPropertyChanged("CurrentStep");
            return _currentStep;
        }

        public int TotalInvasions
        {
            get
            {
                return _totalInvasions;
            }
            set
            {
                if (_totalInvasions != value)
                {
                    _totalInvasions = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public sealed partial class InvasionCtrl : UserControl
    {


        public InvasionCtrl()
        {
            this.InitializeComponent();
            Angle = 0;
        }

        public void Reset()
        {
            InvasionData = new InvasionData();
            Angle = 0;
        }

        public static readonly DependencyProperty InvasionDataProperty = DependencyProperty.Register("InvasionData", typeof(InvasionData), typeof(InvasionCtrl), new PropertyMetadata(new InvasionData(), InvasionDataChanged));
        public InvasionData InvasionData
        {
            get => ( InvasionData )GetValue(InvasionDataProperty);
            set => SetValue(InvasionDataProperty, value);
        }
        private static void InvasionDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as InvasionCtrl;
            var depPropValue = (InvasionData)e.NewValue;
            depPropClass?.SetInvasionData(depPropValue);
        }
        private void SetInvasionData(InvasionData model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentStep")
            {
                InvasionData model = sender as InvasionData;
                Debug.Assert(model != null);
                double angle =  model.CurrentStep % (model.INVASION_STEPS + 1 ) * 45;
                Angle = angle;
                return;
            }

            if (e.PropertyName == "ShowBaron")
            {
                InvasionData model = sender as InvasionData;
                Debug.Assert(model != null);
                if (model.ShowBaron)
                {
                    CTRL_Baron.ShowAsync();
                }
                else

                {
                    CTRL_Baron.HideAsync();
                }

            }
        }

        public static readonly DependencyProperty MainPageModelProperty = DependencyProperty.Register("MainPageModel", typeof(MainPageModel), typeof(TraditionalRollCtrl), new PropertyMetadata(MainPageModel.Default));
        public MainPageModel MainPageModel
        {
            get => ( MainPageModel )GetValue(MainPageModelProperty);
            set => SetValue(MainPageModelProperty, value);
        }
        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(TraditionalRollCtrl), new PropertyMetadata(PlayerModel.DefaultPlayer));
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
    



    }
}
