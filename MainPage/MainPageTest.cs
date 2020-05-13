using Catan.Proxy;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Media.Audio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;




// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{



    public sealed partial class MainPage : Page, ILog
    {


        private void InitTest()
        {

        }

        private async Task<List<int>> GetRandomOrg()
        {
            List<int> list = new List<int>();
            HttpClient Client = new HttpClient() { Timeout = TimeSpan.FromDays(1) };
            int count = 10000;
            string url = "https://www.random.org/integers/?num=" + count.ToString() + "&min=1&max=6&col=1&base=10&format=plain&rnd=new";
            var response = await Client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string res = await response.Content.ReadAsStringAsync();
                string[] dicerolls = res.Split(new char[] { '\n', ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < dicerolls.Count(); i += 2)
                {

                    list.Add(Int32.Parse(dicerolls[i]) + Int32.Parse(dicerolls[i + 1]));
                }
            }

            return list;
        }

        // int toggle = 0;
        private async void OnTest1(object sdr, RoutedEventArgs rea)
        {




            MersenneTwister twist = new MersenneTwister((int)DateTime.Now.Ticks);
            Random rand = new Random((int)DateTime.Now.Ticks);
            //int roll;
            //int count = 0;
            int[] distribution = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            List<int> list = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                var temp = await GetRandomOrg();
                if (temp != null && temp.Count > 0)
                    list.AddRange(temp);
            }


            foreach (int i in list)
            {
                distribution[i - 2]++;
            }


            //do
            //{
            //    // roll = rand.Next(1, 7) + rand.Next(1, 7);
            //    roll = twist.Next(1, 7) + twist.Next(1, 7);
            //    distribution[roll]++;
            //    count++;
            //} while (count < 200000);
            //this.TraceMessage($"took {count} times");

            distribution.ForEach((n) => Debug.WriteLine(n));



            //Random rand = new Random((int)DateTime.Now.Ticks);
            //foreach (var player in MainPageModel.PlayingPlayers)
            //{
            //    player.GameData.DiceOne = rand.Next(1, 6);
            //    player.GameData.DiceTwo = rand.Next(1, 6);
            //}

            //SyncronizedPlayerRolls playerRolls = new SyncronizedPlayerRolls();

            //playerRolls.Rolls.Add(new SynchronizedRoll() { PlayerName = "Joe", Rolls = new List<int>() { 6 } });
            //playerRolls.Rolls.Add(new SynchronizedRoll() { PlayerName = "James", Rolls = new List<int>() { 10, 9, 4 } });
            //playerRolls.Rolls.Add(new SynchronizedRoll() { PlayerName = "Doug", Rolls = new List<int>() { 7, 6 } });
            //playerRolls.Rolls.Add(new SynchronizedRoll() { PlayerName = "Adrian", Rolls = new List<int>() { 7,8 } });
            //playerRolls.Rolls.Add(new SynchronizedRoll() { PlayerName = "Chris", Rolls = new List<int>() { 10, 9, 5 } });



            //bool ties = playerRolls.HasTies();

            //playerRolls.Sort();

            //var dict = new Dictionary<string, List<int>>();

            //dict["Joe"] = new List<int>() { 10, 9, 5 };
            //dict["Dodgy"] = new List<int>() { 10, 9, 4 };
            //dict["Doug"] = new List<int>() { 9, 12 };
            //dict["Adrian"] = new List<int>() { 8 };
            //dict["Chris"] = new List<int>() { 8, 2 };

            //this.TraceMessage(CatanProxy.Serialize(playerRolls, true));

            //int n = 8;
            //toggle = 1 - toggle;

            //TileOrientation orientation = TileOrientation.FaceUp;
            //if (toggle == 0) orientation = TileOrientation.FaceDown;
            //GameContainer.AllTiles.ForEach((t) => t.SetTileOrientationAsync(orientation, 1000));




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
            //  GameContainer.AllTiles.ForEach((t) => t.AnimateFadeAsync(.25));

            MainPageModel.PlayingPlayers.ForEach((p) => { p.GameData.DiceOne = 0; p.GameData.DiceOne = 0; });

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