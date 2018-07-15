using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
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
            get { return _settings; }
            set { _settings = value; }
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
                    _daMove.To = 0;

                else
                    _daMove.To = width; ;

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

       


        private string SaveFileName = "CatanSettings.ini";
        public async Task SaveSettings()
        {
            try
            {
                
                string saveString = _settings.Serialize(); ;
                if (saveString == "")
                    return;

                Dictionary<string, string> dict = new Dictionary<string, string>
                {
                    ["Settings"] = saveString
                };


                var folder = await StaticHelpers.GetSaveFolder();
                var option = CreationCollisionOption.ReplaceExisting;
                var file = await folder.CreateFileAsync(SaveFileName, option);
                await FileIO.WriteTextAsync(file, StaticHelpers.SerializeDictionary(dict));


            }
            catch (Exception exception)
            {

                string s = StaticHelpers.GetErrorMessage($"Error saving to file {SaveFileName}", exception);
                await StaticHelpers.ShowErrorText(s);

            }
        }

        private async void RotateTile_Checked(object sender, RoutedEventArgs e)
        {
            if (_initializing)
                return;
            _settings.RotateTile = (((CheckBox)sender).IsChecked == true);
            await SaveSettings();
            CatanSettingsCallback.RotateTile = _settings.RotateTile;


        }



        private async void AnimateFadeTile_Checked(object sender, RoutedEventArgs e)
        {
            if (_initializing)
                return;
            _settings.AnimateFade = (((CheckBox)sender).IsChecked == true);
            await SaveSettings();
            CatanSettingsCallback.AnimateFade = _settings.AnimateFade;

        }

        private async void FadeValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_initializing)
                return;
            _settings.FadeSeconds = (int)((Slider)sender).Value;
            await SaveSettings();
            CatanSettingsCallback.FadeSeconds = _settings.FadeSeconds;

        }

        private async void ZoomValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_initializing)
                return;
            _settings.Zoom = (double)((Slider)sender).Value;
            await SaveSettings();
            CatanSettingsCallback.Zoom = _settings.Zoom;

        }



        private async void ShowStopwatch_Checked(object sender, RoutedEventArgs e)
        {
            if (_initializing)
                return;
            _settings.ShowStopwatch = (((CheckBox)sender).IsChecked == true);
            await SaveSettings();
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

        private async void AnimationSpeedChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (_initializing)
                return;
            _settings.AnimationSpeed = (int)_sliderAnimationSpeed.Value;
            await SaveSettings();
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

        private async void ResourceTracking_Click(object sender, RoutedEventArgs e)
        {
            if (_initializing)
                return;
            _settings.ResourceTracking = (((CheckBox)sender).IsChecked == true);
            await SaveSettings();
            CatanSettingsCallback.ResourceTracking = _settings.ResourceTracking;
        }

       

        private async void UseRandomNumbers_Checked(object sender, RoutedEventArgs e)
        {
            if (_initializing)
                return;
            _settings.UseRandomNumbers = (((CheckBox)sender).IsChecked == true);
            await SaveSettings();
            CatanSettingsCallback.UseRandomNumbers = _settings.UseRandomNumbers;
        }

        private async void ValidateBuilding_Checked(object sender, RoutedEventArgs e)
        {
            if (_initializing)
                return;
            _settings.ValidateBuilding = (((CheckBox)sender).IsChecked == true);
            await SaveSettings();
            CatanSettingsCallback.ValidateBuilding = _settings.ValidateBuilding;
        }
    }


}
