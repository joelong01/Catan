using System.Collections.ObjectModel;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class SettingsDlg : ContentDialog
    {
        #region Methods

        private void OnCancel(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void OnOk(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void OnResetGridLayout(object sender, RoutedEventArgs e)
        {
            await MainPage.Current.ResetGridLayout();
        }

        #endregion Methods

        #region Constructors

        private ObservableCollection<string> ValidServiceUris { get; set; } = new ObservableCollection<string>();

        public SettingsDlg()
        {
            this.InitializeComponent();
            ValidServiceUris.Add("localhost:5001");
            ValidServiceUris.Add("192.168.1.128:5000");
            ValidServiceUris.Add("catanhub.azurewebsites.net");
        }

        public SettingsDlg(Settings settings, PlayerModel human)
        {
            this.InitializeComponent();
            this.Settings = settings;
            this.TheHuman = human;
            ValidServiceUris.Add("localhost:5001");
            ValidServiceUris.Add("192.168.1.128:5000");
            ValidServiceUris.Add("catanhub.azurewebsites.net");
        }

        #endregion Constructors

        #region Properties

        public ICatanSettings CatanSettingsCallback { get; set; }

        #endregion Properties

        #region Properties

        public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register("Settings", typeof(Settings), typeof(SettingsDlg), new PropertyMetadata(new Settings()));
        public static readonly DependencyProperty TheHumanProperty = DependencyProperty.Register("TheHuman", typeof(PlayerModel), typeof(SettingsDlg), new PropertyMetadata(PlayerModel.DefaultPlayer));

        public Settings Settings
        {
            get => (Settings)GetValue(SettingsProperty);
            set => SetValue(SettingsProperty, value);
        }

        public PlayerModel TheHuman
        {
            get => (PlayerModel)GetValue(TheHumanProperty);
            set => SetValue(TheHumanProperty, value);
        }

        #endregion Properties

        private void OnServiceUriChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (e.AddedItems.Count > 0)
            //{
            //    this.Settings.HostName = e.AddedItems[0].ToString();
            //}

            this.TraceMessage($"{e.AddedItems[0]}");
        }
    }
}