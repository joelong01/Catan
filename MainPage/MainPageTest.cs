using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Catan.Proxy;

using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class MainPage : Page
    {
        private void InitTest()
        {
        }

        private async Task LoseHalfYourCards()
        {
            int loss = (int)CurrentPlayer.GameData.Resources.Current.Count / 2;
            if (loss >= 4)
            {
                ResourceCardCollection rc = new ResourceCardCollection();
                rc.InitalizeResources(CurrentPlayer.GameData.Resources.Current);
                TakeCardControl.To = MainPageModel.Bank;
                TakeCardControl.From = CurrentPlayer;
                TakeCardControl.FaceUp = true;
                TakeCardControl.HowMany = loss;
                TakeCardControl.Source = rc;
                TakeCardControl.Destination = new ResourceCardCollection();
                Grid_TakeCard.Visibility = Visibility.Visible;
                (bool ret, TradeResources trAdded) = await TakeCardControl.GetCards();
                if (ret)
                {
                    CurrentPlayer.GameData.Resources.GrantResources(trAdded.GetNegated());
                }
            }
        }

        // int toggle = 0;
        private void OnTest1(object sdr, RoutedEventArgs rea)
        {
            CurrentPlayer.GameData.Score++;

            CurrentPlayer.GameData.RollsWithResource++;
            CurrentPlayer.GameData.TimesTargeted++;
            CurrentPlayer.GameData.LargestArmy = true;
            CurrentPlayer.GameData.LongestRoad = 12;
            CurrentPlayer.GameData.MaxNoResourceRolls++;
            CurrentPlayer.GameData.Resources.ResourcesLostToMonopoly = new TradeResources() { Wheat = 7 };
            CurrentPlayer.GameData.Resources.ResourcesLostSeven = new TradeResources() { Ore = 3, Wheat = 2, Wood = 5 };
            CurrentPlayer.GameData.Resources.ResourcesLostToBaron = new TradeResources() { Ore = 10 };
            CurrentPlayer.GameData.NoResourceCount = 13;
            CurrentPlayer.GameData.GoldRolls = 5;
            CurrentPlayer.GameData.Resources.TotalResources = new TradeResources() { Ore = 3, Wheat = 2, Wood = 5, Brick = 10 };
        }

        private void OnTest2(object sdr, RoutedEventArgs rea)
        {
            TradeResources tr = new TradeResources()
            {
                Sheep = 3,
                Wheat = 3,
                Ore = 3,
                Brick = 3,
                Wood = 3
            };

            // CurrentPlayer.GameData.Resources.ResourcesThisTurn2.AllUp();

            CurrentPlayer.GameData.Resources.GrantResources(tr);
            CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.City);
            CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.City);
            CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.Settlement);
            CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.Road);
            CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.Road);
            CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.Road);

            CurrentPlayer.GameData.Resources.AddDevCard(DevCardType.Knight);
            CurrentPlayer.GameData.Resources.AddDevCard(DevCardType.YearOfPlenty);
            CurrentPlayer.GameData.Resources.AddDevCard(DevCardType.Monopoly);
            CurrentPlayer.GameData.Resources.MakeDevCardsAvailable();
            CurrentPlayer.GameData.Resources.AddDevCard(DevCardType.RoadBuilding);
            CurrentPlayer.GameData.Resources.AddDevCard(DevCardType.VictoryPoint);
            CurrentPlayer.GameData.Resources.PlayDevCard(DevCardType.Knight);
        }

        // Undo
        private async void OnTest3(object sdr, RoutedEventArgs rea)
        {
            await LoseHalfYourCards();
        }

        private void OnTestExpansionGame(object sender, RoutedEventArgs e)
        {
            //AnimationSpeedBase = 10; // speed up the animations
            //RandomGoldTileCount = 3;
            //await this.Reset();
            //// await MainPageModel.Log.Init(CreateSaveFileName("Expansion Game"));
            //await SetStateAsync(null, GameState.WaitingForNewGame, true);
            //_gameView.CurrentGame = _gameView.Games[1];

            ////   SavedGames.Insert(0, MainPageModel.Log);
            ////   await AddLogEntry(null, GameState.GamePicked, CatanAction.SelectGame, true, LogType.Normal, 1);
            //List<PlayerModel> PlayerDataList = new List<PlayerModel>
            //{
            //    MainPageModel.AllPlayers[0],
            //    MainPageModel.AllPlayers[1],
            //    MainPageModel.AllPlayers[2],
            //    MainPageModel.AllPlayers[3],
            //    MainPageModel.AllPlayers[4],
            //};
            //await StartGame(PlayerDataList, 1);
            //await NextState(); // simluates pushing "Start"
            //CurrentPlayer = MainPageModel.PlayingPlayers[0];
            //await PickSettlementsAndRoads();
        }

        private void OnTestRegularGame(object sender, RoutedEventArgs e)
        {
            //AnimationSpeedBase = 10; // speed up the animations

            //await this.Reset();
            ////   await MainPageModel.Log.Init(CreateSaveFileName("Test Game"));

            //await SetStateAsync(null, GameState.WaitingForNewGame, true);
            //_gameView.CurrentGame = _gameView.Games[0];

            ////  SavedGames.Insert(0, MainPageModel.Log);
            //await AddLogEntry(null, GameState.GamePicked, CatanAction.SelectGame, true, LogType.Normal, 0);
            //List<PlayerModel> PlayerDataList = new List<PlayerModel>
            //{
            //    MainPageModel.AllPlayers[0],
            //    MainPageModel.AllPlayers[1],
            //    MainPageModel.AllPlayers[2],
            //    MainPageModel.AllPlayers[3]
            //};
            //await StartGame(PlayerDataList, 0);
            //await NextState(); // simluates pushing "Start"
            //CurrentPlayer = MainPageModel.PlayingPlayers[0];
            //await PickSettlementsAndRoads();
        }

        private async Task TestPickTwoCards()
        {
            var source = new ResourceCardCollection();
            source.Add(ResourceType.Wheat, false);
            source.Add(ResourceType.Wheat, false);
            source.Add(ResourceType.Wood, false);
            source.Add(ResourceType.Brick, false);
            var destination = new ResourceCardCollection();
            TakeCardControl.From = MainPageModel.AllPlayers[0];
            TakeCardControl.To = MainPageModel.AllPlayers[1];
            Grid_TakeCard.Visibility = Visibility.Visible;
            TakeCardControl.HowMany = 2;
            TakeCardControl.Source = source;
            TakeCardControl.Destination = destination;
            (bool ret, TradeResources trAdded) = await TakeCardControl.GetCards();

            this.TraceMessage($"ret= {ret} Cards={trAdded}");
        }

        private async Task TestYearOfPlenty()
        {
            TradeResources tr = new TradeResources()
            {
                Wood = 2,
                Wheat = 2,
                Brick = 2,
                Ore = 2,
                Sheep = 2
            };

            ResourceCardCollection rc = new ResourceCardCollection();
            rc.InitalizeResources(tr);
            Grid_TakeCard.Visibility = Visibility.Visible;
            TakeCardControl.To = CurrentPlayer;
            TakeCardControl.From = MainPageModel.Bank;
            TakeCardControl.FaceUp = true;
            TakeCardControl.HowMany = 2;
            TakeCardControl.Source = rc;
            TakeCardControl.Destination = new ObservableCollection<ResourceCardModel>();
            (bool ret, TradeResources addedResources) = await TakeCardControl.GetCards();

            CurrentPlayer.GameData.Resources.GrantResources(addedResources);
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

        private readonly Random testRandom = new Random();
    }

    public class WSConnectInfo
    {
        public string GameId { get; set; }
        public string PlayerName { get; set; }
    }
}
