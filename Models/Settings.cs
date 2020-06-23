using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Catan.Proxy;

namespace Catan10
{
    public class Settings : INotifyPropertyChanged
    {
        #region Delegates + Fields + Events + Enums

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _animateFade = true;
        private bool _AnimateFadeTiles = true;
        private int _animationSpeed = 3;
        private bool _AutoJoinGames = false;
        private bool _AutoRespond = false;
        private GameCommunicationStrategy _comStrat = GameCommunicationStrategy.Homegrown;
        string _defaultGameName = "DefaultGame";
        private int _fadeSeconds = 3;
        private int _FadeTime = 0;
        private GridPosition _GameViewPosition = new GridPosition();
        private Dictionary<string, GridPosition> _gridPosition = new Dictionary<string, GridPosition>();
        private bool _RandomizeNumbers = true;
        private bool _resourceTracking = true;
        private bool _rotateTile = false;
        private string _serviceUri = "jdlgameservice.azurewebsites.net";
        private bool _showStopwatch = true;
        private bool _useClassicTiles = true;
        private bool _useRandomNumbers = true;
        private bool _validateBuilding = true;
        private double _zoom = 1.0;

        #endregion Delegates + Fields + Events + Enums

        #region Properties

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
        bool _smallOffers = false;
        public bool SmallOffers
        {
            get
            {
                return _smallOffers;
            }
            set
            {
                if (_smallOffers != value)
                {
                    _smallOffers = value;
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

        public string DefaultGameName
        {
            get
            {
                return _defaultGameName;
            }
            set
            {
                if (_defaultGameName != value)
                {
                    _defaultGameName = value;
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

        public string HostName
        {
            get
            {
                return _serviceUri;
            }
            set
            {
                if (_serviceUri != value)
                {
                    _serviceUri = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool IsHomegrownGame
        {
            get
            {
                return _comStrat == GameCommunicationStrategy.Homegrown;
            }

            set
            {
                {
                    if (_comStrat != GameCommunicationStrategy.Homegrown && value)
                    {
                        _comStrat = GameCommunicationStrategy.Homegrown;
                        IsLocalGame = false;
                        IsSignalRGame = false;
                    }

                    NotifyPropertyChanged();
                }
            }
        }

        public bool IsLocalGame
        {
            get
            {
                return _comStrat == GameCommunicationStrategy.Local;
            }
            set
            {
                if (_comStrat != GameCommunicationStrategy.Local && value)
                {
                    _comStrat = GameCommunicationStrategy.Local;
                    IsHomegrownGame = false;
                    IsSignalRGame = false;
                }
                NotifyPropertyChanged();
            }
        }

        public bool IsSignalRGame
        {
            get
            {
                return _comStrat == GameCommunicationStrategy.SignalR;
            }
            set
            {
                if (_comStrat != GameCommunicationStrategy.SignalR && value)
                {
                    _comStrat = GameCommunicationStrategy.SignalR;
                    IsHomegrownGame = false;
                    IsLocalGame = false;
                }
                NotifyPropertyChanged();
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

        #endregion Properties

        #region Constructors + Destructors

        public Settings()
        {
        }

        #endregion Constructors + Destructors

        #region Methods

        public static Settings Deserialize(string s)
        {
            return CatanProxy.Deserialize<Settings>(s);
        }

        public string Serialize()
        {
            return CatanProxy.Serialize(this);
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods
    }
}
