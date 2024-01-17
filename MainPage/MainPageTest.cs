using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Catan10.CatanService;
using System.Diagnostics;
using Windows.UI.WindowManagement;
using System.Linq;
using Windows.ApplicationModel.Activation;

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
        ///     1. Create the "game" ... this is the communication channel the game uses to send messages to all the playerIndex
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
            await StartExpansionTestGame(true, MainPageModel.TestingCitiesAndKnights, 5);

        }

        public async Task StartExpansionTestGame(bool assignResources, bool useCitiesAndKnights, int playerCount)
        {
            this.MainPageModel.Testing = true;
            RandomGoldTileCount = 3;
            await this.Reset();
            CTRL_GameView.Reset();

            CTRL_GameView.CurrentGame = CTRL_GameView.Games[1];
            MainPageModel.PlayingPlayers.Clear();
            GameInfo info = new GameInfo()
            {
                Creator = TheHuman.PlayerName,
                GameIndex = 1,
                Id = Guid.NewGuid(),
                Started = false,
                CitiesAndKnights = useCitiesAndKnights
            };
            await NewGameLog.CreateGame(this, info, CatanAction.GameCreated);

            MainPageModel.PlayingPlayers.Clear();

            for (int i = 0; i < playerCount; i++)
            {
                await AddPlayerLog.AddPlayer(this, MainPageModel.AllPlayers[i].PlayerName);
            };

            await NextState(); // Order Done
            await NextState(); // Board Accepted
            await NextState(); // Start Game
            await NextState(); // Start Pick Resources

            if (assignResources)
            {
                while (Log.GameState != GameState.DoneResourceAllocation)
                {
                    await AutoSetBuildingAndRoad();
                    await NextState();
                }

                await NextState();
            }
        }

        private async Task TestLongestRoad()
        {
            LongestRoadTest test = new LongestRoadTest(this);
            await test.TestLongestRoad();
        }

        public async Task Test_MoveToPlayer(int playerIndex, GameState desiredState)
        {
            int count = 0;
            PlayerModel desiredPlayer = PlayingPlayers[playerIndex];
            while (CurrentPlayer != desiredPlayer || CurrentGameState != desiredState)
            {

                if (CurrentGameState == GameState.WaitingForRoll)
                {
                    await Test_DoRoll(3, 5, SpecialDice.Science);
                    Debug.Assert(count < PlayingPlayers.Count); // we should never have to roll twice...
                    count++;

                }
                else
                {
                    await NextState();
                }
            }
            //  this.TraceMessage($"Rolled {count} times");
        }

        public async Task PurchaseAndPlaceKnight(int knightIndex, bool activate, KnightRank rank)
        {
            var knight = GetBuilding(knightIndex);
            await PurchaseEntitlement(Entitlement.BuyOrUpgradeKnight);
            await KnightLeftPointerPressed(knight); // build it
            if (activate)
            {
                await PurchaseEntitlement(Entitlement.ActivateKnight);
                await KnightLeftPointerPressed(knight); // activate it
            }

            while (knight.Knight.KnightRank < rank)
            {
                await PurchaseEntitlement(Entitlement.BuyOrUpgradeKnight);
                await KnightLeftPointerPressed(knight);  // upgrade
            }
        }

        public async Task PurchaseAndPlaceRoad(int roadIndex)
        {
            var road = CTRL_GameView.GetRoad(roadIndex);
            await PurchaseEntitlement(Entitlement.Road);
            await UpdateRoadLog.PostLogEntry(this, road, RoadState.Road, RaceTracking);
        }
        /// <summary>
        ///     build a building. can start with None and build a City directly buy buying both entitlements
        /// </summary>
        /// <param name="buildingIndex"></param>
        /// <param name="entitlement"></param>
        /// <returns></returns>
        public async Task PurchaseAndPlaceBuilding(int buildingIndex, Entitlement entitlement)
        {
            var building = CTRL_GameView.GetBuilding(buildingIndex);
            if (building.BuildingState == BuildingState.None && entitlement == Entitlement.Settlement)
            {
                await PurchaseEntitlement(Entitlement.Settlement);
                await UpdateBuildingLog.UpdateBuildingState(this, building, BuildingState.Settlement, CurrentGameState);
                return;
            }

            if (building.BuildingState == BuildingState.None && entitlement == Entitlement.City)
            {
                await PurchaseAndPlaceBuilding(buildingIndex, Entitlement.Settlement);
            }

            if (building.BuildingState == BuildingState.Settlement && entitlement == Entitlement.City)
            {
                await UpdateBuildingLog.UpdateBuildingState(this, building, BuildingState.City, CurrentGameState);
            }
            else
            {
                Debug.Assert(false, "Bad entitlement or building");
            }
        }

        /// <summary>
        ///     This starts in WaitingForNext or it will create a new game.
        ///     it then
        ///     
        ///     1. places a Knight, Upgrades, then Activates it
        ///     2. Moves to next desiredPlayer
        ///     3. that desiredPlayer rolls
        ///     4. then buys the Deserter Entitlement
        ///     5. picks the Knight that was built by the previous desiredPlayer
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
        private async void OnUndoToPreviousPlayer(object sender, RoutedEventArgs e)
        {
            int index = PlayingPlayers.IndexOf(CurrentPlayer);
            index--;
            index = ( index + PlayingPlayers.Count ) % PlayingPlayers.Count;
            await UndoToPlayer(PlayingPlayers[index]);
        }

        //
        //  find a place the Current User can place a Building
        private BuildingCtrl Test_FindBuildingPlacement(BuildingState buildingState)
        {
            foreach (BuildingCtrl building in CTRL_GameView.AllBuildings)
            {
                if (ValidateBuildingLocation(building) == buildingState)
                {
                    return building;
                }
            }
            return null;
        }
        public async Task Test_DoRoll(int redRoll, int whiteRoll, SpecialDice special)
        {
            Debug.Assert(redRoll > 0 && redRoll < 7);
            Debug.Assert(whiteRoll > 0 && whiteRoll < 7);
            var rollModel = new RollModel()
            {
                RedDie = redRoll,
                WhiteDie = whiteRoll,
                Roll = redRoll + whiteRoll,
                SpecialDice = special
            };
            // this.TraceMessage($"{CurrentPlayer.PlayerName} rolls {rollModel.Roll} Stack Entry: {Log.ActionCount}");
            await OnRolledNumber(rollModel);
        }
        public async Task StartTestGame(GameInfo info, bool autoSetResources)
        {
            AnimationSpeedBase = 10; // speed up the animations
            MainPageModel.Testing = true;
            RandomGoldTileCount = 1;
            await this.Reset();
            CTRL_GameView.Reset();

            CTRL_GameView.CurrentGame = CTRL_GameView.Games[1];
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
            if (!autoSetResources) return;

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
            TileCtrl tile = GameContainer.CurrentGame.HexPanel.BaronTile;
            GameContainer.CurrentGame.HexPanel.BaronTile = GameContainer.TilesInIndexOrder[0];
            GameContainer.CurrentGame.HexPanel.BaronTile = tile;


        }

        private async void OnTestRegularGame(object sender, RoutedEventArgs e)
        {


            GameInfo info = new GameInfo()
            {
                Creator = TheHuman.PlayerName,
                GameIndex = 0,
                Id = Guid.NewGuid(),
                Started = false,
                CitiesAndKnights=MainPageModel.TestingCitiesAndKnights
            };

            await StartTestGame(info, true);
            await Test_DoRoll(1, 2, SpecialDice.Pirate);

        }
        private async void OnTestBaron(object sender, RoutedEventArgs e)
        {
            await TestBaron();
        }
        private async Task TestBaron()
        {
            try
            {

                MainPageModel.Testing = true;
                MainPageModel.TestingCitiesAndKnights = false;
                if (CurrentGameState != GameState.WaitingForNext)
                {
                    GameInfo info = new GameInfo()
                    {
                        Creator = TheHuman.PlayerName,
                        GameIndex = 0,
                        Id = Guid.NewGuid(),
                        Started = false,
                        CitiesAndKnights=false
                    };

                    await StartTestGame(info, true);
                }
                var desertTile = CTRL_GameView.CurrentGame.HexPanel.DesertTiles[0];
                Debug.Assert(desertTile != null);
                await TestCheckpointLog.AddTestCheckpoint(this);
                //
                //  scenario 1: roll a 7
                await RollSevenAndMoveBaron(PlayingPlayers[1]);
                Debug.Assert(CurrentGameState == GameState.WaitingForNext);
                Debug.Assert(PlayingPlayers[1].GameData.TimesTargeted == 1);
                await RollbackToCheckpoint();

                Debug.Assert(PlayingPlayers[1].GameData.TimesTargeted == 0);

                await TestCheckpointLog.AddTestCheckpoint(this);
                GameContainer.CurrentGame.HexPanel.BaronnAnimationSkipToEnd();
                Debug.Assert(this.CurrentGameState == GameState.WaitingForRoll);
                Debug.Assert(CurrentPlayer.GameData.Resources.ThisTurnsDevCard.DevCardType == DevCardType.None);
                Debug.Assert(CTRL_GameView.CurrentGame.HexPanel.BaronTile == desertTile);

                for (int i = 0; i < PlayingPlayers.Count * 2; i++)
                {
                    var targetPlayer = PlayingPlayers.ElementAt(i % PlayingPlayers.Count);
                    await BuyAndPlayKnightDevCard(targetPlayer);
                    Debug.Assert(CurrentGameState == GameState.WaitingForRoll);
                    await Test_DoRoll(6, 6, SpecialDice.None); // just rolling a 12
                    Debug.Assert(CurrentGameState == GameState.WaitingForNext);
                    await NextState();
                    Debug.Assert(CurrentGameState == GameState.WaitingForRoll);
                }
                Debug.Assert(PlayingPlayers[0].GameData.Resources.KnightsPlayed == 2);
                Debug.Assert(PlayingPlayers[1].GameData.Resources.KnightsPlayed == 2);
                Debug.Assert(PlayingPlayers[1].GameData.Resources.KnightsPlayed == 2);
                Debug.Assert(PlayingPlayers[0].GameData.TimesTargeted == 2);
                Debug.Assert(PlayingPlayers[1].GameData.TimesTargeted == 2);
                Debug.Assert(PlayingPlayers[2].GameData.TimesTargeted == 2);
                Debug.Assert(!PlayingPlayers[0].GameData.LargestArmy);
                Debug.Assert(!PlayingPlayers[1].GameData.LargestArmy);
                Debug.Assert(!PlayingPlayers[2].GameData.LargestArmy);

                await BuyAndPlayKnightDevCard(PlayingPlayers[1]);
                Debug.Assert(PlayingPlayers[0].GameData.Resources.KnightsPlayed == 3);
                Debug.Assert(PlayingPlayers[1].GameData.TimesTargeted == 3);
                Debug.Assert(PlayingPlayers[0].GameData.LargestArmy);
                await Test_DoRoll(6, 6, SpecialDice.None); // just rolling a 12
                await NextState();

                //
                //  desiredPlayer 2 ties desiredPlayer 1 for most knights, but doesn't get largest army
                await BuyAndPlayKnightDevCard(PlayingPlayers[2]);
                Debug.Assert(PlayingPlayers[0].GameData.Resources.KnightsPlayed == 3);
                Debug.Assert(PlayingPlayers[2].GameData.TimesTargeted == 3);
                Debug.Assert(PlayingPlayers[0].GameData.LargestArmy == true);
                Debug.Assert(PlayingPlayers[1].GameData.LargestArmy == false);


                Debug.Assert(CurrentPlayer == PlayingPlayers[1]);
                // I want to get around to the same desiredPlayer so they can buy a knight again -- 
                // so just roll/nextstate

                for (int i = 0; i < PlayingPlayers.Count; i++)
                {
                    await Test_DoRoll(i + 1, i + 3, SpecialDice.None); // just rolling a 12
                    await NextState();
                }
                Debug.Assert(CurrentPlayer == PlayingPlayers[1]);
                //
                //  desiredPlayer 2 passes desiredPlayer 1 for most knights, gets largest army
                await BuyAndPlayKnightDevCard(PlayingPlayers[2]);
                Debug.Assert(PlayingPlayers[0].GameData.Resources.KnightsPlayed == 3);
                Debug.Assert(PlayingPlayers[1].GameData.Resources.KnightsPlayed == 4);
                Debug.Assert(PlayingPlayers[2].GameData.TimesTargeted == 4);
                Debug.Assert(!PlayingPlayers[0].GameData.LargestArmy);
                Debug.Assert(PlayingPlayers[1].GameData.LargestArmy);

                // desiredPlayer 2 changes their mind...
                await DoUndo();
                Debug.Assert(PlayingPlayers[0].GameData.Resources.KnightsPlayed == 3);
                Debug.Assert(PlayingPlayers[1].GameData.Resources.KnightsPlayed == 3);
                Debug.Assert(PlayingPlayers[2].GameData.TimesTargeted == 3);
                Debug.Assert(PlayingPlayers[0].GameData.LargestArmy);
                Debug.Assert(!PlayingPlayers[1].GameData.LargestArmy);
                await RollbackToCheckpoint();

                foreach (var player in PlayingPlayers)
                {
                    Debug.Assert(player.GameData.LargestArmy == false);
                    Debug.Assert(player.GameData.Resources.KnightsPlayed == 0);
                    Debug.Assert(player.GameData.TimesTargeted == 0);
                }

                this.TraceMessage($"ended on {CTRL_GameView.CurrentGame.HexPanel.BaronTile}");


            }
            finally
            { MainPageModel.Testing = false; }
        }

        private async Task BuyAndPlayKnightDevCard(PlayerModel target)
        {
            var currentTile = CTRL_GameView.CurrentGame.HexPanel.BaronTile;
            int knightsPlayed = CurrentPlayer.GameData.Resources.KnightsPlayed;
            int timesTargetted = target.GameData.TimesTargeted;
            TileCtrl targetTile = null;
            foreach (var kvp in target.GameData.Settlements[0].BuildingToTileDictionary)
            {
                if (kvp.Value != currentTile)
                {
                    targetTile = kvp.Value;
                    break;
                }
            }
            Debug.Assert(targetTile != null);
            Debug.Assert(CurrentPlayer.GameData.Resources.UnspentEntitlements.Count == 0);
            await PurchaseEntitlement(Entitlement.MoveBaron);
            Debug.Assert(CurrentPlayer.GameData.Resources.UnspentEntitlements.Contains(Entitlement.MoveBaron));
            Debug.Assert(CurrentPlayer.GameData.Resources.UnspentEntitlements.Count == 1);
            Debug.Assert(CurrentPlayer.GameData.Resources.KnightsPlayed == knightsPlayed);
            var victims = new List<string>()  {target.PlayerName};

            int startingBaronIndex = CTRL_GameView.BaronTile.Index;
            await MovedBaronLog.PostLog(this,
                                          victims,
                                          targetTile.Index,
                                          startingBaronIndex,
                                          TargetWeapon.Baron,
                                           MoveBaronReason.PlayedDevCard,
                                            ResourceType.None);
            GameContainer.CurrentGame.HexPanel.BaronnAnimationSkipToEnd();
            Debug.Assert(CurrentGameState == GameState.WaitingForRoll);
            Debug.Assert(CurrentPlayer.GameData.Resources.KnightsPlayed == knightsPlayed + 1);
            Debug.Assert(CurrentPlayer.GameData.Resources.ThisTurnsDevCard.DevCardType == DevCardType.Knight);
            Debug.Assert(target.GameData.TimesTargeted == timesTargetted + 1);

        }

        private async Task RollSevenAndMoveBaron(PlayerModel target)
        {
            var currentTile = CTRL_GameView.CurrentGame.HexPanel.BaronTile;
            int knightsPlayed = CurrentPlayer.GameData.Resources.KnightsPlayed;
            int timesTargetted = target.GameData.TimesTargeted;
            TileCtrl targetTile = null;
            foreach (var kvp in target.GameData.Settlements[0].BuildingToTileDictionary)
            {
                if (kvp.Value != currentTile)
                {
                    targetTile = kvp.Value;
                    break;
                }
            }
            Debug.Assert(targetTile != null);
            Debug.Assert(CurrentPlayer.GameData.Resources.UnspentEntitlements.Count == 0);

            await Test_DoRoll(4, 3, SpecialDice.None);
            Debug.Assert(CurrentGameState == GameState.MustMoveBaron);

            var victims = new List<string>()  {target.PlayerName};

            int startingBaronIndex = CTRL_GameView.BaronTile.Index;
            await MovedBaronLog.PostLog(this,
                                          victims,
                                          targetTile.Index,
                                          startingBaronIndex,
                                          TargetWeapon.Baron,
                                           MoveBaronReason.Rolled7,
                                            ResourceType.None);
            GameContainer.CurrentGame.HexPanel.BaronnAnimationSkipToEnd();
            Debug.Assert(CurrentGameState == GameState.WaitingForNext);
            Debug.Assert(CurrentPlayer.GameData.Resources.KnightsPlayed == knightsPlayed);
            Debug.Assert(target.GameData.TimesTargeted == timesTargetted + 1);

        }

        private async void OnTestInvasion(object sender, RoutedEventArgs e)
        {

            await TestInvasion();

        }
        private async Task TestInvasion()
        {
            try
            {

                MainPageModel.Testing = true;
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

                    await StartTestGame(info, true);
                }
                await TestCheckpointLog.AddTestCheckpoint(this);
                if (CurrentGameState != GameState.WaitingForRoll)
                {
                    this.TraceMessage($"can't continue with GameState.{CurrentGameState}");
                }

                var knights = new List<BuildingCtrl>();


                for (int i = 0; i < PlayingPlayers.Count; i++)
                {
                    Debug.Assert(CurrentGameState == GameState.WaitingForRoll);
                    await Test_DoRoll(6, 6, SpecialDice.Pirate);
                    //  get some knight entitlements
                    await PurchaseEntitlement(CurrentPlayer, Entitlement.BuyOrUpgradeKnight, CurrentGameState);

                    //
                    //  figure out where to build it

                    var knight = Test_FindBuildingPlacement(BuildingState.Knight);
                    await KnightLeftPointerPressed(knight); // builds it
                    knights.Add(knight);
                    await MoveToNextPlayer();

                }
                Debug.Assert(CurrentGameState == GameState.WaitingForRoll);
                await TestCheckpointLog.AddTestCheckpoint(this);
                Debug.Assert(MainPageModel.TotalKnightRanks == 0);
                //
                //  Scenario 1: Invasion loss with no winners
                bool playerLost = await RollUntilInvasion();
                Debug.Assert(playerLost);
                int count = await HandleInvasion();
                Debug.Assert(count == 3, "3 people should have lost Cities");

                //
                //  Nobody should have any entitlements yet, as they have to resolve the invasion before buying anything
                PlayingPlayers.ForEach(p => Debug.Assert(p.GameData.Resources.UnspentEntitlements.Count == 0));
                // all cities should be destroyed
                PlayingPlayers.ForEach(p => Debug.Assert(p.GameData.Cities.Count == 0));
                await RollbackToCheckpoint();
                Debug.Assert(this.CurrentGameState == GameState.WaitingForRoll);
                Debug.Assert(CurrentPlayer == PlayingPlayers[0]);
                Debug.Assert(MainPageModel.TotalKnightRanks == 0);


                //
                //  Scenario 2: Invasion with one winner.  they should get a victory point and 2 people should lose cities
                //  our initial conditions are that playe[0] has rolled, built a knight, and activated it.
                //  we expect desiredPlayer[1] and desiredPlayer[2] to lose a city and desiredPlayer[0] to get a Victory Point
                //
                await Test_DoRoll(6, 6, SpecialDice.Pirate);
                Debug.Assert(CurrentGameState == GameState.WaitingForNext);
                //  activate one knight
                Debug.Assert(knights[0].IsKnight);
                Debug.Assert(knights[0].Knight.KnightRank == KnightRank.Basic);
                Debug.Assert(knights[0].Knight.Activated == false);
                await PurchaseEntitlement(CurrentPlayer, Entitlement.ActivateKnight, CurrentGameState);
                await KnightLeftPointerPressed(knights[0]); // activate it



                Debug.Assert(knights[0].Knight.Activated == true);
                Debug.Assert(CurrentPlayer.GameData.Resources.UnspentEntitlements.Count == 0);
                Debug.Assert(PlayingPlayers[0].GameData.CK_Knights.Count == 1);
                Debug.Assert(MainPageModel.TotalKnightRanks == 1);


                //
                //  we will roll back to desiredPlayer[0] having a activated a knight
                await TestCheckpointLog.AddTestCheckpoint(this);

                bool playersLost = await RollUntilInvasion();
                Debug.Assert(playersLost);
                count = await HandleInvasion();
                Debug.Assert(count == 2, "2 people should have lost Cities");

                Debug.Assert(CurrentPlayer == PlayingPlayers[1]);
                Debug.Assert(PlayingPlayers[0].GameData.Resources.VictoryPoints == 1);
                Debug.Assert(MainPageModel.TotalKnightRanks == 0);

                // Roll back to 1 desiredPlayer with an Active Knight, no invasion yet
                await RollbackToCheckpoint();
                Debug.Assert(this.CurrentGameState == GameState.WaitingForNext);
                Debug.Assert(CurrentPlayer == PlayingPlayers[0]);
                Debug.Assert(PlayingPlayers[0].GameData.TotalKnightRank == 1); // should get the rank back after the rollback

                // Scenairio 3: Players lose the invasion, but 2 people have the most knights
                //              Initial conditions are Players[0] and Players[1] have knights that are active.
                //              Player[3] has a Knight, but it is not active
                //              Player[2] is the victim, loses a city.  Players 0 & 1 get DevCard.Any


                await MoveToNextPlayer();
                Debug.Assert(CurrentGameState == GameState.WaitingForRoll);
                await Test_DoRoll(6, 6, SpecialDice.Pirate);
                Debug.Assert(CurrentGameState == GameState.WaitingForNext);
                Debug.Assert(CurrentPlayer == PlayingPlayers[1]);


                //  activate one knight
                Debug.Assert(knights[1].IsKnight);
                Debug.Assert(knights[1].Knight.KnightRank == KnightRank.Basic);
                Debug.Assert(knights[1].Knight.Activated == false);
                await PurchaseEntitlement(CurrentPlayer, Entitlement.ActivateKnight, CurrentGameState);
                await KnightLeftPointerPressed(knights[1]); // activate it
                Debug.Assert(knights[1].Knight.Activated == true);


                Debug.Assert(CurrentPlayer.GameData.Resources.UnspentEntitlements.Count == 0);
                Debug.Assert(PlayingPlayers[0].GameData.TotalKnightRank == 1);
                Debug.Assert(PlayingPlayers[1].GameData.TotalKnightRank == 1);
                Debug.Assert(PlayingPlayers[2].GameData.TotalKnightRank == 0);
                Debug.Assert(MainPageModel.TotalKnightRanks == 2);
                //
                //  we will roll back to desiredPlayer[0] and playerIndex[1] having a activated a knight
                await TestCheckpointLog.AddTestCheckpoint(this);

                await RollUntilInvasion();
                count = await HandleInvasion();
                Debug.Assert(count == 1, "1 person should have lost Cities");
                // the CurrentPlayer is based on how many rools.  with 3 starting playerIndex, this is where we end up
                Debug.Assert(CurrentPlayer == PlayingPlayers[1]);
                PlayingPlayers.ForEach(player => Debug.Assert(player.GameData.Resources.VictoryPoints == 0));
                Debug.Assert(PlayingPlayers[0].GameData.Resources.CurrentResources.AnyDevCard == 1);
                Debug.Assert(PlayingPlayers[1].GameData.Resources.CurrentResources.AnyDevCard == 1);

                // Roll back to 2 playerIndex with an Active Knight, no invasion yet
                await RollbackToCheckpoint();
                await TestCheckpointLog.AddTestCheckpoint(this);
                Debug.Assert(this.CurrentGameState == GameState.WaitingForNext);
                Debug.Assert(CurrentPlayer == PlayingPlayers[1]);
                Debug.Assert(CurrentPlayer.GameData.Resources.UnspentEntitlements.Count == 0);
                Debug.Assert(PlayingPlayers[0].GameData.TotalKnightRank == 1);
                Debug.Assert(PlayingPlayers[1].GameData.TotalKnightRank == 1);
                Debug.Assert(PlayingPlayers[2].GameData.TotalKnightRank == 0);
                Debug.Assert(MainPageModel.TotalKnightRanks == 2);

                //
                //  Scenario 4: everybody has an active knight and the playerIndex win the invasion
                await MoveToNextPlayer();
                Debug.Assert(CurrentGameState == GameState.WaitingForRoll);
                await Test_DoRoll(6, 6, SpecialDice.Pirate);
                Debug.Assert(CurrentGameState == GameState.WaitingForNext);
                Debug.Assert(CurrentPlayer == PlayingPlayers[2]);
                //  activate one knight
                Debug.Assert(knights[2].IsKnight);
                Debug.Assert(knights[2].Knight.KnightRank == KnightRank.Basic);
                Debug.Assert(knights[2].Knight.Activated == false);
                await PurchaseEntitlement(CurrentPlayer, Entitlement.ActivateKnight, CurrentGameState);
                await KnightLeftPointerPressed(knights[2]); // activate it
                Debug.Assert(knights[2].Knight.Activated == true);
                Debug.Assert(MainPageModel.TotalKnightRanks >= MainPageModel.TotalCities);
                playersLost = await RollUntilInvasion();
                Debug.Assert(!playersLost);
                await HandleInvasion();
                Debug.Assert(CurrentGameState == GameState.WaitingForRoll);
                await RollbackToCheckpoint();
                Debug.Assert(CurrentGameState == GameState.WaitingForNext);
                await RollbackToCheckpoint();
                Debug.Assert(CurrentGameState == GameState.WaitingForRoll);
            }
            finally
            {
                MainPageModel.Testing = false;

            }

        }

        private async Task MoveToNextPlayer()
        {
            while (CurrentGameState != GameState.WaitingForRoll)
            {
                await NextState();
            }
        }

        //
        //  Enter with GameState == GameState.MustDestroyCity and Leave with GameState.WaitingForNext
        private async Task<int> HandleInvasion()
        {
            int count = 0;
            while (this.CurrentGameState == GameState.MustDestroyCity)
            {
                count++;
                Debug.Assert(CurrentPlayer.GameData.Resources.UnspentEntitlements.Count == 1);
                await DestroyCity(CurrentPlayer.GameData.Cities[0]);
                Debug.Assert(PlayingPlayers[0].GameData.Resources.UnspentEntitlements.Count == 0);
            }
            if (count > 0)
            {
                Debug.Assert(this.CurrentGameState == GameState.DoneDestroyingCities);
                await MoveToNextPlayer();
            }
            else
            {

                Debug.Assert(this.CurrentGameState == GameState.WaitingForRoll);
            }


            PlayingPlayers.ForEach(p => Debug.Assert(p.GameData.Resources.UnspentEntitlements.Count == 0));
            return count;
        }

        // keep rolling until the invasion count is incremented.  this works for both winning and losing an invasion
        // returns true if the playerIndex LOST the invasion
        private async Task<bool> RollUntilInvasion()
        {
            Debug.Assert(CurrentGameState == GameState.WaitingForRoll || CurrentGameState == GameState.WaitingForNext);

            if (CurrentGameState == GameState.WaitingForNext)
            {
                await MoveToNextPlayer();
            }

            while (CTRL_Invasion.InvasionData.CurrentStep != CTRL_Invasion.InvasionData.INVASION_STEPS)
            {

                await Test_DoRoll(6, 6, SpecialDice.Pirate);
                if (CurrentGameState == GameState.WaitingForNext)
                {
                    await MoveToNextPlayer(); // as apposed to starting the invasion
                }

            }

            return CurrentGameState == GameState.MustDestroyCity;
        }

        private void ForceBaronToRealTile()
        {
            var tile = CTRL_GameView.CurrentGame.HexPanel.BaronTile;
            int idx = tile.Index + 1 % CTRL_GameView.CurrentGame.Tiles.Count;
            var tempTile = CTRL_GameView.CurrentGame.Tiles.ElementAt(idx);
            CTRL_GameView.CurrentGame.HexPanel.BaronTile = tempTile;
            CTRL_GameView.CurrentGame.HexPanel.BaronnAnimationSkipToEnd();
            CTRL_GameView.CurrentGame.HexPanel.BaronTile = tile;
            CTRL_GameView.CurrentGame.HexPanel.BaronnAnimationSkipToEnd();

        }

        private async Task RollbackToState(GameState state)
        {
            int count = 0;
            while (Log.PeekAction.NewState != state)
            {
                if (Log.PeekAction.CanUndo)
                {
                    count++;
                    await DoUndo();

                }
                else
                {
                    Debug.Assert(false, "Did you forget to set a test checkpoint?");
                }
            }
        }

        public async Task RollbackToCheckpoint()
        {
            await RollbackToState(GameState.TestCheckpoint);

            await DoUndo();

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
            //foreach (var desiredPlayer in MainPageModel.PlayingPlayers)
            //{
            //    if (desiredPlayer == CurrentPlayer) continue;

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
            //        TradePartner = desiredPlayer,
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

                await StartTestGame(info, true);
                await Test_DoRoll(1, 2, SpecialDice.Pirate);
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

                await StartTestGame(info, true);
                await Test_DoRoll(1, 2, SpecialDice.Pirate);
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
            Debug.Assert(PlayingPlayers[0].GameData.CK_Knights.Count == 1);
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

                await StartTestGame(info, true);
                await Test_DoRoll(1, 2, SpecialDice.Pirate);
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
            Debug.Assert(PlayingPlayers[0].GameData.CK_Knights.Count == 1);
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
            Debug.Assert(PlayingPlayers[0].GameData.CK_Knights.Count == 0);
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
            await DoUndo(); // back to desiredPlayer[0]
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

        private async Task UndoToPlayer(PlayerModel player, int millisecsDelay = 0)
        {

            while (CurrentPlayer != player)
            {
                this.TraceMessage($"Undoing: {this.Log.PeekAction}");
                await DoUndo();
                await Task.Delay(millisecsDelay);
            }


        }
        #endregion Methods
    }

}
