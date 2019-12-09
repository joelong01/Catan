using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{


    public sealed partial class SettingsDlg : ContentDialog
    {
        public ICatanSettings CatanSettingsCallback { get; set; }

        public SettingsDlg()
        {
            this.InitializeComponent();
        }

        public SettingsDlg(ICatanSettings pCb, Settings settings)
        {
            this.InitializeComponent();
            CatanSettingsCallback = pCb;
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                UpdateUi(settings);


            }
        }

        #region Properties

        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), typeof(SettingsDlg), new PropertyMetadata(1, ZoomChanged));
        public static readonly DependencyProperty AnimationSpeedProperty = DependencyProperty.Register("AnimationSpeed", typeof(double), typeof(SettingsDlg), new PropertyMetadata(3, AnimationSpeedChanged));
        public static readonly DependencyProperty ResourceTrackingProperty = DependencyProperty.Register("ResourceTracking", typeof(bool?), typeof(SettingsDlg), new PropertyMetadata(true, ResourceTrackingChanged));
        public static readonly DependencyProperty RotateTileProperty = DependencyProperty.Register("RotateTile", typeof(bool?), typeof(SettingsDlg), new PropertyMetadata(false, RotateTileChanged));
        public static readonly DependencyProperty ShowStopWatchProperty = DependencyProperty.Register("ShowStopWatch", typeof(bool?), typeof(SettingsDlg), new PropertyMetadata(true, ShowStopWatchChanged));
        public static readonly DependencyProperty RandomizeNumbersProperty = DependencyProperty.Register("RandomizeNumbers", typeof(bool?), typeof(SettingsDlg), new PropertyMetadata(true, RandomizeNumbersChanged));
        public static readonly DependencyProperty ValidateBuildingProperty = DependencyProperty.Register("ValidateBuilding", typeof(bool?), typeof(SettingsDlg), new PropertyMetadata(true, ValidateBuildingChanged));
        public static readonly DependencyProperty AnimateFadeTilesProperty = DependencyProperty.Register("AnimateFadeTiles", typeof(bool?), typeof(SettingsDlg), new PropertyMetadata(true, AnimateFadeTilesChanged));
        public static readonly DependencyProperty FadeTimeProperty = DependencyProperty.Register("FadeTime", typeof(double), typeof(SettingsDlg), new PropertyMetadata(3, FadeTimeChanged));
        public double FadeTime
        {
            get => (double)GetValue(FadeTimeProperty);
            set => SetValue(FadeTimeProperty, value);
        }
        private static void FadeTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettingsDlg depPropClass = d as SettingsDlg;
            double depPropValue = (double)e.NewValue;
            depPropClass.SetFadeTime(depPropValue);
        }
        private async void SetFadeTime(double value)
        {

            CatanSettingsCallback.FadeSeconds = (int)value;
            await CatanSettingsCallback.SettingChanged();
        }




        public bool? AnimateFadeTiles
        {
            get => (bool?)GetValue(AnimateFadeTilesProperty);
            set => SetValue(AnimateFadeTilesProperty, value);
        }
        private static void AnimateFadeTilesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettingsDlg depPropClass = d as SettingsDlg;
            bool? depPropValue = (bool?)e.NewValue;
            depPropClass.SetAnimateFadeTiles(depPropValue);
        }
        private async void SetAnimateFadeTiles(bool? value)
        {
            CatanSettingsCallback.AnimateFade = (bool)value;
            await CatanSettingsCallback.SettingChanged();

        }

        public bool? ValidateBuilding
        {
            get => (bool?)GetValue(ValidateBuildingProperty);
            set => SetValue(ValidateBuildingProperty, value);
        }
        private static void ValidateBuildingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettingsDlg depPropClass = d as SettingsDlg;
            bool? depPropValue = (bool?)e.NewValue;
            depPropClass.SetValidateBuilding(depPropValue);
        }
        private async void SetValidateBuilding(bool? value)
        {
            CatanSettingsCallback.ValidateBuilding = (bool)value;
            await CatanSettingsCallback.SettingChanged();

        }

        public bool? RandomizeNumbers
        {
            get => (bool?)GetValue(RandomizeNumbersProperty);
            set => SetValue(RandomizeNumbersProperty, value);
        }
        private static void RandomizeNumbersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettingsDlg depPropClass = d as SettingsDlg;
            bool? depPropValue = (bool?)e.NewValue;
            depPropClass.SetRandomizeNumbers(depPropValue);
        }
        private async void SetRandomizeNumbers(bool? value)
        {
            CatanSettingsCallback.UseRandomNumbers = (bool)value;
            await CatanSettingsCallback.SettingChanged();

        }

        public bool? ShowStopWatch
        {
            get => (bool?)GetValue(ShowStopWatchProperty);
            set => SetValue(ShowStopWatchProperty, value);
        }
        private static void ShowStopWatchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettingsDlg depPropClass = d as SettingsDlg;
            bool? depPropValue = (bool?)e.NewValue;
            depPropClass.SetShowStopWatch(depPropValue);
        }
        private async void SetShowStopWatch(bool? value)
        {
            CatanSettingsCallback.ShowStopwatch = (bool)value;
            await CatanSettingsCallback.SettingChanged();

        }

        public bool? RotateTile
        {
            get => (bool?)GetValue(RotateTileProperty);
            set => SetValue(RotateTileProperty, value);
        }
        private static void RotateTileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettingsDlg depPropClass = d as SettingsDlg;
            bool? depPropValue = (bool?)e.NewValue;
            depPropClass.SetRotateTile(depPropValue);
        }
        private async void SetRotateTile(bool? value)
        {
            CatanSettingsCallback.RotateTile = (bool)value;
            await CatanSettingsCallback.SettingChanged();

        }

        public bool? ResourceTracking
        {
            get => (bool?)GetValue(ResourceTrackingProperty);
            set => SetValue(ResourceTrackingProperty, value);
        }
        private static void ResourceTrackingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettingsDlg depPropClass = d as SettingsDlg;
            bool? depPropValue = (bool?)e.NewValue;
            depPropClass.SetResourceTracking(depPropValue);
        }
        private async void SetResourceTracking(bool? value)
        {
            CatanSettingsCallback.ResourceTracking = (bool)value;
            await CatanSettingsCallback.SettingChanged();

        }


        public double AnimationSpeed
        {
            get => (double)GetValue(AnimationSpeedProperty);
            set => SetValue(AnimationSpeedProperty, value);
        }
        private static void AnimationSpeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettingsDlg depPropClass = d as SettingsDlg;
            double depPropValue = (double)e.NewValue;
            depPropClass.SetAnimationSpeed(depPropValue);
        }
        private async void SetAnimationSpeed(double value)
        {
            CatanSettingsCallback.AnimationSpeedBase = (int)value;
            await CatanSettingsCallback.SettingChanged();

        }

        public double Zoom
        {
            get => (double)GetValue(ZoomProperty);
            set => SetValue(ZoomProperty, value);
        }
        private static void ZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SettingsDlg depPropClass = d as SettingsDlg;
            double depPropValue = (double)e.NewValue;
            depPropClass.SetZoom(depPropValue);
        }
        private async void SetZoom(double value)
        {
            CatanSettingsCallback.Zoom = value;
            await CatanSettingsCallback.SettingChanged();

        }




        #endregion


        private void UpdateUi(Settings settings)
        {
            AnimateFadeTiles = settings.AnimateFade;
            RotateTile = settings.RotateTile;
            FadeTime = settings.FadeSeconds;
            Zoom = settings.Zoom;
            ShowStopWatch = settings.ShowStopwatch;
            AnimationSpeed = settings.AnimationSpeed;


        }


        private void OnOk(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private void OnCancel(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }

        private async void OnResetGridLayout(object sender, RoutedEventArgs e)
        {
            await CatanSettingsCallback.ResetGridLayout();
        }
    }
}
