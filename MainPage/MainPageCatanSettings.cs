using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml.Controls;

namespace Catan10
{
    public sealed partial class MainPage : Page, ICatanSettings
    {
        private bool _initializeSettings = true;

          public bool AnimateFade
        {
            get => MainPageModel.Settings.AnimateFade;

            set => MainPageModel.Settings.AnimateFade = value;
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

        public int FadeSeconds
        {
            get => MainPageModel.Settings.FadeSeconds;

            set
            {
                MainPageModel.Settings.FadeSeconds = value;
            }
        }

        public bool ResourceTracking
        {
            get => true;

            set
            {
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
                    foreach (TileCtrl t in CTRL_GameView.CurrentGame.Tiles)
                    {
                        t.ResetTileRotation();
                    }
                }
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

        public double Zoom
        {
            get => 1.0; // _transformGameView.ScaleX;

            set
            {
                //MainPageModel.Settings.Zoom = value;
                //_transformGameView.ScaleX = value;
                //_transformGameView.ScaleY = value;
            }
        }

        public void Close()
        {
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

        public async Task ResetGridLayout()
        {
            string json = @"{""LocalPurchase"": {""ScaleX"": 1,""ScaleY"": 1,""TranslateX"": 0,""TranslateY"": 0},""PlayerTradeGrid"":{""ScaleX"":1,""ScaleY"":1,""TranslateX"":-635,""TranslateY"":-199},""SynchronizedRolls"":{""ScaleX"":1,""ScaleY"":1,""TranslateX"":4,""TranslateY"":427},""Grid_BoardMeasurement"":{""ScaleX"":1,""ScaleY"":1,""TranslateX"":389,""TranslateY"":391},""Grid_RollStats"":{""ScaleX"":1,""ScaleY"":1,""TranslateX"":-850,""TranslateY"":248},""ControlGrid"":{""ScaleX"":1,""ScaleY"":1,""TranslateX"":-270,""TranslateY"":330},""Draggable_PrivateData"":{""ScaleX"":1,""ScaleY"":1,""TranslateX"":-568,""TranslateY"":248},""CTRL_GameView"":{""ScaleX"":0.60000002384185791,""ScaleY"":0.60000002384185791,""TranslateX"":138,""TranslateY"":-215},  ""DGC_Game"": {""ScaleX"": 1,""ScaleY"": 1,""TranslateX"": 0,""TranslateY"": 0}}";
            MainPageModel.Settings.GridPositions = JsonSerializer.Deserialize<Dictionary<string, GridPosition>>(json);
            //foreach (var kvp in MainPageModel.Settings.GridPositions)
            //{
            //    GridPosition pos = kvp.Value;
            //    pos.TranslateX = 0;
            //    pos.TranslateY = 0;
            //    pos.ScaleX = 1.0;
            //    pos.ScaleY = 1.0;
            //}

            UpdateGridLocations();
            await SaveGridLocations();
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
                await CTRL_GameView.SetRandomCatanBoard(true);
            };

            ContentDialogResult ret = await dlg.ShowAsync();
            return ret == ContentDialogResult.Primary;
        }

        public async Task RotateTiles()
        {
            await CTRL_GameView.RotateTiles();
            _daRotatePlayers.To += 180;
            _daRotateRolls.To += 180;
            await _sbRotatePlayerAndRolls.ToTask();
        }

        public async Task SettingChanged()
        {
            if (_initializeSettings)
            {
                return;
            }

            await SaveGameState();
        }

        public async Task Winner()
        {
            await OnWin();
        }
    }
}
