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
using Windows.System;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;
using Windows.Foundation.Collections;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class MainPage : Page
    {
        #region Delegates + Fields + Events + Enums

        private readonly Random testRandom = new Random();

        #endregion Delegates + Fields + Events + Enums

        #region Methods

        private void InitTest()
        {
        }
        private async Task LoseHalfYourCards()
        {
            int loss = (int)CurrentPlayer.GameData.Resources.CurrentResources.Count / 2;
            CurrentPlayer = TheHuman;
            if (loss < 4)
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
            }


            loss = ( int )CurrentPlayer.GameData.Resources.CurrentResources.Count / 2;
            if (loss >= 4)
            {
                ResourceCardCollection rc = new ResourceCardCollection(false);
                rc.AddResources(CurrentPlayer.GameData.Resources.CurrentResources);
                TakeCardDlg dlg = new TakeCardDlg()
                {
                    To = MainPageModel.Bank,
                    From = CurrentPlayer,
                    SourceOrientation = TileOrientation.FaceUp,
                    CountVisible = true,
                    HowMany = loss,
                    Source = rc,
                    Destination = new ResourceCardCollection(false),
                    Instructions = $"Give {loss} cards to the bank.",
                    ConsolidateCards = false,
                };
                var ret = await dlg.ShowAsync();
                if (ret == ContentDialogResult.Primary)
                {
                    CurrentPlayer.GameData.Resources.GrantResources(ResourceCardCollection.ToTradeResources(dlg.Destination).GetNegated(), false);
                }
            }
        }

        private async void Menu_OnResetService(object sender, RoutedEventArgs e)
        {

            await CreateAndConfigureProxy();



        }
        private async void OnGrantEntitlements(object sender, RoutedEventArgs e)
        {
            await TestGrantEntitlementMessage();
        }

        private async void OnGrantResources(object sender, RoutedEventArgs e)
        {
            TradeResources tr = new TradeResources()
            {
                Sheep = 1,
                Wheat = 1,
                Ore = 1,
                Brick = 1,
                Wood = 1
            };

            await TestGrantEntitlements.Post(this, tr, new List<Entitlement>(), new List<DevCardType>());
        }

        private async void OnGrantDevCard(object sender, RoutedEventArgs e)
        {
            string tag = ((FrameworkElement) sender).Tag as string;
            var dc = new List<DevCardType>();
            if (tag == "All")
            {
                dc.Add(DevCardType.Knight);
                dc.Add(DevCardType.YearOfPlenty);
                dc.Add(DevCardType.Monopoly);
                dc.Add(DevCardType.RoadBuilding);
                dc.Add(DevCardType.VictoryPoint);
            }
            else
            {
                DevCardType devCardType = Enum.Parse<DevCardType>(tag);
                dc.Add(devCardType);
            }

            await TestGrantEntitlements.Post(this, new TradeResources(), new List<Entitlement>(), dc);
        }

        // int toggle = 0;
        private void OnTest1(object sdr, RoutedEventArgs rea)
        {
            MainPageModel.GameInfo.Pirates = true;
            CTRL_InvationCounter.Next();
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
        private async Task DoReplay(List<CatanMessage> messages)
        {
            if (messages == null) return;
            if (messages.Count == 0) return;
            await EndGame();
            GameInfo gameInfo = null;
            for (int i = 0; i < messages.Count; i++)
            {

                if (messages[i].MessageType == MessageType.Ack) continue;

                CatanMessage parsedMessage = null;
                try
                {
                    parsedMessage = CatanSignalRClient.ParseMessage(messages[i]);
                }
                catch (Exception e)
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
                            Debug.Assert(( ( AckModel )ack.Data ).AckedMessageId == parsedMessage.MessageId);
                            if (parsedMessage.ActionType == ActionType.Normal)
                            {
                                parsedMessage.ActionType = ActionType.Replay;
                            }
                            MainPageModel.ChangeUnprocessMessage(1);
                            await ProcessMessage(parsedMessage);
                        }
                        break;
                    case MessageType.CreateGame:
                        gameInfo = parsedMessage.GameInfo;
                        break;
                    case MessageType.DeleteGame:
                        break;
                    case MessageType.JoinGame:
                        await JoinOrCreateGame(parsedMessage.GameInfo); // this is local only
                        break;
                    case MessageType.LeaveGame:
                        break;
                    case MessageType.Ack:
                        break;
                    default:
                        break;
                }

            }

            //
            //  now our state matches what is in the log...see if the game we are supposed to be in is running
            if (gameInfo != null)
            {
                await CreateAndConfigureProxy();
                await MainPageModel.CatanService.CreateGame(gameInfo);
                await MainPageModel.CatanService.JoinGame(gameInfo, TheHuman.PlayerName);
                this.TraceMessage($"Joined game {gameInfo}");
            }

        }

        private async void OnTest2(object sdr, RoutedEventArgs rea)
        {
            IReadOnlyList<User> users = await User.FindAllAsync();

            foreach (User user in users)
            {
                String[] desiredProperties = new String[]
                    {
                        KnownUserProperties.FirstName,
                        KnownUserProperties.LastName,
                        KnownUserProperties.ProviderName,
                        KnownUserProperties.AccountName,
                        KnownUserProperties.GuestHost,
                        KnownUserProperties.PrincipalName,
                        KnownUserProperties.DomainName,
                        KnownUserProperties.SessionInitiationProtocolUri,
                    };
                // Issue a bulk query for all of the properties.
                IPropertySet values = await user.GetPropertiesAsync(desiredProperties);
                string result = "";
                foreach (String property in desiredProperties)
                {
                    result += property + ": " + values[property] + "\n";
                }
                this.TraceMessage(result);

                TheHuman = NameToPlayer(( string )values[KnownUserProperties.FirstName]);
                if (TheHuman != null)
                {
                    IRandomAccessStreamReference streamReference = await user.GetPictureAsync(UserPictureSize.Size64x64);
                    if (streamReference != null)
                    {
                        IRandomAccessStream stream = await streamReference.OpenReadAsync();
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(stream);
                        ImageBrush brush = new ImageBrush
                        {
                            AlignmentX = AlignmentX.Left,
                            AlignmentY = AlignmentY.Top,
                            Stretch = Stretch.UniformToFill,
                            ImageSource = bitmapImage
                        };

                        TheHuman.ImageBrush = brush;


                    }
                }

            }
        }

        // Undo
        private async void OnTest3(object sdr, RoutedEventArgs rea)
        {
            await TestGrantEntitlementMessage();
        }

        private async void OnTestExpansionGame(object sender, RoutedEventArgs e)
        {
            AnimationSpeedBase = 10; // speed up the animations
            RandomGoldTileCount = 3;
            await this.Reset();
            _gameView.Reset();


            _gameView.CurrentGame = _gameView.Games[1];
            MainPageModel.PlayingPlayers.Clear();
            GameInfo info = new GameInfo()
            {
                Creator = TheHuman.PlayerName,
                GameIndex = 1,
                Id = Guid.NewGuid(),
                Started = false
            };
            await NewGameLog.JoinOrCreateGame(this, info, CatanAction.GameCreated);

            MainPageModel.PlayingPlayers.Clear();

            for (int i = 0; i < 5; i++)
            {
                await AddPlayerLog.AddPlayer(this, MainPageModel.AllPlayers[i].PlayerName);
            };

            await NextState(); // Order Done
            await NextState(); // Board Accepted
            await NextState(); // Start Game
            await NextState(); // Start Pick Resources

            while (Log.GameState != GameState.DoneResourceAllocation)
            {
                await AutoSetBuildingAndRoad();
                await NextState();
            }

            await NextState();
        }

        private async void OnTestRegularGame(object sender, RoutedEventArgs e)
        {
            AnimationSpeedBase = 10; // speed up the animations
            RandomGoldTileCount = 1;
            await this.Reset();
            _gameView.Reset();


            _gameView.CurrentGame = _gameView.Games[1];
            MainPageModel.PlayingPlayers.Clear();
            GameInfo info = new GameInfo()
            {
                Creator = TheHuman.PlayerName,
                GameIndex = 0,
                Id = Guid.NewGuid(),
                Started = false,
                Pirates=true
            };
            await NewGameLog.JoinOrCreateGame(this, info, CatanAction.GameCreated);

            MainPageModel.PlayingPlayers.Clear();

            for (int i = 0; i < 3; i++)
            {
                await AddPlayerLog.AddPlayer(this, MainPageModel.AllPlayers[i].PlayerName);
                await Task.Delay(10);
            };

            await NextState(); // Order Done
            await NextState(); // Board Accepted
            await NextState(); // Start Game
            await NextState(); // Start Pick Resources
            await Task.Delay(10);

            while (Log.GameState != GameState.DoneResourceAllocation)
            {
                await Task.Delay(10);
                await AutoSetBuildingAndRoad();
                await NextState();
            }

          //  await NextState();
        }

        private async void OnTestService(object sender, RoutedEventArgs e)
        {
            await CreateAndConfigureProxy();

            Guid id = Guid.Parse("{A2D8D755-9015-41F3-9CF3-560B2BE758EF}");
            string gameName = "Test_Game_{A2D8D755-9015-41F3-9CF3-560B2BE758EF}";
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            List<GameInfo> games = await MainPageModel.CatanService.GetAllGames();
            MainPageModel.CatanService.OnGameDeleted += CatanService_OnGameDeleted;

            void CatanService_OnGameDeleted(GameInfo gameInfo, string by)
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
            void CatanService_OnGameCreated(GameInfo gameInfo, string playerName)
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
            void CatanService_OnGameJoined(GameInfo gameInfo, string playerName)
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
            CurrentPlayer.GameData.Resources.TotalResourcesForGame = new TradeResources() { Ore = 3, Wheat = 2, Wood = 5, Brick = 10 };
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
                CurrentPlayer.GameData.Resources.GrantResources(ResourceCardCollection.ToTradeResources(dlg.Destination).GetNegated(), false);
            }

            this.TraceMessage($"ret= {ret} Cards={ResourceCardCollection.ToTradeResources(dlg.Destination)}");
        }

        private void TestTrade()
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

        private void TestTrades2()
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

        #endregion Methods
    }


}
