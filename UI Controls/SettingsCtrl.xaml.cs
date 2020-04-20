using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{





    public sealed partial class SettingsCtrl : UserControl
    {



        private bool _initializing = true;
        private Settings _settings = new Settings();

        public ICatanSettings CatanSettingsCallback { get; set; }

        public Settings Settings
        {
            get => _settings;
            set => _settings = value;
        }

        public SettingsCtrl()
        {
            this.InitializeComponent();


        }

        public void Init(ICatanSettings pCb)
        {
            CatanSettingsCallback = pCb;

            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                UpdateUi();
                NotifyAllSettings();

            }
        }

        public async Task Show()
        {
            try
            {
                double width = 334;
                HamburgerButton.IsEnabled = false;
                if (_daMove.To == width)
                {
                    _daMove.To = 0;
                }
                else
                {
                    _daMove.To = width;
                };

                await _sbMove.ToTask();
            }
            finally
            {
                HamburgerButton.IsEnabled = true;
            }

        }

        private void UpdateUi()
        {
            _chkAnimateFade.IsChecked = _settings.AnimateFade;
            _chkRotateTile.IsChecked = _settings.RotateTile;
            _sliderFadeTime.Value = _settings.FadeSeconds;
            _sliderZoom.Value = _settings.Zoom;
            _chkShowStopwatch.IsChecked = _settings.ShowStopwatch;
            _sliderAnimationSpeed.Value = _settings.AnimationSpeed;

            _initializing = false;



        }

        public void NotifyAllSettings()
        {
            CatanSettingsCallback.AnimateFade = _settings.AnimateFade;
            CatanSettingsCallback.RotateTile = _settings.RotateTile;
            CatanSettingsCallback.FadeSeconds = _settings.FadeSeconds;
            CatanSettingsCallback.ShowStopwatch = _settings.ShowStopwatch;
            CatanSettingsCallback.AnimationSpeedBase = _settings.AnimationSpeed;
            CatanSettingsCallback.ResourceTracking = _settings.ResourceTracking;
            CatanSettingsCallback.UseRandomNumbers = _settings.UseRandomNumbers;
            CatanSettingsCallback.ValidateBuilding = _settings.ValidateBuilding;
        }




        private void RotateTile_Checked(object sender, RoutedEventArgs e)
        {
            if (_initializing)
            {
                return;
            }

            _settings.RotateTile = (((CheckBox)sender).IsChecked == true);

            CatanSettingsCallback.RotateTile = _settings.RotateTile;


        }



        private void AnimateFadeTile_Checked(object sender, RoutedEventArgs e)
        {
            if (_initializing)
            {
                return;
            }

            _settings.AnimateFade = (((CheckBox)sender).IsChecked == true);

            CatanSettingsCallback.AnimateFade = _settings.AnimateFade;

        }

        private void FadeValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_initializing)
            {
                return;
            }

            _settings.FadeSeconds = (int)((Slider)sender).Value;

            CatanSettingsCallback.FadeSeconds = _settings.FadeSeconds;

        }

        private void ZoomValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_initializing)
            {
                return;
            }

            _settings.Zoom = ((Slider)sender).Value;

            CatanSettingsCallback.Zoom = _settings.Zoom;

        }



        private void ShowStopwatch_Checked(object sender, RoutedEventArgs e)
        {
            if (_initializing)
            {
                return;
            }

            _settings.ShowStopwatch = (((CheckBox)sender).IsChecked == true);

            CatanSettingsCallback.ShowStopwatch = _settings.ShowStopwatch;
        }


        private void OnNewGame(object sender, RoutedEventArgs e)
        {
            CatanSettingsCallback.NewGame();
        }



        private void OnOpenSavedGame(object sender, RoutedEventArgs e)
        {
            CatanSettingsCallback.OpenSavedGame();
        }

        private void AnimationSpeedChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_initializing)
            {
                return;
            }

            _settings.AnimationSpeed = (int)_sliderAnimationSpeed.Value;

            CatanSettingsCallback.AnimationSpeedBase = _settings.AnimationSpeed;
        }


        private void OnClose(object sender, RoutedEventArgs e)
        {
            CatanSettingsCallback.Close();
        }

        private void OnReshuffle(object sender, RoutedEventArgs e)
        {
            CatanSettingsCallback.Reshuffle();
        }

        private void _btnExplorer(object sender, RoutedEventArgs e)
        {
            CatanSettingsCallback.Explorer();
        }

        private void _btnRotateTiles(object sender, RoutedEventArgs e)
        {
            CatanSettingsCallback.RotateTiles();
        }

        private void OnWinner(object sender, RoutedEventArgs e)
        {
            CatanSettingsCallback.Winner();
        }

        private async void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            await Show();
        }

        private void ResourceTracking_Click(object sender, RoutedEventArgs e)
        {
            if (_initializing)
            {
                return;
            }

            _settings.ResourceTracking = (((CheckBox)sender).IsChecked == true);

            CatanSettingsCallback.ResourceTracking = _settings.ResourceTracking;
        }



        private void UseRandomNumbers_Checked(object sender, RoutedEventArgs e)
        {
            if (_initializing)
            {
                return;
            }

            _settings.UseRandomNumbers = (((CheckBox)sender).IsChecked == true);

            CatanSettingsCallback.UseRandomNumbers = _settings.UseRandomNumbers;
        }

        private void ValidateBuilding_Checked(object sender, RoutedEventArgs e)
        {
            if (_initializing)
            {
                return;
            }

            _settings.ValidateBuilding = (((CheckBox)sender).IsChecked == true);

            CatanSettingsCallback.ValidateBuilding = _settings.ValidateBuilding;
        }
    }


}
