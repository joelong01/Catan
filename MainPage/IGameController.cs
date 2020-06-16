using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Threading.Tasks;

using Catan.Proxy;

using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class MainPage : Page, IGameController
    {

        #region Properties + Fields 

        private TaskCompletionSource<bool> UndoRedoTcs { get; set; }

        #endregion Properties + Fields 

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
                    if (MainPageModel.Settings.AutoRespond && MainPageModel.GameStartedBy == TheHuman)
                    {
                        await SetStateLog.SetState(this, GameState.AllocateResourceForward);
                    }
                    break;

                case GameState.WaitingForPlayers:
                    //
                    //   if you go back to waiting for players, put the tiles facedown
                    GameContainer.CurrentGame.Tiles.ForEach((tile) => tile.TileOrientation = TileOrientation.FaceDown);
                    if (MainPageModel.Settings.AutoRespond && MainPageModel.GameStartedBy == TheHuman)
                    {
                        //
                        //  simulate clicking on Next
                        await SetStateLog.SetState(this, GameState.PickingBoard);
                    }
                    break;

                case GameState.PickingBoard:

                    await HideRollsInPublicUi();
                    if (MainPageModel.GameStartedBy == TheHuman)
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
                        p.GameData.SyncronizedPlayerRolls.CurrentRoll.DiceOne = 0;
                        p.GameData.SyncronizedPlayerRolls.CurrentRoll.DiceTwo = 0;
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
                    await HideRollsInPublicUi();
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

        public bool AutoRespondAndTheHuman => (this.MainPageModel.Settings.AutoRespond && MainPageModel.GameStartedBy == TheHuman);

        public CatanGames CatanGame { get; set; } = CatanGames.Regular;

        public GameState CurrentGameState
        {
            get
            {
                if (MainPageModel.Log == null) return GameState.WaitingForNewGame;

                return MainPageModel.Log.GameState;
            }
        }

        public List<int> CurrentRandomGoldTiles => _gameView.CurrentRandomGoldTiles;

        public CatanGameData GameData => this.GameContainer.CurrentGame.GameData;

        public GameInfo GameInfo => MainPageModel.ServiceGameInfo;

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

        public bool IsServiceGame => MainPageModel.IsServiceGame;

        public Log Log => MainPageModel.Log;

        public List<PlayerModel> PlayingPlayers => new List<PlayerModel>(MainPageModel.PlayingPlayers);



        public IRollLog RollLog => MainPageModel.Log.RollLog as IRollLog;

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
        public Task AddPlayer(AddPlayerLog playerLogHeader)
        {
            Contract.Assert(CurrentGameState == GameState.WaitingForPlayers);

            var playerToAdd = NameToPlayer(playerLogHeader.PlayerToAdd);
            Contract.Assert(playerToAdd != null);
            if (MainPageModel.PlayingPlayers.Contains(playerToAdd) == false)
            {
                AddPlayerMenu(playerToAdd);

                //
                //  need to give the players some data about the game
                playerToAdd.GameData.MaxCities = _gameView.CurrentGame.GameData.MaxCities;
                playerToAdd.GameData.MaxRoads = _gameView.CurrentGame.GameData.MaxRoads;
                playerToAdd.GameData.MaxSettlements = _gameView.CurrentGame.GameData.MaxSettlements;
                playerToAdd.GameData.MaxShips = _gameView.CurrentGame.GameData.MaxShips;
                playerToAdd.GameData.NotificationsEnabled = true;
                playerToAdd.AddedTime = playerLogHeader.CreatedTime;
                int playerCount = MainPageModel.PlayingPlayers.Count;

                //
                //  insert based on the time the message was sent
                bool added = false;
                for (int i = 0; i < MainPageModel.PlayingPlayers.Count; i++)
                {
                    if (playerToAdd.AddedTime < MainPageModel.PlayingPlayers[i].AddedTime)
                    {
                        MainPageModel.PlayingPlayers.Insert(i, playerToAdd);
                        added = true;
                        break;
                    }
                }
                if (!added) // put at end
                {
                    MainPageModel.PlayingPlayers.Add(playerToAdd);
                }
                //
                //  Whoever starts the game controls the game until a first player is picked -- this doesn't have to happen
                //  as a message because we assume the clocks are shared
                //
                MainPageModel.GameStartedBy = PlayingPlayers[0];
                if (CurrentPlayer != MainPageModel.GameStartedBy)
                {
                    CurrentPlayer = MainPageModel.GameStartedBy;
                }
                
            }
            else
            {
                this.TraceMessage($"Recieved an AddPlayer call for {playerToAdd} when they are already in the game");
            }

            return Task.CompletedTask;
        }

        public async Task ChangePlayer(ChangePlayerLog changePlayerLog)
        {
            // this controller is the one spot where the CurrentPlayer should be changed.  it should update all the bindings
            // the setter will update all the associated state changes that happen when the CurrentPlayer
            // changes

            CurrentPlayer = NameToPlayer(changePlayerLog.NewCurrentPlayer);

            //
            // always stop highlighted the roll when the player changes
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
            if ((changePlayerLog.NewState == GameState.WaitingForRoll) || (changePlayerLog.NewState == GameState.WaitingForNext))
            {
                await SetRandomTileToGold(changePlayerLog.NewRandomGoldTiles);
            }

            if (changePlayerLog.NewState != changePlayerLog.OldState)
            {
                await UpdateUiForState(changePlayerLog.NewState);
            }
        }

        public void CompleteRedo()
        {
            if (UndoRedoTcs != null)
            {
                UndoRedoTcs.SetResult(true);
            }
        }

        public void CompleteUndo()
        {
            if (UndoRedoTcs != null)
            {
                UndoRedoTcs.SetResult(true);
            }
        }

        public RandomBoardSettings CurrentRandomBoard()
        {
            return _gameView.RandomBoardSettings;
        }

        /// <summary>
        ///     this is where we do the work to synchronize a roll across devices.
        ///
        ///     General Algorythm:
        ///
        ///     1. Store the rolls in PlayerData.GameData (they are rolls for the game)
        ///     2. When called, see if we've hit the terminating conditions
        ///         a) everybody has rolled
        ///         b) there are no ties
        ///     3. if we need more rolls
        ///         a) check to see if TheHuman (e.g. player on this machine) needs to roll
        ///         b) if so, wait for a roll
        ///         c) update GameData
        ///         d) log results
        ///
        /// </summary>
        /// <param name="logEntry"></param>
        /// <returns>true if everybody has rolled and their are no ties</returns>
        public async Task<bool> DetermineRollOrder(RollOrderLog logEntry)
        {
            //
            // need to update the state first because data binding is used to show/hide roll UI and it is driven off of
            // GameState.  the end of this function changes the state to GameState.WaitingForRollForOrder as well

            Contract.Assert(logEntry.NewState == GameState.WaitingForRollForOrder);

            PlayerModel sentBy = PlayerNameToPlayer(logEntry.SentBy, MainPageModel.AllPlayers);

            Contract.Assert(sentBy != null);
            RollModel pickedRoll = sentBy.GameData.SyncronizedPlayerRolls.AddRolls(logEntry.Rolls);
            Contract.Assert(pickedRoll != null);
            Contract.Assert(pickedRoll.DiceOne > 0 && pickedRoll.DiceOne < 7);
            Contract.Assert(pickedRoll.DiceTwo > 0 && pickedRoll.DiceTwo < 7);

            //
            //  look at all the rolls and see if the current player needs to roll again
            foreach (var p in MainPageModel.PlayingPlayers)
            {
                if (p == TheHuman) continue; // don't compare yourself to yourself
                if (p.GameData.SyncronizedPlayerRolls.CurrentRoll.DiceOne == -1) continue; //hasn't rolled yet
                if (p.GameData.SyncronizedPlayerRolls.CompareTo(TheHuman.GameData.SyncronizedPlayerRolls) == 0)
                {
                    if (p.GameData.SyncronizedPlayerRolls.RollValues.Count == TheHuman.GameData.SyncronizedPlayerRolls.RollValues.Count)
                    {
                        //
                        //  you are tied, but need to rollagain
                        string s = "Tie Roll. Roll again!";
                        await StaticHelpers.ShowErrorText(s, "Catan");
                        await _rollControl.Reset();                     
                    }

                    return false;
                }
            }

            //
            //  go through PlayingPlayers and ask if any are tied and if any need to roll
            //

            int count = MainPageModel.PlayingPlayers.Count;
            bool tie = false;
            for (int i = 0; i < count; i++)
            {
                var p1 = MainPageModel.PlayingPlayers[i];
                for (int j = i; j < count; j++)
                {
                    var p2 = MainPageModel.PlayingPlayers[j];
                    if (p2 == p1) continue;
                    if (p2.GameData.SyncronizedPlayerRolls.CompareTo(p1.GameData.SyncronizedPlayerRolls) == 0)
                    {
                        //
                        //  there is a tie.  keep going
                        tie = true;
                        break;
                    }
                }
                i++;
            }

            bool allPlayersRolled = true;
            foreach (var p in MainPageModel.PlayingPlayers)
            {
                if (p.GameData.SyncronizedPlayerRolls.RollValues.Count == 0)
                {
                    allPlayersRolled = false;
                    break;
                }
            }

            if (allPlayersRolled && !tie)
            {
                var newList = new List<PlayerModel>(MainPageModel.PlayingPlayers);
                newList.Sort((x, y) => x.GameData.SyncronizedPlayerRolls.CompareTo(y.GameData.SyncronizedPlayerRolls));
                for (int i = 0; i < newList.Count; i++)
                {
                    MainPageModel.PlayingPlayers[i] = newList[i];
                }

                //
                //  because all players are sharing the rolls, we can implicitly set the CurrentPlayer w/o sharing that we have done so.
                CurrentPlayer = MainPageModel.PlayingPlayers[0];

                //
                //
                if (CurrentGameState == GameState.WaitingForRollForOrder)
                {
                    return true;
                }
                else
                {
                    this.TraceMessage($"didn't call WaitingForRollOrderToWaitingForStart for {logEntry}");
                }
                // else eat it...only need one of these
            }

            return false;
        }

        /// <summary>
        ///     We store Rolls as RollModels which have both the GoldTiles (set before Roll) and the Roll (set after Roll).
        ///     Once you are assigned a Roll, that is your roll.  you can undo it, but when Redo (or even Next) happens,
        ///     you get the same Gold Tiles and the same value of the roll.
        /// </summary>
        /// <returns>the rollState object to use for this turn</returns>
        public RollState GetNextRollState()
        {
            var rollState = Log.RollLog.PopUndoneRoll();
            if (rollState == null)
            {
                rollState = new RollState()
                {
                    GoldTiles = this.GetRandomGoldTiles(),
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

        public async Task HideRollsInPublicUi()
        {
            await _rollControl.Reset();

            MainPageModel.PlayingPlayers.ForEach((p) =>
            {
                p.GameData.SyncronizedPlayerRolls.CurrentRoll.DiceOne = 0;
                p.GameData.SyncronizedPlayerRolls.CurrentRoll.DiceTwo = 0;
                p.GameData.RollOrientation = TileOrientation.FaceDown;
            }); // I hate this hack but I couldn't figure out how to do it with DataBinding
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

        /// <summary>
        ///     Checks to see if it is a service game.  if it is, post a message to the service.
        ///     if not, process the message immediately
        /// </summary>
        /// <param name="logHeader"></param>
        /// <param name="normal"></param>
        /// <returns>False if the IGameController.Do() should be executed locally </returns>
        public async Task<bool> PostMessage(LogHeader logHeader, ActionType msgType)
        {
            if (MainPageModel.ServiceGameInfo == null) return false; // can be null druing testing scenarios

            CatanMessage message = new CatanMessage()
            {
                Data = logHeader,
                From = TheHuman.PlayerName,
                ActionType = msgType,
                DataTypeName = logHeader.GetType().FullName

            };
            //  var tcs = new TaskCompletionSource<object>();
            //    MessageCompletionDictionary.Add(logHeader.LogId, tcs);
            
            
                await MainPageModel.CatanService.BroadcastMessage(MainPageModel.ServiceGameInfo.Id, message);
            
            //if (MainPageModel.Settings.IsLocalGame)
            //{
            //    await ProcessMessage(message);
            //}

            // await tcs.Task;

            return (!MainPageModel.Settings.IsLocalGame);
        }

        public DevCardType PurchaseNextDevCard()
        {
            return _gameView.CurrentGame.GetNextDevCard();
        }

        /// <summary>
        ///     this is the second phase of doing a roll -- put it on the stack with possibly Gold Tiles only
        ///
        /// </summary>
        /// <param name="rollState"></param>
        /// <returns></returns>
        public async Task PushRollState(RollState rollState)
        {
            Contract.Assert(rollState != null);
            Contract.Assert(rollState.GoldTiles != null);
            await Log.RollLog.PushStateNoRoll(rollState);
        }

        public async Task<bool> RedoAsync()
        {
            LogHeader logHeader = Log.PeekUndo;
            if (logHeader == null) return false;
            UndoRedoTcs = new TaskCompletionSource<bool>();

            CatanMessage message = new CatanMessage()
            {
                Data = logHeader,
                From = TheHuman.PlayerName,
                ActionType = ActionType.Redo,
                DataTypeName = logHeader.GetType().FullName
            };


            await MainPageModel.CatanService.BroadcastMessage(MainPageModel.ServiceGameInfo.Id, message);


            //if (MainPageModel.Settings.IsLocalGame)
            //{
            //    ILogController logController = logHeader as ILogController;
            //    await logController.Redo(this);
            //}


            return await UndoRedoTcs.Task;
        }

        public void ResetAllBuildings()
        {
            _gameView.AllBuildings.ForEach((building) => building.Reset()); // turn off pips on the local machine
        }

        public async Task ResetRandomGoldTiles()
        {
            await _gameView.ResetRandomGoldTiles();
        }

        public Task ResetRollControl()
        {
            return _rollControl.Reset();
        }

        //
        //  find all the tiles with building for this roll where the onwer == Player
        public (TradeResources Granted, TradeResources Baroned) ResourcesForRoll(PlayerModel player, int roll)
        {
            TradeResources tr = new TradeResources();
            TradeResources baron = new TradeResources();
            foreach (BuildingCtrl building in _gameView.CurrentGame.HexPanel.Buildings)
            {
                if (building == null) continue;
                if (building.Owner != player) continue;
                foreach (var kvp in building.BuildingToTileDictionary)
                {
                    if (kvp.Value.Number == roll)
                    {
                        if (kvp.Value.HasBaron == false)
                        {
                            tr.Add(kvp.Value.ResourceType, building.ScoreValue);
                        }
                        else
                        {
                            baron.Add(kvp.Value.ResourceType, building.ScoreValue);
                        }
                    }
                }
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

        public Task SetRoadState(UpdateRoadLog updateRoadModel)
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

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Need to clean up any UI actions  -- e.g. if the GameStarter clicks on "Roll to See who goes first", then that will cause
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
        /// <summary>
        ///     starting back as early as possible -- load MainPageModel from disk and recreate all the players.
        /// </summary>
        /// <returns></returns>
        public async Task InitializeMainPageModel()
        {
            _gameView.Reset();
            _gameView.SetCallbacks(this, this);
            await LoadMainPageModel();
            UpdateGridLocations();
            _progress.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            _progress.IsActive = false;

            await _rollControl.Reset();
            CurrentPlayer = TheHuman;

        }

        /// <summary>
        ///     Not a lot to do when Start happens.  Just get ready for the board to get set and players to be added.
        ///     We do keep track of who created the game as they are the ones that have to click "Start" to stop the addition
        ///     of new players.
        /// </summary>
        /// <param name="logHeader"></param>
        /// <returns></returns>
        public Task JoinOrCreateGame(NewGameLog logHeader)
        {
            if (MainPageModel.IsGameStarted) return Task.CompletedTask;

            MainPageModel.GameStartedBy = FindPlayerByName(MainPageModel.AllPlayers, logHeader.CreatedBy);
            _gameView.CurrentGame = _gameView.Games[logHeader.GameIndex];
            MainPageModel.IsGameStarted = true;
            return Task.CompletedTask;
                                 
        }
       

        public void StopHighlightingTiles()
        {
            GameContainer.AllTiles.ForEach((tile) => tile.StopHighlightingTile());
        }

        public Task TellServiceGameStarted()
        {
            if (MainPageModel.CatanService == null) return Task.CompletedTask;
            this.TraceMessage("you took this out.  put it back or delete it.");
            // await MainPageModel.CatanService.StartGame(MainPageModel.ServiceGameInfo);
            return Task.CompletedTask;
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
        public Task UndoAddPlayer(AddPlayerLog playerLogHeader)
        {
            var player = NameToPlayer(playerLogHeader.SentBy);
            Contract.Assert(player != null, "Player Can't Be Null");
            MainPageModel.PlayingPlayers.Remove(player);
            return Task.CompletedTask;
        }

        public async Task<bool> UndoAsync()
        {
            LogHeader logHeader = Log.PeekAction;
            if (logHeader == null || logHeader.CanUndo == false) return false;
            UndoRedoTcs = new TaskCompletionSource<bool>();
            CatanMessage message = new CatanMessage()
            {
                Data = logHeader,
                From = TheHuman.PlayerName,
                ActionType = ActionType.Undo,
                DataTypeName = logHeader.GetType().FullName
            };

            await MainPageModel.CatanService.BroadcastMessage(MainPageModel.ServiceGameInfo.Id, message);


            if (MainPageModel.Settings.IsLocalGame)
            {
                ILogController logController = logHeader as ILogController;
                await logController.Undo(this);
            }


            return await UndoRedoTcs.Task;
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

        public Task UndoSetRoadState(UpdateRoadLog updateRoadModel)
        {
            RoadCtrl road = GetRoad(updateRoadModel.RoadIndex);
            Contract.Assert(road != null);
            var player = NameToPlayer(updateRoadModel.SentBy);
            Contract.Assert(player != null);

            UpdateRoadState(player, road, updateRoadModel.NewRoadState, updateRoadModel.OldRoadState, updateRoadModel.OldRaceTracking);

            return Task.CompletedTask;
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
            var newState = updateBuildingLog.OldBuildingState;
            if (updateBuildingLog.OldBuildingState == BuildingState.Pips || updateBuildingLog.OldBuildingState == BuildingState.Build) newState = BuildingState.None;
            await building.UpdateBuildingState(player, updateBuildingLog.NewBuildingState, newState);
            if (updateBuildingLog.NewBuildingState == BuildingState.City)
            {
                player.GameData.Resources.UnspentEntitlements.Add(Entitlement.City);
            }
            else if (updateBuildingLog.NewBuildingState == BuildingState.Settlement)
            {
                player.GameData.Resources.UnspentEntitlements.Add(Entitlement.Settlement);
            }
            else
            {
                Contract.Assert(false, "should be city or settlement");
            }

            if (updateBuildingLog.OldState == GameState.AllocateResourceReverse)
            {
                TradeResources tr = new TradeResources();
                foreach (var kvp in building.BuildingToTileDictionary)
                {
                    tr.Add(kvp.Value.ResourceType, -1);
                }
                CurrentPlayer.GameData.Resources.GrantResources(tr);
            }
        }

        public async Task UpdateBuilding(UpdateBuildingLog updateBuildingLog)
        {
            BuildingCtrl building = GetBuilding(updateBuildingLog.BuildingIndex);
            Contract.Assert(building != null);
            PlayerModel player = NameToPlayer(updateBuildingLog.SentBy);
            Contract.Assert(player != null);
            Entitlement entitlement = Entitlement.Undefined;
            if (updateBuildingLog.NewBuildingState == BuildingState.City) entitlement = Entitlement.City;
            if (updateBuildingLog.NewBuildingState == BuildingState.Settlement) entitlement = Entitlement.Settlement;
            Contract.Assert(entitlement != Entitlement.Undefined);
            player.GameData.Resources.ConsumeEntitlement(entitlement);
            await building.UpdateBuildingState(player, updateBuildingLog.OldBuildingState, updateBuildingLog.NewBuildingState);
            if (building.BuildingState != BuildingState.Pips && building.BuildingState != BuildingState.None) // but NOT if if is transitioning to the Pips state - only happens from the Menu "Show Highest Pip Count"
            {
                await HideAllPipEllipses();
                _showPipGroupIndex = 0;
            }
            BuildingState oldState = updateBuildingLog.OldBuildingState;
            if (CurrentGameState == GameState.AllocateResourceReverse)
            {
                if (building.BuildingState == BuildingState.Settlement && (oldState == BuildingState.None || oldState == BuildingState.Pips || oldState == BuildingState.Build))
                {
                    TradeResources tr = new TradeResources();
                    foreach (var kvp in building.BuildingToTileDictionary)
                    {
                        tr.Add(kvp.Value.ResourceType, 1);
                    }
                    CurrentPlayer.GameData.Resources.GrantResources(tr);
                    // MainPageModel.GameResources += tr;
                    // this.TraceMessage($"{CurrentPlayer.PlayerName} Granted: {CurrentPlayer.GameData.Resources.TotalResources}");
                }
                else if ((building.BuildingState == BuildingState.None) && (oldState == BuildingState.Settlement))
                {
                    //
                    //  user did an undo
                    TradeResources tr = new TradeResources();
                    foreach (var kvp in building.BuildingToTileDictionary)
                    {
                        tr.Add(kvp.Value.ResourceType, -1);
                    }
                    CurrentPlayer.GameData.Resources.GrantResources(tr);
                    //  MainPageModel.GameResources += tr;
                }
            }

            //
            //  NOTE:  these have to be called in this order so that the undo works correctly
            //  await AddLogEntry(CurrentPlayer, GameStateFromOldLog, CatanAction.UpdateBuildingState, true, logType, building.Index, new LogBuildingUpdate(_gameView.CurrentGame.Index, null, building, oldState, building.BuildingState));
            UpdateTileBuildingOwner(player, building, building.BuildingState, oldState);
            CalculateAndSetLongestRoad();
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

        public void SetSpyInfo(string sentBy, bool spyOn)
        {
            this.SpyVisible = true;
            this.TurnedSpyOn = sentBy;
        }
    }
}
