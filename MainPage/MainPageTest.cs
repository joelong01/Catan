using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class MainPage : Page
    {
        private readonly Random testRandom = new Random();

        private void InitTest()
        {
        }

        private async Task LoseHalfYourCards()
        {
            TradeResources tr = new TradeResources()
            {
                Sheep = 3,
                Wheat = 3,
                Ore = 2,
                Brick = 0,
                Wood = 0
            };
            CurrentPlayer.GameData.Resources.GrantResources(tr);

            int loss = (int)CurrentPlayer.GameData.Resources.Current.Count / 2;
            if (loss >= 4)
            {
                ResourceCardCollection rc = new ResourceCardCollection(false);
                rc.AddResources(CurrentPlayer.GameData.Resources.Current);
                TakeCardDlg dlg = new TakeCardDlg()
                {
                    To = MainPageModel.Bank,
                    From = CurrentPlayer,
                    SourceOrientation = TileOrientation.FaceUp,
                    HowMany = loss,
                    Source = rc,
                    Destination = new ResourceCardCollection(false),
                    Instructions = $"Give {loss} cards to the bank."
                };
                var ret = await dlg.ShowAsync();
                if (ret == ContentDialogResult.Primary)
                {
                    CurrentPlayer.GameData.Resources.GrantResources(ResourceCardCollection.ToTradeResources(dlg.Destination).GetNegated());
                }
            }
        }

        private async void OnGrantEntitlements(object sender, RoutedEventArgs e)
        {
            await TestGrantEntitlementMessage();
        }

        private async void OnGrantResources(object sender, RoutedEventArgs e)
        {
            TradeResources tr = new TradeResources()
            {
                Sheep = 3,
                Wheat = 3,
                Ore = 3,
                Brick = 3,
                Wood = 3
            };

            await TestGrantEntitlements.Post(this, tr, new List<Entitlement>(), new List<DevCardType>());
        }

        private async Task TradeGoldTest()
        {

            int goldCards = 2;
            IGameController gameController = this;
            ResourceCardCollection destination = new ResourceCardCollection();
            destination.Clear();
            TradeResources tr = new TradeResources()
            {
                Wood = goldCards,
                Brick = goldCards,
                Wheat = goldCards,
                Ore = goldCards,
                Sheep = goldCards
            };
            ResourceCardCollection source = ResourceCardCollection.Flatten(tr);

            string c = goldCards > 1 ? "cards" : "card";

            TakeCardDlg dlg = new TakeCardDlg()
            {
                To = gameController.TheHuman,
                From = gameController.MainPageModel.Bank,
                SourceOrientation = TileOrientation.FaceUp,
                HowMany = goldCards,
                Source = source,
                Instructions = $"Take {goldCards} {c} from the bank.",
                Destination = destination,
            };

            var ret = await dlg.ShowAsync();
            if (ret != ContentDialogResult.Primary)
            {
                await StaticHelpers.ShowErrorText("Why did you click Cancel?  I'll pick a random resource for you.  No undo.", "Catan");
                Random random = new Random((int)DateTime.Now.Ticks);
                int idx = random.Next(source.Count);
                destination.Add(source[idx]);
            }

            var picked = ResourceCardCollection.ToTradeResources(dlg.Destination);
            this.TraceMessage(picked.ToString());
        }

        // int toggle = 0;
        private async void OnTest1(object sdr, RoutedEventArgs rea)
        {
            CurrentPlayer.GameData.Resources.StolenResource = ResourceType.Wheat;
           // await TradeGoldTest();
            //await TestTargetPlayer();
        }

        private async void OnTest2(object sdr, RoutedEventArgs rea)
        {
            await LoseHalfYourCards();
        }

        // Undo
        private async void OnTest3(object sdr, RoutedEventArgs rea)
        {
            await TestGrantEntitlementMessage();
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

        private async Task TestGrantEntitlementMessage()
        {
            TradeResources tr = new TradeResources()
            {
                Sheep = 3,
                Wheat = 3,
                Ore = 3,
                Brick = 3,
                Wood = 3
            };

            List<Entitlement> entitlements = new List<Entitlement>()
            {
                Entitlement.City,
                Entitlement.Road,
                Entitlement.Road,
                Entitlement.Settlement
            };

            List<DevCardType> devCards = new List<DevCardType>()
            {
                DevCardType.Knight, DevCardType.RoadBuilding, DevCardType.YearOfPlenty, DevCardType.Monopoly, DevCardType.VictoryPoint
            };

            await TestGrantEntitlements.Post(this, tr, entitlements, devCards);
        }

        private void TestStats()
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

        private async Task TestTargetPlayer()
        {
            var source = new ResourceCardCollection(false);            
            var destination = new ResourceCardCollection(false);
            source.ForEach((c) => c.Orientation = TileOrientation.FaceDown);
            TakeCardDlg dlg = new TakeCardDlg()
            {
                To = MainPageModel.Bank,
                From = CurrentPlayer,
                SourceOrientation = TileOrientation.FaceDown,
                HowMany = 1,
                Source = source,
                Destination = new ResourceCardCollection(false),
                Instructions = $"Take a card from {CurrentPlayer.PlayerName}"
            };
            var ret = await dlg.ShowAsync();
            if (ret == ContentDialogResult.Primary)
            {
                CurrentPlayer.GameData.Resources.GrantResources(ResourceCardCollection.ToTradeResources(dlg.Destination).GetNegated());
            }

            this.TraceMessage($"ret= {ret} Cards={ResourceCardCollection.ToTradeResources(dlg.Destination)}");
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

            ResourceCardCollection rc = new ResourceCardCollection(false);
            rc.AddResources(tr);
            TakeCardDlg dlg = new TakeCardDlg()
            {
                To = CurrentPlayer,
                From = MainPageModel.Bank,
                SourceOrientation = TileOrientation.FaceUp,
                HowMany = 2,
                Source = rc,
                Instructions = "Take 2 cards from the bank.",
                Destination = new ObservableCollection<ResourceCardModel>(),
            };

            var ret = await dlg.ShowAsync();
            if (ret == ContentDialogResult.Primary)
            {
                CurrentPlayer.GameData.Resources.GrantResources(ResourceCardCollection.ToTradeResources(dlg.Destination));
            }
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
    }

    public class WSConnectInfo
    {
        public string GameId { get; set; }
        public string PlayerName { get; set; }
    }
}
