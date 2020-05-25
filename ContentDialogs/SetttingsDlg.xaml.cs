using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Catan.Proxy;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public class Settings : INotifyPropertyChanged
    {
        #region properties

        private bool _animateFade = true;
        private bool _AnimateFadeTiles = true;
        private int _animationSpeed = 3;
        private bool _AutoJoinGames = false;
        private bool _AutoRespond = false;
        private int _fadeSeconds = 3;
        private int _FadeTime = 0;
        private GridPosition _GameViewPosition = new GridPosition();
        private Dictionary<string, GridPosition> _gridPosition = new Dictionary<string, GridPosition>();
        private bool _RandomizeNumbers = true;
        private bool _resourceTracking = true;
        private bool _rotateTile = false;
        private bool _showStopwatch = true;
        private bool _useClassicTiles = true;
        private bool _useRandomNumbers = true;
        private bool _validateBuilding = true;
        private double _zoom = 1.0;

        public bool AnimateFade
        {
            get
            {
                return _animateFade;
            }
            set
            {
                if (value != _animateFade)
                {
                    _animateFade = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool AnimateFadeTiles
        {
            get
            {
                return _AnimateFadeTiles;
            }
            set
            {
                if (_AnimateFadeTiles != value)
                {
                    _AnimateFadeTiles = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int AnimationSpeed
        {
            get
            {
                return _animationSpeed;
            }
            set
            {
                if (value != _animationSpeed)
                {
                    _animationSpeed = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool AutoJoinGames
        {
            get
            {
                return _AutoJoinGames;
            }
            set
            {
                if (value != _AutoJoinGames)
                {
                    _AutoJoinGames = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool AutoRespond
        {
            get
            {
                return _AutoRespond;
            }
            set
            {
                if (value != _AutoRespond)
                {
                    _AutoRespond = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int FadeSeconds
        {
            get
            {
                return _fadeSeconds;
            }
            set
            {
                if (value != _fadeSeconds)
                {
                    _fadeSeconds = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int FadeTime
        {
            get
            {
                return _FadeTime;
            }
            set
            {
                if (_FadeTime != value)
                {
                    _FadeTime = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public GridPosition GameViewPosition
        {
            get
            {
                return _GameViewPosition;
            }
            set
            {
                if (_GameViewPosition != value)
                {
                    _GameViewPosition = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Dictionary<string, GridPosition> GridPositions
        {
            get
            {
                return _gridPosition;
            }
            set
            {
                if (value != _gridPosition)
                {
                    _gridPosition = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool RandomizeNumbers
        {
            get
            {
                return _RandomizeNumbers;
            }
            set
            {
                if (_RandomizeNumbers != value)
                {
                    _RandomizeNumbers = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool ResourceTracking
        {
            get
            {
                return _resourceTracking;
            }
            set
            {
                if (value != _resourceTracking)
                {
                    _resourceTracking = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool RotateTile
        {
            get
            {
                return _rotateTile;
            }
            set
            {
                if (value != _rotateTile)
                {
                    _rotateTile = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool ShowStopwatch
        {
            get
            {
                return _showStopwatch;
            }
            set
            {
                if (value != _showStopwatch)
                {
                    _showStopwatch = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool UseClassicTiles
        {
            get
            {
                return _useClassicTiles;
            }
            set
            {
                if (value != _useClassicTiles)
                {
                    _useClassicTiles = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool UseRandomNumbers
        {
            get
            {
                return _useRandomNumbers;
            }
            set
            {
                if (value != _useRandomNumbers)
                {
                    _useRandomNumbers = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool ValidateBuilding
        {
            get
            {
                return _validateBuilding;
            }
            set
            {
                if (value != _validateBuilding)
                {
                    _validateBuilding = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double Zoom
        {
            get
            {
                return _zoom;
            }
            set
            {
                if (value != _zoom)
                {
                    _zoom = value;
                    NotifyPropertyChanged();
                }
            }
        }

        #endregion properties

        public Settings()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static Settings Deserialize(string s)
        {
            return CatanProxy.Deserialize<Settings>(s);
        }

        public string Serialize()
        {
            return CatanProxy.Serialize(this);
        }
    }

    public sealed partial class SettingsDlg : ContentDialog
    {
        public SettingsDlg()
        {
            this.InitializeComponent();
        }

        public SettingsDlg(Settings settings, PlayerModel human)
        {
            this.InitializeComponent();
            this.Settings = settings;
            this.TheHuman = human;
        }

        public ICatanSettings CatanSettingsCallback { get; set; }

        #region Properties

        public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register("Settings", typeof(Settings), typeof(SettingsDlg), new PropertyMetadata(new Settings()));
        public static readonly DependencyProperty TheHumanProperty = DependencyProperty.Register("TheHuman", typeof(PlayerModel), typeof(SettingsDlg), new PropertyMetadata(MainPage.Current.TheHuman));

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
    }
}