using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Catan10.CatanService;
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
                Wood = 1,
                Brick = 2,

                Wheat = 3,
                Sheep = 4,
                Ore = 5,

                Paper = 6,
                Cloth = 7,
                Coin = 8,

                VictoryPoint= 7,
                Trade=6,
                Politics=5,

                Science = 4,
                AnyDevCard = 3
            };

            await TestGrantEntitlements.Post(this, tr, new List<Entitlement>(), new List<DevCardType>());

            MainPage.Current.CurrentPlayer.GameData.PoliticsRank = 3;
            MainPage.Current.CurrentPlayer.GameData.TradeRank = 2;
            MainPage.Current.CurrentPlayer.GameData.ScienceRank = 1;
            MainPage.Current.CurrentPlayer.GameData.VictoryPoints = 2;
            CurrentPlayer.GameData.Resources.ResourcesLostToBaron.Ore = 5;
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
            GameContainer.CurrentGame.HexPanel.CitiesAndKnights = true;
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
                        await CreateGame(parsedMessage.GameInfo); // this is local only
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

        private void DumpLogRecords(object sdr, RoutedEventArgs rea)
        {
            Log.DumpActionStack();
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
            await NewGameLog.CreateGame(this, info, CatanAction.GameCreated);

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
        /// <summary>
        ///     This starts in WaitingForNext or it will create a new game.
        ///     it then
        ///     
        ///     1. places a Knight, Upgrades, then Activates it
        ///     2. Moves to next player
        ///     3. that player rolls
        ///     4. then buys the Deserter Entitlement
        ///     5. picks the Knight that was built by the previous player
        ///     6. picks a spot to place it.
        ///     7. Undo it all
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnTestDeserter(object sender, RoutedEventArgs e)
        {
            await TestDeserter();

        }
        private async void OnTestMoveKnight(object sender, RoutedEventArgs e)
        {
            await TestMoveKnight();

        }

        private async void OnTestMetro(object sender, RoutedEventArgs e)
        {
            await TestMetro();
        }
        private async void OnUndoToWaitingForNext(object sender, RoutedEventArgs e)
        {
            await UndoToState(GameState.WaitingForNext, 0);

        }

        //
        //  find a place the Current User can place a Building
        private BuildingCtrl Test_FindBuildingPlacement(BuildingState buildingState)
        {
            foreach (BuildingCtrl building in _gameView.AllBuildings)
            {
                if (ValidateBuildingLocation(building) == buildingState)
                {
                    return building;
                }
            }
            return null;
        }
        private async Task Test_DoRoll(int redRoll, int whiteRoll, SpecialDice special)
        {
            var rollModel = new RollModel()
            {
                RedDie = redRoll,
                WhiteDie = whiteRoll,
                Roll = redRoll + whiteRoll,
                SpecialDice = special
            };
            await OnRolledNumber(rollModel);
        }
        private async Task StartGame(GameInfo info)
        {
            AnimationSpeedBase = 10; // speed up the animations
            RandomGoldTileCount = 1;
            await this.Reset();
            _gameView.Reset();

            _gameView.CurrentGame = _gameView.Games[1];
            MainPageModel.PlayingPlayers.Clear();
            await NewGameLog.CreateGame(this, info, CatanAction.GameCreated);
            UpdateGridLocations();
            await Task.Delay(10);

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

            PlayingPlayers[0].GameData.TradeRank = 1;
            PlayingPlayers[1].GameData.ScienceRank = 2;
            PlayingPlayers[2].GameData.PoliticsRank = 3;

            await NextState();
            await Test_DoRoll(1, 2, SpecialDice.Pirate);

        }

        private async void OnTestRegularGame(object sender, RoutedEventArgs e)
        {


            GameInfo info = new GameInfo()
            {
                Creator = TheHuman.PlayerName,
                GameIndex = 0,
                Id = Guid.NewGuid(),
                Started = false,
                CitiesAndKnights=true
            };

            await StartGame(info);

        }

        private async void OnTestRollSeven(object sender, RoutedEventArgs e)
        {
            RollModel rm = new RollModel()
            {
                RedDie = 1,
                WhiteDie = 6,
                SpecialDice = SpecialDice.Pirate
            };
            for (int i = 0; i < 6; i++)
            {

                var roll = rm.Copy();
                await this.OnRolledNumber(roll);
                await NextState();
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

        private async Task TestMetro()
        {
            if (CurrentGameState != GameState.WaitingForNext)
            {
                GameInfo info = new GameInfo()
                {
                    Creator = TheHuman.PlayerName,
                    GameIndex = 0,
                    Id = Guid.NewGuid(),
                    Started = false,
                    CitiesAndKnights=true
                };

                await StartGame(info);
            }

            if (CurrentGameState != GameState.WaitingForNext)
            {
                this.TraceMessage($"can't continue with GameState.{CurrentGameState}");
            }
            while (CurrentPlayer.GameData.PoliticsRank < 5)
            {
                await ImprovementLog.PostLog(this, Entitlement.PoliticsUpgrade, CurrentPlayer.GameData.PoliticsRank);
            }
      
            Debug.Assert(CurrentGameState == GameState.UpgradeToMetro);

            await MetroTransitionLog.UpgradeCityLog(this, this.CurrentPlayer.GameData.Cities[0].Index);
            Debug.Assert(CurrentPlayer.GameData.Score == 5);


            // move to Player
            await NextState();

            // roll
            await Test_DoRoll(1, 2, SpecialDice.Pirate);
            while (CurrentPlayer.GameData.PoliticsRank < 5)
            {
                await ImprovementLog.PostLog(this, Entitlement.PoliticsUpgrade, CurrentPlayer.GameData.PoliticsRank);
                await Task.Delay(0); // force UI to update

            }
            await Task.Delay(0); // force UI to update
            Debug.Assert(CurrentGameState == GameState.WaitingForNext); // Tie does not get the win!
                                                                        // move to Player
            await NextState();
            await Task.Delay(0); // force UI to update
            // roll
            await Test_DoRoll(1, 2, SpecialDice.Pirate);
            while (CurrentPlayer.GameData.PoliticsRank < 6)
            {
                await ImprovementLog.PostLog(this, Entitlement.PoliticsUpgrade, CurrentPlayer.GameData.PoliticsRank);
                await Task.Delay(0); // force UI to update
            }

            Debug.Assert(CurrentPlayer.GameData.PoliticsRank == 6);

            Debug.Assert(CurrentGameState == GameState.UpgradeToMetro);
            await Task.Delay(0); // force UI to update
            await MetroTransitionLog.UpgradeCityLog(this, this.CurrentPlayer.GameData.Cities[0].Index);

            Debug.Assert(CurrentPlayer.GameData.Score == 5);

            Debug.Assert(PlayingPlayers[0].GameData.Score == 3);

            Debug.Assert(PlayingPlayers[0].GameData.Cities[0].City.Metropolis = false);

        }
        private async Task TestMoveKnight()
        {
            if (CurrentGameState != GameState.WaitingForNext)
            {
                GameInfo info = new GameInfo()
                {
                    Creator = TheHuman.PlayerName,
                    GameIndex = 0,
                    Id = Guid.NewGuid(),
                    Started = false,
                    CitiesAndKnights=true
                };

                await StartGame(info);
            }

            if (CurrentGameState != GameState.WaitingForNext)
            {
                this.TraceMessage($"can't continue with GameState.{CurrentGameState}");
            }

            //
            //  get some knight entitlements
            await PurchaseEntitlement(CurrentPlayer, Entitlement.BuyOrUpgradeKnight, CurrentGameState);
            await PurchaseEntitlement(CurrentPlayer, Entitlement.BuyOrUpgradeKnight, CurrentGameState);
            await PurchaseEntitlement(CurrentPlayer, Entitlement.ActivateKnight, CurrentGameState);

            //
            //  figure out where to build it

            var originalKnightBuilding = Test_FindBuildingPlacement(BuildingState.Knight);
            await KnightLeftPointerPressed(originalKnightBuilding); // builds it
            await KnightLeftPointerPressed(originalKnightBuilding); // activate it
            await KnightLeftPointerPressed(originalKnightBuilding); // upgrade it

            Debug.Assert(originalKnightBuilding.IsKnight);
            Debug.Assert(originalKnightBuilding.Knight.KnightRank == KnightRank.Strong);
            Debug.Assert(originalKnightBuilding.Knight.Activated == true);
            Debug.Assert(CurrentPlayer.GameData.Resources.UnspentEntitlements.Count == 0);
            Debug.Assert(PlayingPlayers[0].GameData.Knights.Count == 1);
            Debug.Assert(MainPageModel.TotalKnightRanks == 2);

            // guy a couple of roads
            await PurchaseEntitlement(CurrentPlayer, Entitlement.Road, CurrentGameState);
            await PurchaseEntitlement(CurrentPlayer, Entitlement.Road, CurrentGameState);

            foreach (var road in originalKnightBuilding.AdjacentRoads)
            {
                if (road.Owner == null)
                {
                    await UpdateRoadLog.PostLogEntry(this, road, NextRoadState(road), RaceTracking);
                }
            }

            await PurchaseEntitlement(CurrentPlayer, Entitlement.MoveKnight, CurrentGameState);



        }

        private async Task TestDeserter()
        {
            if (CurrentGameState != GameState.WaitingForNext)
            {
                GameInfo info = new GameInfo()
                {
                    Creator = TheHuman.PlayerName,
                    GameIndex = 0,
                    Id = Guid.NewGuid(),
                    Started = false,
                    CitiesAndKnights=true
                };

                await StartGame(info);
            }

            if (CurrentGameState != GameState.WaitingForNext)
            {
                this.TraceMessage($"can't continue with GameState.{CurrentGameState}");
            }


            //
            //  get some knight entitlements
            await PurchaseEntitlement(CurrentPlayer, Entitlement.BuyOrUpgradeKnight, CurrentGameState);
            await PurchaseEntitlement(CurrentPlayer, Entitlement.BuyOrUpgradeKnight, CurrentGameState);
            await PurchaseEntitlement(CurrentPlayer, Entitlement.ActivateKnight, CurrentGameState);

            //
            //  figure out where to build it

            var originalKnightBuilding = Test_FindBuildingPlacement(BuildingState.Knight);
            await KnightLeftPointerPressed(originalKnightBuilding); // builds it
            await KnightLeftPointerPressed(originalKnightBuilding); // activate it
            await KnightLeftPointerPressed(originalKnightBuilding); // upgrade it

            Debug.Assert(originalKnightBuilding.IsKnight);
            Debug.Assert(originalKnightBuilding.Knight.KnightRank == KnightRank.Strong);
            Debug.Assert(originalKnightBuilding.Knight.Activated == true);
            Debug.Assert(CurrentPlayer.GameData.Resources.UnspentEntitlements.Count == 0);
            Debug.Assert(PlayingPlayers[0].GameData.Knights.Count == 1);
            Debug.Assert(MainPageModel.TotalKnightRanks == 2);

            // move to Player
            await NextState();

            // roll
            await Test_DoRoll(1, 2, SpecialDice.Pirate);

            // get the Deserter Entitlement
            await PurchaseEntitlement(CurrentPlayer, Entitlement.Deserter, GameState.PickDeserter);

            Debug.Assert(CurrentGameState == GameState.PickDeserter);

            await KnightLeftPointerPressed(originalKnightBuilding); // this is the knight we just built
            Debug.Assert(originalKnightBuilding.BuildingState == BuildingState.None);

            var newKnight = Test_FindBuildingPlacement(BuildingState.Knight);

            Debug.Assert(newKnight.BuildingState == BuildingState.None);
            await KnightLeftPointerPressed(newKnight);

            Debug.Assert(newKnight.IsKnight);
            Debug.Assert(newKnight.Knight.KnightRank == KnightRank.Strong);
            Debug.Assert(newKnight.Knight.Activated == false);
            Debug.Assert(CurrentPlayer.GameData.Resources.UnspentEntitlements.Count == 0);
            Debug.Assert(PlayingPlayers[0].GameData.Knights.Count == 0);
            Debug.Assert(MainPageModel.TotalKnightRanks == 0); // not activated yet
            Debug.Assert(CurrentGameState == GameState.WaitingForNext);

            await PurchaseEntitlement(CurrentPlayer, Entitlement.ActivateKnight, CurrentGameState);
            await KnightLeftPointerPressed(newKnight); // Activate it
            Debug.Assert(MainPageModel.TotalKnightRanks == 2);
            //  await UndoToState(GameState.WaitingForNext);
            //  await UndoKnightWithAsserts(originalKnightBuilding);

        }
        private async Task UndoKnightWithAsserts(BuildingCtrl originalKnightBuilding)
        {
            await Task.Delay(10); // in case you want to look at it...
            await DoUndo(); // Activate
            await Task.Delay(10);
            await DoUndo(); // Buy activate entitlement
            await Task.Delay(100);
            await DoUndo(); // undoes the DoneWithDeserter and the placement
            await Task.Delay(100);
            Debug.Assert(CurrentGameState == GameState.PlaceDeserterKnight);

            Debug.Assert(MainPageModel.TotalKnightRanks == 0);

            await DoUndo(); // undoes the Pick
            await Task.Delay(100);
            Debug.Assert(CurrentGameState == GameState.PickDeserter);
            Debug.Assert(MainPageModel.TotalKnightRanks == 2);
            Debug.Assert(originalKnightBuilding.IsKnight);
            Debug.Assert(originalKnightBuilding.Knight.KnightRank == KnightRank.Strong);
            Debug.Assert(originalKnightBuilding.Knight.Activated);

            Debug.Assert(CurrentPlayer.GameData.Resources.UnspentEntitlements.Count == 1);
            Debug.Assert(CurrentPlayer.GameData.Resources.UnspentEntitlements[0] == Entitlement.Deserter);
            await DoUndo(); // undoes the buy entitlement
            Debug.Assert(CurrentPlayer.GameData.Resources.UnspentEntitlements.Count == 0);
            Debug.Assert(CurrentGameState == GameState.WaitingForNext);

            await DoUndo(); // back to roll
            await Task.Delay(10);
            await DoUndo(); // back to player[0]
            await Task.Delay(10);
            await DoUndo(); // Activate knight
            await Task.Delay(10);
            await DoUndo(); // Upgrade Knight
            await Task.Delay(10);
            await DoUndo(); // Place Knight

            await Task.Delay(10);
            await DoUndo(); // Buy entitlement

            await Task.Delay(10);
            await DoUndo(); //  Buy entitlement

            await Task.Delay(10);
            await DoUndo(); //  Buy entitlement
        }

        private async Task UndoToState(GameState state, int millisecsDelay = 0)
        {

            do
            {
                this.TraceMessage($"Undoing: {this.Log.PeekAction}");
                await DoUndo();
                await Task.Delay(millisecsDelay);
            } while (CurrentGameState != state);


        }
        #endregion Methods
    }

}
