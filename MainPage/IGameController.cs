using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Threading.Tasks;

using Catan.Proxy;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class MainPage : Page, IGameController
    {

        #region Methods

        private async Task UpdateUiForState(GameState currentState)
        {
            switch (currentState)
            {
                case GameState.Uninitialized:
                    break;

                case GameState.WaitingForNewGame:
                    break;

                case GameState.BeginResourceAllocation:
                    if (MainPageModel.Settings.AutoRespond && MainPageModel.GameInfo.Creator == TheHuman.PlayerName)
                    {
                        await SetStateLog.SetState(this, GameState.AllocateResourceForward);
                    }
                    break;

                case GameState.WaitingForPlayers:
                    //
                    //   if you go back to waiting for players, put the tiles facedown
                    GameContainer.CurrentGame.Tiles.ForEach((tile) => tile.TileOrientation = TileOrientation.FaceDown);
                    if (MainPageModel.Settings.AutoRespond && MainPageModel.GameInfo.Creator == TheHuman.PlayerName)
                    {
                        //
                        //  simulate clicking on Next
                        await SetStateLog.SetState(this, GameState.PickingBoard);
                    }
                    break;

                case GameState.PickingBoard:

                    if (MainPageModel.GameInfo.Creator == TheHuman.PlayerName)
                    {
                        //
                        //  we only need one person sending around a random board
                        await RandomBoardLog.RandomizeBoard(this, 0);
                        if (MainPageModel.Settings.AutoRespond)
                        {
                            //
                            //  simulate clicking on Next
                            await SetStateLog.SetState(this, GameState.WaitingForRollForOrder);
                        }
                    }
                    break;

                case GameState.WaitingForRollForOrder:
                    //
                    //  turn off pips

                    _gameView.AllBuildings.ForEach((building) => building.Reset()); // turn off pips on the local machine

                    if (MainPageModel.Settings.AutoRespond)
                    {
                        Random rand = new Random();
                        await RollOrderLog.PostMessage(this, PickingBoardToWaitingForRollOrder.GetRollModelList());
                    }

                    //
                    //  When leaving WaitingForRollOrder, next state is going to be AllocationResourcesForward -- get rid of the UI that shows all the player rolls

                    break;

                case GameState.AllocateResourceForward:
                    MainPageModel.PlayingPlayers.ForEach((p) =>
                    {
                        p.GameData.RollOrientation = TileOrientation.FaceDown;
                        p.GameData.SyncronizedPlayerRolls.CurrentRoll.RedDie = 0;
                        p.GameData.SyncronizedPlayerRolls.CurrentRoll.WhiteDie = 0;
                    });

                    //
                    //  during allocation phase, you get one road and one settlement
                    CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.Road);
                    CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.Settlement);
                    break;

                case GameState.AllocateResourceReverse:
                    //
                    //  during allocation phase, you get one road and one settlement
                    CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.Road);
                    CurrentPlayer.GameData.Resources.GrantEntitlement(Entitlement.Settlement);
                    break;

                case GameState.DoneResourceAllocation:
                    break;

                case GameState.WaitingForRoll:

                    break;

                case GameState.WaitingForNext:

                    break;

                case GameState.Supplemental:
                    break;

                default:
                    break;
            }
        }

        #endregion Methods

        #region Properties

        public bool AutoRespondAndTheHuman => ( this.MainPageModel.Settings.AutoRespond && CurrentPlayer == TheHuman );
        public PlayerModel LastPlayerToRoll { get; set; }

        public CatanGames CatanGame { get; set; } = CatanGames.Regular;

        public GameState CurrentGameState
        {
            get
            {
                if (MainPageModel.Log == null) return GameState.WaitingForNewGame;

                return MainPageModel.GameState;
            }
        }

        public List<int> CurrentRandomGoldTiles => _gameView.CurrentRandomGoldTiles;

        public CatanGameData GameData => this.GameContainer.CurrentGame.GameData;

        public GameInfo GameInfo => MainPageModel.GameInfo;

        public List<int> HighlightedTiles
        {
            get
            {
                var list = new List<int>();
                foreach (var tile in GameContainer.AllTiles)
                {
                    if (tile.Highlighted)
                    {
                        list.Add(tile.Index);
                    }
                }
                return list;
            }
        }

        public PlayerModel NextPlayer
        {
            get
            {
                List<PlayerModel> playingPlayers = PlayingPlayers;

                int idx = playingPlayers.IndexOf(CurrentPlayer);

                Contract.Assert(idx != -1, "The player needs to be playing!");

                int count = playingPlayers.Count;

                // Calculate the index of the next player, wrapping around if necessary
                int nextIndex = (idx + 1) % count;

                return playingPlayers[nextIndex];
            }
        }

        public bool IsServiceGame => MainPageModel.IsServiceGame;

        public Log Log => MainPageModel.Log;

        public List<PlayerModel> PlayingPlayers => new List<PlayerModel>(MainPageModel.PlayingPlayers);

        public ICatanService Proxy => MainPageModel?.CatanService;
        public IRollLog RollLog => MainPageModel.Log.RollLog as IRollLog;

        #endregion Properties

        /// <summary>
        ///     Called when a Player is added to a Service game
        ///
        ///     this can also be called directly by the user via UI interaction, so we need to make sure that works idenitically as if
        ///     the message came from another machine.
        ///
        ///     5/16/2020
        ///                 - Let the players be out of order. Each client will see 'TheHuman' as the first player in the list
        ///                 - Make the CurrentPlayer be whoever created the game (so the Next button is correctly updated)
        ///                 - when we transition out of Rolling for first, we'll sync the list order
        ///
        ///     5/23/2020
        ///                 - CreateGame adds the player
        ///                 - we have a latent bug if the user adds themselves to an existing game instead of using the auto connect feature
        ///
        /// </summary>
        /// <param name="playerLogHeader"></param>
        /// <returns></returns>
        public async Task AddPlayer(string playerToAdd)
        {
            if (playerToAdd == "Catan Spy") await Task.Delay(0);
            //
            //  7/25/2020:  if you are replaying a game, this call comes in at GameState.FinishedRollOrder
            Contract.Assert(CurrentGameState == GameState.WaitingForPlayers || CurrentGameState == GameState.WaitingForNewGame || CurrentGameState == GameState.FinishedRollOrder);

            var newPlayer = NameToPlayer(playerToAdd);
            Contract.Assert(newPlayer != null);
            if (MainPageModel.PlayingPlayers.Contains(newPlayer) == false)
            {
                AddPlayerMenu(newPlayer);

                //
                //  need to give the players some data about the game
                newPlayer.GameData.MaxCities = _gameView.CurrentGame.GameData.MaxCities;
                newPlayer.GameData.MaxRoads = _gameView.CurrentGame.GameData.MaxRoads;
                newPlayer.GameData.MaxSettlements = _gameView.CurrentGame.GameData.MaxSettlements;
                newPlayer.GameData.MaxShips = _gameView.CurrentGame.GameData.MaxShips;
                newPlayer.GameData.MaxKnights = _gameView.CurrentGame.GameData.MaxKnights;
                newPlayer.GameData.NotificationsEnabled = true;
                newPlayer.AddedTime = DateTime.Now;
                newPlayer.GameData.Trades.TradeRequest.Owner.Player = newPlayer;

                //
                //  insert based on the time the message was sent
                bool added = false;
                for (int i = 0; i < MainPageModel.PlayingPlayers.Count; i++)
                {
                    if (newPlayer.AddedTime < MainPageModel.PlayingPlayers[i].AddedTime)
                    {
                        MainPageModel.PlayingPlayers.Insert(i, newPlayer);
                        added = true;
                        break;
                    }
                }
                if (!added) // put at end
                {
                    MainPageModel.PlayingPlayers.Add(newPlayer);
                }
                //
                //  Whoever starts the game controls the game until a first player is picked -- this doesn't have to happen
                //  as a message because we assume the clocks are shared
                //
                if (MainPageModel.GameInfo == null || !String.IsNullOrEmpty(MainPageModel.GameInfo.Creator))
                {
                    CurrentPlayer = NameToPlayer(MainPageModel.GameInfo.Creator);
                }
                else
                {
                    CurrentPlayer = NameToPlayer(playerToAdd);
                }
            }
            else
            {
                this.TraceMessage($"Recieved an AddPlayer call for {newPlayer} when they are already in the game");
            }

            await Task.Delay(0);
        }

        public async Task ChangePlayer(ChangePlayerLog changePlayerLog)
        {
            // this controller is the one spot where the CurrentPlayer should be changed.  it should update all the bindings
            // the setter will update all the associated state changes that happen when the CurrentPlayer
            // changes

            CurrentPlayer = NameToPlayer(changePlayerLog.NewCurrentPlayer);

            //
            // always stop highlighted the rollModel when the player changes
            GameContainer.AllTiles.ForEach((tile) => tile.StopHighlightingTile());

            //
            //  in supplemental, we don't show random gold tiles
            if (CurrentGameState == GameState.Supplemental)
            {
                await GameContainer.ResetRandomGoldTiles();
            }

            //
            // when we change player we optionally set tiles to be randomly gold - iff we are moving forward (not undo)
            // we need to check to make sure that we haven't already picked random goal tiles for this particular role.  the scenario is
            // we hit Next and are waiting for a role (and have thus picked random gold tiles) and then hit undo for some reason so that the
            // previous player can finish their turn.  when we hit Next again, we want the same tiles to be chosen to be gold.
            if (( changePlayerLog.NewState == GameState.WaitingForRoll ) || ( changePlayerLog.NewState == GameState.WaitingForNext ))
            {
                // 12/28/2023 - changing this to be an explicit state change
                // await SetRandomTileToGold(changePlayerLog.NewRandomGoldTiles);

                await ToPickGold.PostLog(this, RandomGoldTileCount);
            }

            if (changePlayerLog.NewState != changePlayerLog.OldState)
            {
                await UpdateUiForState(changePlayerLog.NewState);
            }
        }

        public void CompleteRedo()
        {
            // this.TraceMessage("complete");
        }

        public void CompleteUndo()
        {
            // this.TraceMessage("complete");
        }

        public RandomBoardSettings CurrentRandomBoard()
        {
            return _gameView.RandomBoardSettings;
        }

        /// <summary>
        ///     this is where we do the work to synchronize a rollModel across devices.
        ///
        ///     General Algorythm:
        ///
        ///     we start with the CurrentRoll < 0
        ///     if anybody has a CurrentRoll < 0, wait for all the rolls to come in
        ///     When all rolls have come in, look for ties.
        ///     if anybody has a tie, make a CurrentRoll < 0 and add it to their Rolls
        ///     repeat until there are no ties and there are no CurrentRolls < 0
        ///
        /// </summary>
        /// <param name="logEntry"></param>
        /// <returns>true if everybody has rolled and their are no ties</returns>
        public async Task<bool> DetermineRollOrder(RollOrderLog logEntry)
        {

            try
            {
                //
                // need to update the state first because data binding is used to show/hide rollModel UI and it is driven off of
                // GameState.  the end of this function changes the state to GameState.WaitingForRollForOrder as well

                Contract.Assert(logEntry.NewState == GameState.WaitingForRollForOrder);

                PlayerModel sentBy = logEntry.SentBy;

                Contract.Assert(sentBy != null);

                //
                //  when this is called we either have no current rolls, or we hit a tie.
                //  if we hit a tie, then MustRoll == true

                RollModel pickedRoll = sentBy.GameData.SyncronizedPlayerRolls.AddRoll(logEntry.Rolls);

                Contract.Assert(pickedRoll != null);

                DumpAllRolls();

                //
                //  7/4/2020: has everybody rolled? -- don't make any decisions until the rolls are in
                //            we check to see if we are waiting for a rollModel by looking a flag
                //
                List<PlayerModel> waiting = new List<PlayerModel>();
                foreach (var p in MainPageModel.PlayingPlayers)
                {
                    if (p.GameData.SyncronizedPlayerRolls.MustRoll)
                    {
                        waiting.Add(p);
                    }
                }

                if (waiting.Count > 0)
                {
                    this.TraceMessage($"Waiting for Rolls from {PlayerListToCsv(waiting)}");
                    return false;

                }

                this.TraceMessage("All rolls in.  looking for ties.");

                //
                //  the reason you don't return emmediately is you need to find *all* the ties
                //
                bool somebodyIsTied = false;
                bool showDialog = false;

                //
                //  7/29/2020: be sure and loop over *all* the players and set which ones are in a tie and need to rollModel again.
                //  
                List<PlayerModel> playersInTie = new List<PlayerModel>();
                foreach (var p in PlayingPlayers)
                {
                    //
                    //  reset MustRoll flag
                    p.GameData.SyncronizedPlayerRolls.MustRoll = false;
                    if (PlayerInTie(p).Count > 0) // this means player == p is tied with somebody
                    {
                        playersInTie.Add(p); // we will catch the one that we are tied when we iterate to that player
                        p.GameData.SyncronizedPlayerRolls.MustRoll = true;
                        somebodyIsTied = true;
                        if (p == TheHuman)
                        {
                            //
                            //  if TheHuman is tied, show the dialog
                            showDialog = true;
                        }
                    }
                }
                if (somebodyIsTied)
                {
                    this.TraceMessage($"Players {PlayerListToCsv(playersInTie)} are tied");
                }

                if (showDialog && VisualTreeHelper.GetOpenPopups(Window.Current).Count == 0) // if the dialog is already shown, don't show it again
                {
                    //
                    //  waiting for a rollModel from p...                            
                    ContentDialog dlg = new ContentDialog()
                    {
                        Title = "Catan - Roll For Order",
                        Content = $"You are tied with {PlayerListToCsv(playersInTie)}.\n\nRollAgain.",
                        CloseButtonText = "Ok",
                    };

                    await dlg.ShowAsync();
                }

                if (somebodyIsTied) return false;

                this.TraceMessage("All Rolls in.  Setting final order");

                //
                //  we got here because nobody is tied and all rolls have come in

                //
                // set the order locally on each machine

                var newList = new List<PlayerModel>(MainPageModel.PlayingPlayers);
                newList.Sort((x, y) => x.GameData.SyncronizedPlayerRolls.CompareTo(y.GameData.SyncronizedPlayerRolls));
                for (int i = 0; i < newList.Count; i++)
                {
                    MainPageModel.PlayingPlayers[i] = newList[i];
                }

                //
                //  because all players are sharing the rolls, we can implicitly set the CurrentPlayer w/o sharing that we have done so.
                CurrentPlayer = MainPageModel.PlayingPlayers[0];
                return true;
            }
            finally
            {

            }
        }

        public Task ExecuteSynchronously(LogHeader logHeader, ActionType msgType, MessageType messageType)
        {
            CatanMessage message = new CatanMessage()
            {
                Data = (object)logHeader,
                From = TheHuman.PlayerName,
                ActionType = msgType,
                DataTypeName = logHeader.GetType().FullName,
                GameInfo = MainPageModel.GameInfo,
                MessageType = messageType
            };

            MainPageModel.ChangeUnprocessMessage(1);
            return ProcessMessage(message);
        }

        /// <summary>
        ///     We store Rolls as RollModels which have both the GoldTiles (set before RollModel) and the RollModel (set after RollModel).
        ///     Once you are assigned a RollModel, that is your rollModel.  you can undo it, but when Redo (or even Next) happens,
        ///     you get the same Gold Tiles and the same value of the rollModel.
        /// </summary>
        /// <returns>the rollState object to use for this turn</returns>
        public RollState GetNextRollState()
        {
            var rollState = Log.RollLog.PopUndoneRoll();
            if (rollState == null)
            {
                rollState = new RollState()
                {

                    PlayerName = CurrentPlayer.PlayerName
                };
            }

            return rollState;
        }

        public RandomBoardSettings GetRandomBoard()
        {
            return _gameView.GetRandomBoard();
        }

        public List<int> GetRandomGoldTiles()
        {
            if (!this.RandomGold || this.RandomGoldTileCount < 1) return new List<int>();
            var currentRandomGoldTiles = _gameView.CurrentRandomGoldTiles;
            return _gameView.PickRandomTilesToBeGold(RandomGoldTileCount, currentRandomGoldTiles);
        }

        /// <summary>
        ///     starting back as early as possible -- load MainPageModel from disk and recreate all the players.
        /// </summary>
        /// <returns></returns>
        public async Task InitializeMainPageModel()
        {
            var serviceReference = MainPageModel?.CatanService;
            _gameView.Reset();
            _gameView.SetCallbacks(this, this);
            await LoadMainPageModel();
            UpdateGridLocations();
            _progress.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            _progress.IsActive = false;

            //    CurrentPlayer = TheHuman;
            this.TraceMessage("If you want to set the CurrentPlayer to startup, do it here");

        }

        /// <summary>
        ///     Not a lot to do when Start happens.  Just get ready for the board to get set and players to be added.
        ///     We do keep track of who created the game as they are the ones that have to click "Start" to stop the addition
        ///     of new players.
        ///     
        ///     if we are playing a CitiesAndKnights game, the Baron starts out hidden
        /// 
        /// </summary>
        /// <param name="logHeader"></param>
        /// <returns></returns>
        public async Task CreateGame(GameInfo gameInfo)
        {

            MainPageModel.GameInfo = gameInfo;
            _gameView.CurrentGame = _gameView.Games[gameInfo.GameIndex];
            MainPageModel.IsGameStarted = true;
            if (gameInfo.CitiesAndKnights)
            { _gameView.CurrentGame.HexPanel.HideBaron(); }
            await Task.Delay(0);
        }

        public PlayerModel NameToPlayer(string playerName)
        {
            Contract.Assert(MainPageModel.AllPlayers.Count > 0);
            foreach (var player in MainPageModel.AllPlayers)
            {
                if (player.PlayerName == playerName)
                {
                    return player;
                }
            }
            return null;
        }

        public PlayerModel NameToPlayer(PlayerModel player)
        {
            return player;
        }

        /// <summary>
        ///     return
        ///         name1
        ///             or
        ///        name1 and name2
        ///             or
        ///        name1, name2, name3, and name4
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>
        public string PlayerListToCsv(List<PlayerModel> players)
        {
            int count = players.Count;
            if (count == 0) return "";
            if (count == 1) return players[0].PlayerName;
            if (count == 2) return $"{players[0].PlayerName} and {players[1].PlayerName}";

            string s = $"{players[0].PlayerName}, ";
            for (int i = 1; i < count - 1; i++)
            {
                s += $"{players[i].PlayerName}, ";
            }
            s += $"and {players[count - 1].PlayerName}";
            return s;
        }

        /// <summary>
        ///     Checks to see if it is a service game.  if it is, post a message to the service.
        ///     if not, process the message immediately
        /// </summary>
        /// <param name="logHeader"></param>
        /// <param name="normal"></param>
        /// <returns>False if the IGameController.Do() should be executed locally </returns>
        public async Task<bool> PostMessage(LogHeader logHeader, ActionType msgType)
        {
            if (MainPageModel.GameInfo == null) return false; // can be null druing testing scenarios

            CatanMessage message = new CatanMessage()
            {
                GameInfo = this.MainPageModel.GameInfo,
                Data = logHeader,
                From = TheHuman.PlayerName,
                ActionType = msgType,
                DataTypeName = logHeader.GetType().FullName,
                To = "*"
            };

            MainPageModel.ChangeUnprocessMessage(1);

            //
            //  if you start spy before a game starts, nobody is there to recieve the message and decrement the message counter...
            //  so will will call it directly to make the UI update correctly.
            if (MainPageModel.GameState == GameState.WaitingForNewGame && logHeader.TypeName == typeof(CatanSpyLog).FullName)
            {
                await ProcessMessage(message);
                return true;
            }

            if (MainPageModel.Settings.IsLocalGame)
            {
                await ProcessMessage(message);
            }
            else
            {
                await MainPageModel.CatanService.SendBroadcastMessage(message);
            }
            //  this.TraceMessage($"returning PostMessage {message.DataTypeName} for id={message.MessageId}");
            return ( !MainPageModel.Settings.IsLocalGame );
        }

        public DevCardType PurchaseNextDevCard()
        {
            return _gameView.CurrentGame.GetNextDevCard();
        }

        public async Task<bool> RedoAsync()
        {
            LogHeader logHeader = Log.PeekUndo;
            if (logHeader == null) return false;

            await MainPage.Current.PostMessage(logHeader, ActionType.Redo);

            return true;
        }

        public void ResetAllBuildings()
        {
            _gameView.AllBuildings.ForEach((building) => building.Reset()); // turn off pips on the local machine
        }

        public async Task ResetRandomGoldTiles()
        {
            await _gameView.ResetRandomGoldTiles();
        }

        public async Task ResetRollControl()
        {
            await Task.Delay(0);
        }

        //
        //  find all the tiles with building for this rollModel where the owner == Player
        public (TradeResources Granted, TradeResources Baroned) ResourcesForRoll(PlayerModel player, RollModel rollModel, RollAction action)
        {
            TradeResources tr = new TradeResources();
            TradeResources baron = new TradeResources();
            foreach (BuildingCtrl building in _gameView.CurrentGame.HexPanel.Buildings)
            {
                if (building == null) continue;
                if (building.Owner != player) continue;
                foreach (var kvp in building.BuildingToTileDictionary)
                {
                    if (kvp.Value.Number == rollModel.Roll)
                    {
                        if (kvp.Value.HasBaron == false)
                        {
                            if (MainPageModel.GameInfo.CitiesAndKnights && ( building.BuildingState == BuildingState.City || building.BuildingState == BuildingState.Metropolis ))
                            {
                                //
                                //  in pirates - you get one of the resource types plus one commodity when you have a City or Metropolis
                                tr.AddResource(kvp.Value.ResourceType, 1);
                                switch (kvp.Value.ResourceType)
                                {
                                    case ResourceType.Sheep:
                                        tr.AddResource(ResourceType.Cloth, 1);
                                        break;
                                    case ResourceType.Wood:
                                        tr.AddResource(ResourceType.Paper, 1);
                                        break;
                                    case ResourceType.Ore:
                                        tr.AddResource(ResourceType.Coin, 1);
                                        break;
                                    case ResourceType.Wheat:

                                    case ResourceType.Brick:

                                    case ResourceType.GoldMine:
                                        tr.AddResource(kvp.Value.ResourceType, 1);
                                        break;

                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                tr.AddResource(kvp.Value.ResourceType, building.ScoreValue);
                            }
                        }
                        else
                        {
                            baron.AddResource(kvp.Value.ResourceType, building.ScoreValue);
                        }
                    }
                }
            }

            switch (rollModel.SpecialDice)
            {
                // if a player is *Rank=1, then a roll of 1 or 2 on the Red Dice pays

                case SpecialDice.Trade:
                    if (player.GameData.TradeRank > 0 && player.GameData.TradeRank + 1 >= rollModel.RedDie)
                    {
                        tr.AddResource(ResourceType.Trade, 1);
                    }
                    break;
                case SpecialDice.Politics:
                    if (player.GameData.PoliticsRank > 0 && player.GameData.PoliticsRank + 1 >= rollModel.RedDie)
                    {
                        tr.AddResource(ResourceType.Politics, 1);
                    }
                    break;
                case SpecialDice.Science:
                    if (player.GameData.ScienceRank > 0 && player.GameData.ScienceRank + 1 >= rollModel.RedDie)
                    {
                        tr.AddResource(ResourceType.Science, 1);
                    }
                    break;
                case SpecialDice.Pirate:
                    break;
                case SpecialDice.None:
                    break;
                default:
                    break;
            }

            if (action == RollAction.Undo)
            {
                tr = tr.GetNegated();
                baron = baron.GetNegated();
            }
            return (tr, baron);
        }

        public void SetHighlightedTiles(int roll)
        {
            foreach (var tile in _gameView.AllTiles)
            {
                if (tile.Number != roll)
                {
                    tile.AnimateFadeAsync(0.25);
                    tile.StopHighlightingTile();
                }
                else
                {
                    tile.HighlightTile();
                }
            }
        }

        /// <summary>
        ///     Set a random board.  Only the creator can set a random board.
        ///
        ///
        /// </summary>
        /// <param name="randomBoard"></param>
        /// <returns></returns>
        public async Task SetRandomBoard(RandomBoardLog randomBoard)
        {
            Contract.Assert(CurrentGameState == GameState.PickingBoard); // the first is true the first time through then it is the second
            Contract.Assert(randomBoard.NewState == GameState.PickingBoard);

            if (this.GameContainer.AllTiles[0].TileOrientation == TileOrientation.FaceDown)
            {
                await VisualShuffle(randomBoard.NewRandomBoard);
            }
            else
            {
                await _gameView.SetRandomCatanBoard(true, randomBoard.NewRandomBoard);
            }

            UpdateBoardMeasurements();
        }

        public async Task SetRandomTileToGold(List<int> goldTilesIndices)
        {
            await ResetRandomGoldTiles();
            if (this.RandomGold && this.RandomGoldTileCount > 0)
            {
                await _gameView.SetRandomTilesToGold(goldTilesIndices);
            }
        }

        public async Task SetRoadState(UpdateRoadLog updateRoadModel)
        {
            RoadCtrl road = GetRoad(updateRoadModel.RoadIndex);
            Contract.Assert(road != null);
            var player = NameToPlayer(updateRoadModel.SentBy);
            Contract.Assert(player != null);
            string raceTrackCopy = JsonSerializer.Serialize<RoadRaceTracking>(updateRoadModel.OldRaceTracking);
            RoadRaceTracking newRaceTracker = JsonSerializer.Deserialize<RoadRaceTracking>(raceTrackCopy);
            Contract.Assert(newRaceTracker != null);
            if (road.Owner != null)
            {
                this.TraceMessage("Owner changing!");
            }

            UpdateRoadState(player, road, updateRoadModel.OldRoadState, updateRoadModel.NewRoadState, newRaceTracker);

            await Task.Delay(0);
        }

        public void SetSpyInfo(string sentBy, bool spyOn)
        {
            this.SpyVisible = true;
            this.TurnedSpyOn = sentBy;
        }

        /// <summary>
        ///     Need to clean up any UI actions  -- e.g. if the GameStarter clicks on "RollModel to See who goes first", then that will cause
        ///     messages to be Posted to the other clients.  They end up calling this function.  it needs to be exactly the same as if the
        ///     button was clicked on.
        /// </summary>
        /// <param name="logHeader"></param>
        /// <returns></returns>
        public async Task SetState(SetStateLog logHeader)
        {
            await UpdateUiForState(logHeader.NewState);
        }

        public void ShowRollsInPublicUi()
        {
            MainPageModel.PlayingPlayers.ForEach((p) =>
            {
                p.GameData.RollOrientation = TileOrientation.FaceUp;
            });
        }

        public void StopHighlightingTiles()
        {
            GameContainer.AllTiles.ForEach((tile) => tile.StopHighlightingTile());
        }

        public async Task TellServiceGameStarted()
        {
            if (MainPageModel.CatanService == null) await Task.Delay(0);
            this.TraceMessage("you took this out.  put it back or delete it.");
            // await MainPageModel.CatanService.StartGame(MainPageModel.GameInfo);
            await Task.Delay(0);
        }

        public TileCtrl TileFromIndex(int targetTile)
        {
            return GameContainer.TilesInIndexOrder[targetTile];
        }

        /// <summary>
        ///     we don't call the Log to push the undo action -- that is done by the log since the log
        ///     initiates all Undo
        /// </summary>
        /// <param name="playerLogHeader"></param>
        /// <returns></returns>
        public async Task UndoAddPlayer(AddPlayerLog playerLogHeader)
        {
            var player = NameToPlayer(playerLogHeader.SentBy);
            Contract.Assert(player != null, "Player Can't Be Null");
            MainPageModel.PlayingPlayers.Remove(player);
            await Task.Delay(0);
        }

        public async Task<bool> UndoAsync()
        {
            LogHeader logHeader = Log.PeekAction;
            if (logHeader == null || logHeader.CanUndo == false) return false;

            await MainPage.Current.PostMessage(logHeader, ActionType.Undo);

            return true;
        }

        public async Task UndoChangePlayer(ChangePlayerLog logHeader)
        {
            CurrentPlayer = NameToPlayer(logHeader.PreviousPlayer);

            if (logHeader.OldState == GameState.WaitingForNext)
            {
                if (logHeader.OldRandomGoldTiles != null)
                {
                    await SetRandomTileToGold(logHeader.OldRandomGoldTiles);
                }

                logHeader.HighlightedTiles.ForEach((idx) => GameContainer.AllTiles[idx].HighlightTile());
            }
        }

        public async Task UndoSetRandomBoard(RandomBoardLog logHeader)
        {
            if (logHeader.PreviousRandomBoard == null)
            {
                GameContainer.AllTiles.ForEach((tile) => tile.TileOrientation = TileOrientation.FaceDown);
            }
            else
            {
                await _gameView.SetRandomCatanBoard(true, logHeader.PreviousRandomBoard);
                UpdateBoardMeasurements();
            }
        }

        public async Task UndoSetRoadState(UpdateRoadLog updateRoadModel)
        {
            RoadCtrl road = GetRoad(updateRoadModel.RoadIndex);
            Contract.Assert(road != null);
            var player = NameToPlayer(updateRoadModel.SentBy);
            Contract.Assert(player != null);

            UpdateRoadState(player, road, updateRoadModel.NewRoadState, updateRoadModel.OldRoadState, updateRoadModel.OldRaceTracking);

            await Task.Delay(0);
        }

        //
        //  State in the game is stored at the top of the NewLog.ActionStack
        //  so "Undoing" state is just moving the record from the ActionStack
        //  to the Undo stack
        public async Task UndoSetState(SetStateLog setStateLog)
        {
            await UpdateUiForState(setStateLog.OldState);
        }

        public async Task UndoUpdateBuilding(UpdateBuildingLog updateBuildingLog)
        {
            BuildingCtrl building = GetBuilding(updateBuildingLog.BuildingIndex);
            Contract.Assert(building != null);

            PlayerModel player = NameToPlayer(updateBuildingLog.SentBy);


            Contract.Assert(player != null);
            //
            //  5/26/2020: when we undo, we don't want to see the pips (which is the same as "Building")
            var oldState = updateBuildingLog.OldBuildingState;
            if (updateBuildingLog.OldBuildingState == BuildingState.Pips || updateBuildingLog.OldBuildingState == BuildingState.Build) oldState = BuildingState.None;

            //
            //  first set the building state correctly.
            await building.UpdateBuildingState(player, updateBuildingLog.NewBuildingState, oldState);

            // now fix any side effects of changing the state
            switch (updateBuildingLog.NewBuildingState) // this is the transition that originally was changed TO
            {
                case BuildingState.Metropolis:
                case BuildingState.None:

                    break;

                case BuildingState.Settlement:
                    if (updateBuildingLog.OldBuildingState == BuildingState.City) // Destoying a City
                    {
                        player.GameData.Resources.UnspentEntitlements.Add(Entitlement.DestroyCity);
                    }
                    else
                    {
                        player.GameData.Resources.UnspentEntitlements.Add(Entitlement.Settlement);
                    }
                    break;
                case BuildingState.City:
                    player.GameData.Resources.UnspentEntitlements.Add(Entitlement.City);
                    break;
                case BuildingState.NoEntitlement:
                    break;
                case BuildingState.Knight:
                    player.GameData.Resources.UnspentEntitlements.Add(Entitlement.BuyOrUpgradeKnight);
                    break;

                default:
                    Contract.Assert(false, $"should be city, settlement, or knight, got {updateBuildingLog.NewBuildingState}");
                    break;
            }



            if (updateBuildingLog.OldState == GameState.AllocateResourceReverse)
            {
                TradeResources tr = new TradeResources();
                foreach (var kvp in building.BuildingToTileDictionary)
                {
                    tr.AddResource(kvp.Value.ResourceType, -1);
                }
                CurrentPlayer.GameData.Resources.GrantResources(tr);
            }
        }

       

        public async Task MoveKnight(MoveKnightLog moveKnightLog, ActionType actionType)
        {
            BuildingCtrl building = GetBuilding(moveKnightLog.Index);
            Contract.Assert(building != null);
            PlayerModel player = NameToPlayer(moveKnightLog.SentBy);
            Contract.Assert(player != null);
            if (actionType == ActionType.Undo)
            {
                // Undo means that we put the knight back in the location
                await building.UpdateBuildingState(player, BuildingState.None, BuildingState.Knight);
                building.Knight.KnightRank = moveKnightLog.KnightRank;
                building.Knight.Activated = true;
                player.GameData.Resources.GrantEntitlement(Entitlement.MoveKnight);
            }
            else
            {
                // refund the whole knight -- rely on the players to enforce the fules
                for (int i = 0; i < ( int )moveKnightLog.KnightRank; i++)
                {
                    player.GameData.Resources.GrantEntitlement(Entitlement.BuyOrUpgradeKnight);

                }

                await building.UpdateBuildingState(player, BuildingState.Knight, BuildingState.None);
                player.GameData.Resources.ConsumeEntitlement(Entitlement.MoveKnight);

            }
        }
        /// <summary>
        ///     in pirates, the red die is special (possible grants dev cards)
        ///     if the special die is a pirate, we increment the journey.
        ///     if the journey is done, we have an invasion.
        ///     the normal rollModel to allocate resources is handled outside (prior) to this function.
        /// </summary>
        /// <param name="rollModel"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public async Task HandlePirateRoll(RollModel rollModel, ActionType action)
        {
            Debug.Assert(MainPageModel.GameInfo.CitiesAndKnights);

            if (action != ActionType.Undo)
            {
                if (rollModel.SpecialDice == SpecialDice.Pirate)
                {
                    int count =  CTRL_Invasion.Next();
                    this.TraceMessage($"PIRATE      [Count={count}] [steps={CTRL_Invasion.StepsBeforeInvasion}]");
                    if (count < CTRL_Invasion.StepsBeforeInvasion)
                    {
                        //
                        //  we've moved the ship and now we just deal with the rollModel

                        await WaitingForRollToWaitingForNext.PostRollMessage(this, rollModel);
                        await Task.Delay(10);
                        return;
                    }
                    if (count == CTRL_Invasion.StepsBeforeInvasion)
                    {
                        CTRL_Invasion.HideBaron();
                        this.GameContainer.CurrentGame.HexPanel.ShowBaron();

                        if (MainPageModel.TotalCities > MainPageModel.TotalKnightRanks)
                        {
                            // find lowest knight rank
                            int lowestKnightRank = 100; // arbitrary big number
                            int highestKnightRank = 0;
                            foreach (var player in PlayingPlayers)
                            {
                                int rank=0;

                                foreach (var knight in player.GameData.Knights)
                                {
                                    if (knight.Activated)
                                    {
                                        rank += ( int )knight.KnightRank;
                                    }
                                }

                                if (rank < lowestKnightRank)
                                {
                                    lowestKnightRank = rank;
                                }
                                if (rank > highestKnightRank)
                                {
                                    highestKnightRank = rank;
                                }
                            }

                            var playersWithHighestKnightRank = new List<PlayerModel>();

                            // go through and find all the victims
                            bool foundAtLeastOneVictim = false;
                            foreach (var player in PlayingPlayers)
                            {
                                int rank=0;

                                foreach (var knight in player.GameData.Knights)
                                {
                                    if (knight.Activated)
                                    {
                                        rank += ( int )knight.KnightRank;
                                    }
                                }

                                if (rank == lowestKnightRank)
                                {
                                    if (player.GameData.Cities.Count > 0)
                                    {
                                        player.GameData.Resources.GrantEntitlement(Entitlement.DestroyCity);
                                        foundAtLeastOneVictim = true;
                                    }

                                }

                                if (rank == highestKnightRank)
                                {
                                    playersWithHighestKnightRank.Add(player);
                                }

                            }

                            if (foundAtLeastOneVictim)
                            {
                                this.TraceMessage("DESTROY CITY: Start");
                                await DestroyCity_Next.PostLog(this);
                                this.TraceMessage("DESTROY CITY: End");
                            }

                            if (playersWithHighestKnightRank.Count == 1)
                            {
                                playersWithHighestKnightRank[0].GameData.Resources.VictoryPoints++;
                            }
                        }

                        /**
                         * after the invastion, all knights go inactive
                         */

                        foreach (var player in PlayingPlayers)
                        {

                            foreach (var knight in player.GameData.Knights)
                            {
                                knight.Activated = false;
                            }

                        }

                    }
                }
            }

            else
            {
                if (rollModel.SpecialDice == SpecialDice.Pirate)
                {
                    CTRL_Invasion.Previous();
                    if (CTRL_Invasion.InvasionCount == 0)
                    {
                        CTRL_Invasion.ShowBaron();
                        this.GameContainer.CurrentGame.HexPanel.HideBaron();

                    }
                }
            }

            await Task.Delay(10);

        }
        /**
         * find the city and the player, update the city.Wall property and consume or refund the entitlement
         */
        public async Task ProtectCity(ProtectCityLog protectCityLog, ActionType action)
        {
            BuildingCtrl building = GetBuilding(protectCityLog.BuildingIndex);
            Contract.Assert(building != null);
            Contract.Assert(building.IsCity);
            PlayerModel player = NameToPlayer(protectCityLog.SentBy);
            Contract.Assert(player != null);
            if (action == ActionType.Undo)
            {
                building.City.HasWall = false;
                player.GameData.Resources.GrantEntitlement(Entitlement.Wall);

            }
            else
            {
                building.City.HasWall = true;
                player.GameData.Resources.ConsumeEntitlement(Entitlement.Wall);
            }

            await Task.Delay(0);
        }
        public void SetBaronTile(TargetWeapon weapon, TileCtrl targetTile, bool showBaron)
        {
            if (showBaron)
            {
                // hide it in the invasion control, show it on th emain board
                CTRL_Invasion.HideBaron();
                this.GameContainer.CurrentGame.HexPanel.ShowBaron();
            }
            else
            {
                CTRL_Invasion.ShowBaron();
                this.GameContainer.CurrentGame.HexPanel.HideBaron();
            }
            if (weapon == TargetWeapon.PirateShip)
            {
                GameContainer.PirateShipTile = targetTile;
            }
            else
            {
                GameContainer.BaronTile = targetTile;
            }

        }

        public bool BaronVisibility
        {
            get
            {
                return GameContainer.CurrentGame.HexPanel.BaronShown;
            }
        }
        public async Task UpdateBuilding(UpdateBuildingLog updateBuildingLog, ActionType actionType)
        {
            BuildingCtrl building = GetBuilding(updateBuildingLog.BuildingIndex);
            Contract.Assert(building != null);
            PlayerModel player = CurrentPlayer;
            Contract.Assert(player != null);
            Entitlement entitlement = Entitlement.Undefined;
            switch (updateBuildingLog.NewBuildingState)
            {

                case BuildingState.Settlement:
                    if (updateBuildingLog.OldBuildingState == BuildingState.City)
                    {
                        entitlement = Entitlement.DestroyCity;
                    }
                    else
                    {
                        entitlement = Entitlement.Settlement;
                    }
                    break;
                case BuildingState.City:
                    entitlement = Entitlement.City;
                    break;

                case BuildingState.Knight:
                    entitlement = Entitlement.BuyOrUpgradeKnight;
                    break;

                default:
                    Contract.Assert(false, $"Should not call update building with oldState=BuildingState.{updateBuildingLog.NewBuildingState}");
                    break;
            }
            Debug.Assert(CurrentPlayer.GameData.Resources.HasEntitlement(entitlement));
            CurrentPlayer.GameData.Resources.ConsumeEntitlement(entitlement);
            await building.UpdateBuildingState(player, updateBuildingLog.OldBuildingState, updateBuildingLog.NewBuildingState);


            //
            // whenever we update a building, hide the pips
            if (building.BuildingState != BuildingState.Pips && building.BuildingState != BuildingState.None) 
            {
                await HideAllPipEllipses();
                _showPipGroupIndex = 0;
            }
            BuildingState oldState = updateBuildingLog.OldBuildingState;

            if (CurrentGameState == GameState.AllocateResourceReverse)
            {
                if (( building.BuildingState == BuildingState.Settlement && !MainPageModel.GameInfo.CitiesAndKnights ) || ( building.BuildingState == BuildingState.City && MainPageModel.GameInfo.CitiesAndKnights ))
                {

                    TradeResources tr = new TradeResources();
                    foreach (var kvp in building.BuildingToTileDictionary)
                    {
                        tr += TradeResources.TradeResourcesForBuilding(building.BuildingState, kvp.Value.ResourceType, MainPageModel.GameInfo.CitiesAndKnights);
                    }
                    switch (actionType)
                    {
                        case ActionType.Normal:
                        case ActionType.Redo:
                        case ActionType.Replay:
                        case ActionType.Retry:
                            CurrentPlayer.GameData.Resources.GrantResources(tr);
                            break;

                        case ActionType.Undo:
                            CurrentPlayer.GameData.Resources.GrantResources(tr.GetNegated());
                            break;
                        default:
                            break;
                    }

                }

            }

            CalculateAndSetLongestRoad();
            UpdateTileBuildingOwner(player, building, building.BuildingState, oldState);
        }

        /// <summary>
        ///     Function here so both Do and Undo can call it
        /// </summary>
        /// <param name="player"></param>
        /// <param name="road"></param>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        /// <param name="raceTracking"></param>
        public void UpdateRoadState(PlayerModel player, RoadCtrl road, RoadState oldState, RoadState newState, RoadRaceTracking raceTracking)
        {
            road.RoadState = newState;
            Contract.Assert(player != null);
            switch (newState)
            {
                case RoadState.Unowned:
                    if (oldState == RoadState.Ship)
                    {
                        player.GameData.Ships.Remove(road);
                        player.GameData.Resources.GrantEntitlement(Entitlement.Ship);
                    }
                    else
                    {
                        player.GameData.Roads.Remove(road);
                        player.GameData.Resources.GrantEntitlement(Entitlement.Road);
                    }

                    road.Owner = null;
                    road.Number = -1;
                    break;

                case RoadState.Road:
                    player.GameData.Resources.ConsumeEntitlement(Entitlement.Road);
                    road.Number = player.GameData.Roads.Count; // undo-able
                    Contract.Assert(player.GameData != null);
                    player.GameData.Roads.Add(road);
                    road.Owner = player;
                    break;

                case RoadState.Ship:
                    player.GameData.Resources.ConsumeEntitlement(Entitlement.Ship);
                    player.GameData.Roads.Remove(road); // can't be a ship if you aren't a road
                    player.GameData.Ships.Add(road);
                    break;

                default:
                    break;
            }

            CalculateAndSetLongestRoad(raceTracking);
        }

        private void DumpAllRolls()
        {
            string s = "\n";
            foreach (var player in PlayingPlayers)
            {
                s += $"{player.PlayerName}: ";
                player.GameData.SyncronizedPlayerRolls.RollValues.ForEach((p) => s += $"{p},");
                s += "\n";
            }
            this.TraceMessage(s);
        }

        /// <summary>
        ///     checks to see if the player is in a tie rollModel with any other players
        /// </summary>
        /// <param name="player"></param>
        /// <returns>The list of players in the tie</returns>
        private List<PlayerModel> PlayerInTie(PlayerModel player)
        {
            List<PlayerModel> list = new List<PlayerModel>();

            //
            //  look at all the rolls and see if the current player needs to rollModel again
            foreach (var p in MainPageModel.PlayingPlayers)
            {
                if (p == player) continue; // don't compare yourself to yourself
                if (p.GameData.SyncronizedPlayerRolls.RollValues.Count == 0) continue;
                if (p.GameData.SyncronizedPlayerRolls.CompareTo(player.GameData.SyncronizedPlayerRolls) == 0)
                {
                    list.Add(p);
                }
            }

            return list;
        }

        public async Task RolledSeven()
        {

            if (GameInfo.CitiesAndKnights && CTRL_Invasion.InvasionCount > 0)
            {
                await MustMoveBaronLog.PostLog(this, MoveBaronReason.Rolled7);
            }
        }

        public void MoveMerchant(Point to)
        {
            GameContainer.CurrentGame.HexPanel.MoveMerchantAsync(to);
        }

        public PlayerModel PlayerFromId(Guid id)
        {
            foreach (var player in PlayingPlayers)
            {
                if (player.PlayerIdentifier == id)
                    return player;
            }

            return null;
        }
    }
}