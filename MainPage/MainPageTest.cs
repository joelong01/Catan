using Catan.Proxy;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;




// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{


    public sealed partial class MainPage : Page, ILog
    {


        private void InitTest()
        {

        }
        int toggle = 0;
        private async void OnTest1(object sdr, RoutedEventArgs rea)
        {
            int n = 8;
            toggle = 1 - toggle;

            TileOrientation orientation = TileOrientation.FaceUp;
            if (toggle == 0) orientation = TileOrientation.FaceDown;
            GameContainer.AllTiles.ForEach((t) => t.SetTileOrientationAsync(orientation, 1000));
            
          


            //await PlayRollAnimation(n);

            //int count = 0;

            //do
            //{
            //    count++;
            //    RandomBoardLog log = await RandomBoardLog.RandomizeBoard(this, 0);
            //    UpdateBoardMeasurements();

            //    if (count > 1000) break;

            //} while (PipCount.Wheat < 10 || PipCount.Wood < 10 || PipCount.Ore < 10 ||
            //        PipCount.Brick < 10 || PipCount.Sheep < 10 || MainPageModel.FiveStarPositions < 1);


            //this.TraceMessage($"it took {count} times");

        }
        private void OnTest2(object sdr, RoutedEventArgs rea)
        {
            GameContainer.AllTiles.ForEach((t) => t.AnimateFadeAsync(.25));

            // change player
            //ChangePlayerLog changedPlayer = await ChangePlayerLog.ChangePlayer(this, 1, GameState.WaitingForRoll);
            //VerifyRoundTrip<ChangePlayerLog>(changedPlayer);
        }
        // Undo
        private void OnTest3(object sdr, RoutedEventArgs rea)
        {
            
        }

        private void VerifyRoundTrip<T>(T model)
        {
            //var options = new JsonSerializerOptions() { WriteIndented = true };
            //options.Converters.Add(new JsonStringEnumConverter());
            //var jsonString = JsonSerializer.Serialize<T>(model, options);
            //T newModel = JsonSerializer.Deserialize<T>(jsonString, options);
            //var newJsonString = JsonSerializer.Serialize<T>(newModel, options);
            ////   this.TraceMessage(newJsonString);
            //Debug.Assert(newJsonString == jsonString);

        }

        readonly Random testRandom = new Random();

        private async void OnTestRegularGame(object sender, RoutedEventArgs e)
        {
            AnimationSpeedBase = 10; // speed up the animations


            await this.Reset();
            //   await MainPageModel.Log.Init(CreateSaveFileName("Test Game"));

            await SetStateAsync(null, GameState.WaitingForNewGame, true);
            _gameView.CurrentGame = _gameView.Games[0];

            //  SavedGames.Insert(0, MainPageModel.Log);
            await AddLogEntry(null, GameState.GamePicked, CatanAction.SelectGame, true, LogType.Normal, 0);
            List<PlayerModel> PlayerDataList = new List<PlayerModel>
            {
                SavedAppState.AllPlayers[0],
                SavedAppState.AllPlayers[1],
                SavedAppState.AllPlayers[2],
                SavedAppState.AllPlayers[3]
            };
            await StartGame(PlayerDataList, 0);
            await NextState(); // simluates pushing "Start"
            CurrentPlayer = MainPageModel.PlayingPlayers[0];
            await PickSettlementsAndRoads();


        }
        private async void OnTestExpansionGame(object sender, RoutedEventArgs e)
        {
            AnimationSpeedBase = 10; // speed up the animations
            RandomGoldTileCount = 3;
            await this.Reset();
            // await MainPageModel.Log.Init(CreateSaveFileName("Expansion Game"));
            await SetStateAsync(null, GameState.WaitingForNewGame, true);
            _gameView.CurrentGame = _gameView.Games[1];

            //   SavedGames.Insert(0, MainPageModel.Log);
            //   await AddLogEntry(null, GameState.GamePicked, CatanAction.SelectGame, true, LogType.Normal, 1);
            List<PlayerModel> PlayerDataList = new List<PlayerModel>
            {
                SavedAppState.AllPlayers[0],
                SavedAppState.AllPlayers[1],
                SavedAppState.AllPlayers[2],
                SavedAppState.AllPlayers[3],
                SavedAppState.AllPlayers[4],
            };
            await StartGame(PlayerDataList, 1);
            await NextState(); // simluates pushing "Start"
            CurrentPlayer = MainPageModel.PlayingPlayers[0];
            await PickSettlementsAndRoads();


        }
    }
}