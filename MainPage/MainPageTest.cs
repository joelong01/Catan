using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics;
using System.Collections.Generic;
using Windows.Storage;
using Catan.Proxy;




// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{


    public sealed partial class MainPage : Page, ILog
    {
        private NewLog NewLog;

        private void InitTest()
        {
            NewLog = new NewLog(this);
        }

        private async void OnTest1(object sdr, RoutedEventArgs rea)
        {
            var proxy = new CatanProxy()
            {
                HostName = "http://localhost:5000"
            };
            var gameInfo = new GameInfo();
            string[] gameNames = new string[] { "Game - 1", "Game - 2" };
            List<string> games = null;
            foreach (var game in gameNames)
            {
                await proxy.DeleteGame(game);
                games = await proxy.CreateGame(game, gameInfo);

                for (int i=0; i<4; i++) 
                {
                    var player = SavedAppState.Players[i];
                    if (player.PlayerName == "Dodgy") continue;
                    var resources = await proxy.JoinGame(game, player.PlayerName);

                    Debug.Assert(resources != null);
                }
            }
            
            
            
            


            ServiceGameDlg dlg = new ServiceGameDlg(this.SavedAppState)
            {
                HostName = proxy.HostName
            };
            await dlg.ShowAsync();
            this.TraceMessage($"{JsonSerializer.Serialize<PlayerModel>(dlg?.SelectedPlayer)}");

        }
        private async void OnTest2(object sdr, RoutedEventArgs rea)
        {
            // change player
            ChangedPlayerModel changedPlayer = await ChangedPlayerController.ChangePlayer(this, 1, GameState.WaitingForRoll);
            VerifyRoundTrip<ChangedPlayerModel>(changedPlayer);

            NewLog.PushAction(changedPlayer);
        }
        // Undo
        private async void OnTest3(object sdr, RoutedEventArgs rea)
        {
            await NewLog.Undo();

            // NewLog.Redo();
        }

        private void VerifyRoundTrip<T>(T model)
        {
            var options = new JsonSerializerOptions() { WriteIndented = true };
            options.Converters.Add(new JsonStringEnumConverter());
            var jsonString = JsonSerializer.Serialize<T>(model, options);
            T newModel = JsonSerializer.Deserialize<T>(jsonString, options);
            var newJsonString = JsonSerializer.Serialize<T>(newModel, options);
         //   this.TraceMessage(newJsonString);
            Debug.Assert(newJsonString == jsonString);

        }
        Random testRandom = new Random();

        private async void OnTestRegularGame(object sender, RoutedEventArgs e)
        {
            AnimationSpeedBase = 10; // speed up the animations
           

            await this.Reset();
            await MainPageModel.Log.Init(CreateSaveFileName("Test Game"));

            await SetStateAsync(null, GameState.WaitingForNewGame, true);
            _gameView.CurrentGame = _gameView.Games[0];
            
            SavedGames.Insert(0, MainPageModel.Log);
            await AddLogEntry(null, GameState.GamePicked, CatanAction.SelectGame, true, LogType.Normal, 0);
            List<PlayerModel> PlayerDataList = new List<PlayerModel>
            {
                SavedAppState.Players[0],
                SavedAppState.Players[1],
                SavedAppState.Players[2],
                SavedAppState.Players[3]
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
            await MainPageModel.Log.Init(CreateSaveFileName("Expansion Game"));
            await SetStateAsync(null, GameState.WaitingForNewGame, true);
            _gameView.CurrentGame = _gameView.Games[1];
            
            SavedGames.Insert(0, MainPageModel.Log);
            await AddLogEntry(null, GameState.GamePicked, CatanAction.SelectGame, true, LogType.Normal, 1);
            List<PlayerModel> PlayerDataList = new List<PlayerModel>
            {
                SavedAppState.Players[0],
                SavedAppState.Players[1],
                SavedAppState.Players[2],
                SavedAppState.Players[3],
                SavedAppState.Players[4],
            };
            await StartGame(PlayerDataList, 1);
            await NextState(); // simluates pushing "Start"
            CurrentPlayer = MainPageModel.PlayingPlayers[0];
            await PickSettlementsAndRoads();


        }
    }
}