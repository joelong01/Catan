using System;
using System.Diagnostics.Contracts;
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
                MainPageModel.Settings.Zoom = value;
                _transformGameView.ScaleX = value;
                _transformGameView.ScaleY = value;

            }
        }

        public bool RotateTile
        {
            get => MainPageModel.Settings.RotateTile;

            set
            {

                MainPageModel.Settings.RotateTile = value;
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
            get => MainPageModel.Settings.AnimateFade;

            set => MainPageModel.Settings.AnimateFade = value;
        }

        public int FadeSeconds
        {
            get => MainPageModel.Settings.FadeSeconds;

            set
            {
                MainPageModel.Settings.FadeSeconds = value;

            }
        }

        public bool ShowStopwatch
        {
            get => MainPageModel.Settings.ShowStopwatch;

            set => MainPageModel.Settings.ShowStopwatch = value;
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
                if (MainPageModel.Settings.AnimationSpeed > 10)
                {
                    return 4;
                }
                else
                {
                    return MainPageModel.Settings.AnimationSpeed;
                }
            }

            set
            {
                MainPageModel.Settings.AnimationSpeed = value;
                if (value >= 4)
                {
                    MainPageModel.Settings.AnimationSpeed = 10;
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
            get => MainPageModel.Settings.ValidateBuilding;

            set => MainPageModel.Settings.ValidateBuilding = value;
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

            await SaveGameState();
        }
        public async Task ResetGridLayout()
        {
            foreach (var kvp in MainPageModel.Settings.GridPositions)
            {
                GridPosition pos = kvp.Value;                
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