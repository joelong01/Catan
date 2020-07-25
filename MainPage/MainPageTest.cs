using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Catan.Proxy;

using System.Text.Json;
using Windows.Services.Maps;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Catan10.CatanService;
using System.Diagnostics.Contracts;
using System.Diagnostics;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class MainPage : Page
    {
        #region Delegates + Fields + Events + Enums

        private readonly Random testRandom = new Random();

        #endregion Delegates + Fields + Events + Enums

        #region Methods

        private void InitTest ()
        {
        }
        private async Task LoseHalfYourCards ()
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
                    CountVisible = true,
                    HowMany = loss,
                    Source = rc,
                    Destination = new ResourceCardCollection(false),
                    Instructions = $"Give {loss} cards to the bank."
                };
                var ret = await dlg.ShowAsync();
                if (ret == ContentDialogResult.Primary)
                {
                    CurrentPlayer.GameData.Resources.GrantResources(ResourceCardCollection.ToTradeResources(dlg.Destination).GetNegated(), false);
                }
            }
        }

        private async void Menu_OnResetService (object sender, RoutedEventArgs e)
        {

            await CreateAndConfigureProxy();



        }
        private async void OnGrantEntitlements (object sender, RoutedEventArgs e)
        {
            await TestGrantEntitlementMessage();
        }

        private async void OnGrantResources (object sender, RoutedEventArgs e)
        {
            TradeResources tr = new TradeResources()
            {
                Sheep = 0,
                Wheat = 0,
                Ore = 2,
                Brick = 1,
                Wood = 1
            };

            await TestGrantEntitlements.Post(this, tr, new List<Entitlement>(), new List<DevCardType>());
        }

        // int toggle = 0;
        private async void OnTest1 (object sdr, RoutedEventArgs rea)
        {

            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".json");


            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file == null)
            {
                return;
            }


            string json =await FileIO.ReadTextAsync(file);

            List<CatanMessage> messages = CatanSignalRClient.Deserialize<List<CatanMessage>>(json);
            await DoReplay(messages);
        }

        /// <summary>
        ///     the game goes through these basic steps
        ///     1. Create the "game" ... this is the communication channel the game uses to send messages to all the players
        ///     2. Join the "game"
        ///     3. Send Broadcast Messages to the clients 
        ///         => each of these are processed via ProcessMessage()
        /// </summary>
        /// <param name="messages"></param>
        /// <returns></returns>
        private async Task DoReplay (List<CatanMessage> messages)
        {
            if (messages == null) return;
            if (messages.Count == 0) return;
            await EndGame();
            GameInfo gameInfo;
            for (int i=0; i<messages.Count; i++)
            {

                if (messages[i].MessageType == MessageType.Ack) continue;

                CatanMessage parsedMessage = null;
                try
                {
                     parsedMessage =  CatanSignalRClient.ParseMessage(messages[i]);
                }
                catch(Exception e)
                {
                    this.TraceMessage($"{e}");
                    Debugger.Break();
                    continue;
                }
                
                 switch (parsedMessage.MessageType)
                {
                    case MessageType.PrivateMessage:
                    case MessageType.BroadcastMessage:
                        if (messages[i + 1].MessageType == MessageType.Ack)
                        {
                            var ack = CatanSignalRClient.ParseMessage(messages[i+1]);
                            Debug.Assert(((AckModel)ack.Data).AckedMessageId == parsedMessage.MessageId);
                            if (parsedMessage.ActionType == ActionType.Normal)
                            {
                                parsedMessage.ActionType = ActionType.Replay;
                            }
                            MainPageModel.UnprocessedMessages++;
                            await ProcessMessage(parsedMessage);
                        }
                        break;
                    case MessageType.CreateGame:
                        //  parsedMessage.ActionType = ActionType.Retry;
                        //  await this.Proxy.CreateGame(parsedMessage.GameInfo);
                        gameInfo = parsedMessage.GameInfo;
                        break;
                    case MessageType.DeleteGame:
                        break;
                    case MessageType.JoinGame:
                        await JoinOrCreateGame(parsedMessage.GameInfo);
                        break;
                    case MessageType.LeaveGame:
                        break;
                    case MessageType.Ack:
                        break;
                    default:
                        break;
                }

            }

        }

        private async void OnTest2 (object sdr, RoutedEventArgs rea)
        {
            await LoseHalfYourCards();
        }

        // Undo
        private async void OnTest3 (object sdr, RoutedEventArgs rea)
        {
            await TestGrantEntitlementMessage();
        }

        private void OnTestExpansionGame (object sender, RoutedEventArgs e)
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

        private void OnTestRegularGame (object sender, RoutedEventArgs e)
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

        private async void OnTestService (object sender, RoutedEventArgs e)
        {
            await CreateAndConfigureProxy();

            Guid id = Guid.Parse("{A2D8D755-9015-41F3-9CF3-560B2BE758EF}");
            string gameName = "Test_Game_{A2D8D755-9015-41F3-9CF3-560B2BE758EF}";
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            List<GameInfo> games = await MainPageModel.CatanService.GetAllGames();
            MainPageModel.CatanService.OnGameDeleted += CatanService_OnGameDeleted;

            void CatanService_OnGameDeleted (GameInfo gameInfo, string by)
            {
                this.TraceMessage($"Deleted game={gameInfo} by {by}");
                if (gameInfo.Id == id)
                {
                    MainPageModel.CatanService.OnGameDeleted -= CatanService_OnGameDeleted;
                    tcs.TrySetResult(null);
                }
            }



            foreach (var game in games)
            {
                this.TraceMessage($"Found game={game.Name}");
                if (game.Name == gameName)
                {
                    await MainPageModel.CatanService.DeleteGame(game, TheHuman.PlayerName);
                    await tcs.Task;
                }
            }

            tcs = new TaskCompletionSource<object>();

            MainPageModel.CatanService.OnGameCreated += CatanService_OnGameCreated;
            void CatanService_OnGameCreated (GameInfo gameInfo, string playerName)
            {
                if (gameInfo.Id == id)
                {
                    MainPageModel.CatanService.OnGameCreated -= CatanService_OnGameCreated;
                    tcs.TrySetResult(null);
                }
            }

            GameInfo newGame = new GameInfo()
            {
                Id = id,
                Name = gameName,
                Creator = TheHuman.PlayerName,
                RequestAutoJoin = false,
                Started = false
            };

            await MainPageModel.CatanService.CreateGame(newGame);
            await tcs.Task;


            tcs = new TaskCompletionSource<object>();
            MainPageModel.CatanService.OnGameJoined += CatanService_OnGameJoined;
            void CatanService_OnGameJoined (GameInfo gameInfo, string playerName)
            {
                if (playerName == TheHuman.PlayerName && gameInfo.Id == id)
                {
                    tcs.TrySetResult(null);
                    MainPageModel.CatanService.OnGameJoined -= CatanService_OnGameJoined;
                }
            }
            await MainPageModel.CatanService.JoinGame(newGame, TheHuman.PlayerName);

            await tcs.Task;


            var players = await MainPageModel.CatanService.GetAllPlayerNames(id);
            foreach (var name in players)
            {
                if (name == TheHuman.PlayerName)
                {
                    this.TraceMessage("found player");
                }
            }

        }

        private async Task TestGrantEntitlementMessage ()
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

        private void TestStats ()
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

        private async Task TestTargetPlayer ()
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
                CurrentPlayer.GameData.Resources.GrantResources(ResourceCardCollection.ToTradeResources(dlg.Destination).GetNegated(), false);
            }

            this.TraceMessage($"ret= {ret} Cards={ResourceCardCollection.ToTradeResources(dlg.Destination)}");
        }

        private void TestTrade ()
        {

            TradeResources tr = new TradeResources()
            {
                Sheep = 3,
                Wheat = 3,
                Ore = 3,
                Brick = 3,
                Wood = 3
            };


            MainPageModel.PlayingPlayers.ForEach((p) => p.Reset());
            MainPageModel.PlayingPlayers.Clear();

            MainPageModel.PlayingPlayers.Add(MainPageModel.AllPlayers[0]);
            MainPageModel.PlayingPlayers.Add(MainPageModel.AllPlayers[1]);
            MainPageModel.PlayingPlayers.Add(MainPageModel.AllPlayers[2]);




            var player1 = MainPageModel.AllPlayers[1];
            var player2 = MainPageModel.AllPlayers[0];

            player1.GameData.Resources.GrantResources(tr);
            player2.GameData.Resources.GrantResources(tr);

            CurrentPlayer = MainPageModel.PlayingPlayers[2];
            //  CurrentPlayer.GameData.Trades.TradeRequest.AddPotentialTradingPartners(MainPageModel.PlayingPlayers);
            TheHuman = CurrentPlayer;
            CurrentPlayer.GameData.Trades.TradeRequest.Owner.Player = TheHuman;
            //foreach (var player in MainPageModel.PlayingPlayers)
            //{
            //    if (player == CurrentPlayer) continue;

            //    CurrentPlayer.GameData.Trades.PotentialTrades.Add(new TradeOffer()
            //    {
            //        Desire = new TradeResources()
            //        {
            //            Wood = 1
            //        },
            //        Offer = new TradeResources()
            //        {
            //            Brick = 1
            //        },
            //        Owner = CurrentPlayer,
            //        TradePartner = player,
            //        OwnerApproved = true,
            //        PartnerApproved = false,

            //    });
            //}

            //
            //  joe offer dodgy 2 brick for 1 wheat
            //CurrentPlayer.GameData.Trades.PotentialTrades.Add(new TradeOffer()
            //{
            //    Desire = new TradeResources()
            //    {
            //        Wheat = 1
            //    },
            //    Offer = new TradeResources()
            //    {
            //        Brick = 2
            //    },
            //    Owner = MainPageModel.PlayingPlayers[0],
            //    OwnerApproved = true,
            //    PartnerApproved = false,

            //});


        }

        private void TestTrades2 ()
        {
            MainPageModel.Settings.IsLocalGame = true;
            GameInfo info = new GameInfo()
            {
                Creator = "Joe",
                Id = Guid.NewGuid(),
                Started = false
            };


            MainPageModel.PlayingPlayers.Clear();

            MainPageModel.PlayingPlayers.Add(MainPageModel.AllPlayers[0]);
            MainPageModel.PlayingPlayers.Add(MainPageModel.AllPlayers[1]);
            MainPageModel.PlayingPlayers.Add(MainPageModel.AllPlayers[2]);
            MainPageModel.PlayingPlayers.Add(MainPageModel.AllPlayers[3]);
            MainPageModel.PlayingPlayers.Add(MainPageModel.AllPlayers[4]);
            CurrentPlayer.GameData.Trades.PotentialTrades.Clear();

            CurrentPlayer.GameData.Trades.PotentialTrades.Add(new TradeOffer()
            {
                Owner = new Offer()
                {
                    Approved = false,
                    Player = CurrentPlayer,
                    Resources = new TradeResources()
                    {
                        Brick = 1,
                        Wood = 1,
                        Wheat = 1,
                        Ore = 1
                    }
                },
                Partner = new Offer()
                {
                    Approved = false,
                    Player = MainPageModel.AllPlayers[1],
                    Resources = new TradeResources()
                    {
                        Sheep = 1
                    }
                }

            });

            CurrentPlayer.GameData.Trades.PotentialTrades.Add(new TradeOffer()
            {
                Owner = new Offer()
                {
                    Approved = false,
                    Player = MainPageModel.AllPlayers[1],
                    Resources = new TradeResources()
                    {
                        Sheep = 1
                    }
                },
                Partner = new Offer()
                {
                    Approved = false,
                    Player = MainPageModel.AllPlayers[2],
                    Resources = new TradeResources()
                    {
                        Brick = 1,
                        Wood = 1,
                        Wheat = 1,
                        Ore = 1
                    }
                }

            });

            CurrentPlayer.GameData.Trades.PotentialTrades.Add(new TradeOffer()
            {
                Owner = new Offer()
                {
                    Approved = false,
                    Player = MainPageModel.AllPlayers[2],
                    Resources = new TradeResources()
                    {
                        Brick = 1
                    }
                },
                Partner = new Offer()
                {
                    Approved = false,
                    Player = MainPageModel.AllPlayers[3],
                    Resources = new TradeResources()
                    {
                        Wood = 1
                    }
                }

            });

        }

        private async Task TestYearOfPlenty ()
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
            // rc = rc.Flatten();
            TakeCardDlg dlg = new TakeCardDlg()
            {
                To = CurrentPlayer,
                From = MainPageModel.Bank,
                SourceOrientation = TileOrientation.FaceUp,
                HowMany = 2,
                Source = rc,
                CountVisible = true,
                Instructions = "Take 2 cards from the bank.",
                Destination = new ObservableCollection<ResourceCardModel>(),
            };

            var ret = await dlg.ShowAsync();
            if (ret == ContentDialogResult.Primary)
            {
                CurrentPlayer.GameData.Resources.GrantResources(ResourceCardCollection.ToTradeResources(dlg.Destination));
            }
            this.TraceMessage($"{ResourceCardCollection.ToTradeResources(dlg.Destination)}");
        }

        private async Task TradeGoldTest ()
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
            ResourceCardCollection source = new ResourceCardCollection(tr);

            string c = goldCards > 1 ? "cards" : "card";

            TakeCardDlg dlg = new TakeCardDlg()
            {
                To = gameController.TheHuman,
                From = gameController.MainPageModel.Bank,
                SourceOrientation = TileOrientation.FaceUp,
                HowMany = goldCards,
                CountVisible = true,
                Source = source,
                Instructions = $"Take {goldCards} {c} from the bank.",
                Destination = destination,
            };

            var ret = await dlg.ShowAsync();
            if (ret != ContentDialogResult.Primary)
            {
                await ShowErrorMessage("Why did you click Cancel?  I'll pick a random resource for you.  No undo.", "Catan", "");
                Random random = new Random((int)DateTime.Now.Ticks);
                int idx = random.Next(source.Count);
                destination.Add(source[idx]);
            }

            var picked = ResourceCardCollection.ToTradeResources(dlg.Destination);
            this.TraceMessage("trade gold: " + picked.ToString());
        }
        private void VerifyRoundTrip<T> (T model)
        {
            //var options = new JsonSerializerOptions() { WriteIndented = true };
            //options.Converters.Add(new JsonStringEnumConverter());
            //var jsonString = JsonSerializer.Serialize<T>(model, options);
            //T newModel = JsonSerializer.Deserialize<T>(jsonString, options);
            //var newJsonString = JsonSerializer.Serialize<T>(newModel, options);
            ////   this.TraceMessage(newJsonString);
            //Debug.Assert(newJsonString == jsonString);
        }

        #endregion Methods
    }


}
