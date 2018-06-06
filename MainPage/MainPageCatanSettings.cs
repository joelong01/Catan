using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml.Controls;

namespace Catan10
{
    public sealed partial class MainPage : Page, ICatanSettings
    {
       
        public double Zoom
        {
            get
            {
                return _transformGameView.ScaleX;
            }

            set
            {
                _settings.Zoom = value;
                _transformGameView.ScaleX = value;
                _transformGameView.ScaleY = value;
                
            }
        }
      
        public bool RotateTile
        {
            get
            {
                return _settings.RotateTile;
            }

            set
            {
               
                _settings.RotateTile = value;
                if (value == false)
                {
                    foreach (TileCtrl t in _gameView.CurrentGame.Tiles)
                    {
                        t.ResetTileRotation();
                    }

                }
            }
        }
        
        public bool AnimateFade
        {
            get
            {
               
                return _settings.AnimateFade;
            }

            set
            {
                _settings.AnimateFade = value;                
            }
        }
        
        public int FadeSeconds
        {
            get
            {
                return _settings.FadeSeconds;
            }

            set
            {
                _settings.FadeSeconds= value;
                _timer.Interval = TimeSpan.FromSeconds(value);
                if (value == 0)
                    _timer.Interval = TimeSpan.FromMilliseconds(250);               
            }
        }

        public bool ShowStopwatch
        {
            get
            {
                return _settings.ShowStopwatch;
            }

            set
            {
                _settings.ShowStopwatch = value;
            }
        }

        public bool UseClassicTiles
        {
            get
            {
                return true;
            }

            set
            {
                
            }
        }

        public int AnimationSpeedBase
        {
            get
            {
                if (_settings.AnimationSpeed > 10)
                    return 4;
                else
                    return _settings.AnimationSpeed;
            }

            set
            {
                _settings.AnimationSpeed = value;
                if (value >= 4) _settings.AnimationSpeed = 10;
            }
        }

        public bool ResourceTracking
        {
            get
            {
                return true;
            }

            set
            {
                
            }
        }

        public bool UseRandomNumbers
        {
            get
            {
                return true;
            }

            set
            {
                
            }
        }

        public bool ValidateBuilding
        {
            get
            {
                return _settings.ValidateBuilding;
            }

            set
            {
                _settings.ValidateBuilding = value;
            }
        }

        public async Task Explorer()
        {
          
            DataPackage dp = new DataPackage();
            dp.SetText(Windows.Storage.ApplicationData.Current.LocalFolder.Path);
            Clipboard.SetContent(dp);
            await Launcher.LaunchFolderAsync(ApplicationData.Current.LocalFolder);
        }

       

        public async Task NewGame()
        {
            await OnNewGame();
        }

        public async Task OpenSavedGame()
        {
            await OnOpenSavedGame();
        }

        public async Task RotateTiles()
        {
            await _gameView.RotateTiles();
            _daRotatePlayers.To += 180;
            _daRotateRolls.To += 180;
            await _sbRotatePlayerAndRolls.ToTask();
        }

      

        public async Task Winner()
        {
            await OnWin();
        }

        public async Task<bool> Reshuffle()
        {
            ContentDialog dlg = new ContentDialog()
            {
                Title = "Catan",
                Content = "\n\nAre you really, really sure that you want to reshuffle the board?\n\nCatan will work with any board.\n\nReally.",
                PrimaryButtonText = "Yes, I admit to defeat!",
                SecondaryButtonText = "No! We'll try it and see how it goes."
            };

            dlg.PrimaryButtonClick += async (o, i) =>
            {

                await _gameView.RandomizeCatanBoard(true);
                await SetStateAsync(CurrentPlayer, GameState.WaitingForStart, true);
                await ProcessEnter(CurrentPlayer, "");


            };



            var ret = await dlg.ShowAsync();


            return ret == ContentDialogResult.Primary;
        }

        public void Close()
        {
           
        }

        private bool _initializeSettings = true;
        public async Task SettingChanged()
        {
            if (_initializeSettings)
                return;

            await _settings.SaveSettings(_settingsFileName);
        }
        public async Task ResetGridLayout()
        {
            foreach (var pos in _settings.GridPositions)
            {
                pos.TranslateX = 0;
                pos.TranslateY = 0;
            }

            UpdateGridLocations();
           await SaveGridLocations();


        }
    }
}