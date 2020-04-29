using System;
using System.Threading.Tasks;
using Catan.Proxy;
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
            get => _transformGameView.ScaleX;

            set
            {
                SavedAppState.Settings.Zoom = value;
                _transformGameView.ScaleX = value;
                _transformGameView.ScaleY = value;

            }
        }

        public bool RotateTile
        {
            get => SavedAppState.Settings.RotateTile;

            set
            {

                SavedAppState.Settings.RotateTile = value;
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
            get => SavedAppState.Settings.AnimateFade;

            set => SavedAppState.Settings.AnimateFade = value;
        }

        public int FadeSeconds
        {
            get => SavedAppState.Settings.FadeSeconds;

            set
            {
                SavedAppState.Settings.FadeSeconds = value;
                _timer.Interval = TimeSpan.FromSeconds(value);
                if (value == 0)
                {
                    _timer.Interval = TimeSpan.FromMilliseconds(250);
                }
            }
        }

        public bool ShowStopwatch
        {
            get => SavedAppState.Settings.ShowStopwatch;

            set => SavedAppState.Settings.ShowStopwatch = value;
        }

        public bool UseClassicTiles
        {
            get => true;

            set
            {

            }
        }

        public int AnimationSpeedBase
        {
            get
            {
                if (SavedAppState.Settings.AnimationSpeed > 10)
                {
                    return 4;
                }
                else
                {
                    return SavedAppState.Settings.AnimationSpeed;
                }
            }

            set
            {
                SavedAppState.Settings.AnimationSpeed = value;
                if (value >= 4)
                {
                    SavedAppState.Settings.AnimationSpeed = 10;
                }
            }
        }

        public bool ResourceTracking
        {
            get => true;

            set
            {

            }
        }

        public bool UseRandomNumbers
        {
            get => true;

            set
            {

            }
        }

        public bool ValidateBuilding
        {
            get => SavedAppState.Settings.ValidateBuilding;

            set => SavedAppState.Settings.ValidateBuilding = value;
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

                await _gameView.SetRandomCatanBoard(true);
                await SetStateAsync(CurrentPlayer, GameState.WaitingForStart, true);
                //  await ProcessEnter(CurrentPlayer, "");


            };



            ContentDialogResult ret = await dlg.ShowAsync();


            return ret == ContentDialogResult.Primary;
        }

        public void Close()
        {

        }

        private bool _initializeSettings = true;
        public async Task SettingChanged()
        {
            if (_initializeSettings)
            {
                return;
            }

            await SaveSettings();
        }
        public async Task ResetGridLayout()
        {
            foreach (GridPosition pos in SavedAppState.Settings.GridPositions)
            {
                pos.TranslateX = 0;
                pos.TranslateY = 0;
                pos.ScaleX = 1.0;
                pos.ScaleY = 1.0;
            }

            UpdateGridLocations();
            await SaveGridLocations();


        }
    }
}