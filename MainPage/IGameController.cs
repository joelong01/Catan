using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Threading.Tasks;

using Catan.Proxy;

using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class MainPage : Page, ILog, IGameController
    {
        private TaskCompletionSource<bool> ActionTcs { get; set; }
        private TaskCompletionSource<bool> UndoRedoTcs { get; set; }
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

        public bool IsServiceGame => MainPageModel.IsServiceGame;
        public NewLog Log => MainPageModel.Log;

        public List<int> NextRandomGoldTiles
        {
            get
            {
                int playerRoll = TotalRolls / MainPageModel.PlayingPlayers.Count;  // integer divide - drops remainder
                if (playerRoll == CurrentPlayer.GameData.GoldRolls.Count)
                {
                    var newRandomGoldTiles = GetRandomGoldTiles();
                    CurrentPlayer.GameData.GoldRolls.Add(newRandomGoldTiles);
                    return newRandomGoldTiles;
                }
                else
                {
                    Contract.Assert(CurrentPlayer.GameData.GoldRolls.Count > playerRoll);
                    //
                    //  we've already picked the tiles for this roll -- use them
                    return CurrentPlayer.GameData.GoldRolls[playerRoll];
                }
            }
        }

        public List<PlayerModel> PlayingPlayers => new List<PlayerModel>(MainPageModel.PlayingPlayers);
        public CatanProxy Proxy => MainPageModel.Proxy;

       

        private int PlayerNameToIndex(ICollection<PlayerModel> players, string playerName)
        {
            int index = 0;
            foreach (var player in players)
            {
                if (player.PlayerName == playerName) return index;
                index++;
            };
            return -1;
        }

        private async Task UpdateUiForState(GameState currentState)
        {
            switch (currentState)
            {
                case GameState.Uninitialized:
                    break;

                case GameState.WaitingForNewGame:
                    break;

                case GameState.WaitingForStart:
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
                        await SynchronizedRollLog.StartSyncronizedRoll(this, PickingBoardToWaitingForRollOrder.GetRollModelList());
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

                case GameState.Targeted:
                    break;

                case GameState.LostToCardsLikeMonopoly:
                    break;

                case GameState.DoneSupplemental:
                    break;

                case GameState.LostCardsToSeven:
                    break;

                case GameState.MissedOpportunity:
                    break;

                case GameState.GamePicked:
                    break;

                case GameState.MustMoveBaron:
                    break;

                case GameState.Unknown:
                    break;

                default:
                    break;
            }
        }

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
        public async Task AddPlayer(AddPlayerLog playerLogHeader)
        {
            Contract.Assert(CurrentGameState == GameState.WaitingForPlayers);

            var playerToAdd = NameToPlayer(playerLogHeader.PlayerToAdd);
            Contract.Assert(playerToAdd != null);
            if (MainPageModel.PlayingPlayers.Contains(playerToAdd) == false)
            {
                playerToAdd.GameData.OnCardsLost += OnPlayerLostCards;
                AddPlayerMenu(playerToAdd);
                //
                //  need to give the players some data about the game
                playerToAdd.GameData.MaxCities = _gameView.CurrentGame.MaxCities;
                playerToAdd.GameData.MaxRoads = _gameView.CurrentGame.MaxRoads;
                playerToAdd.GameData.MaxSettlements = _gameView.CurrentGame.MaxSettlements;
                playerToAdd.GameData.MaxShips = _gameView.CurrentGame.MaxShips;

                MainPageModel.PlayingPlayers.Add(playerToAdd);
            }

            if (CurrentPlayer != MainPageModel.GameStartedBy)
            {
                await ChangePlayerLog.SetCurrentPlayer(this, MainPageModel.GameStartedBy, CurrentGameState);
            }
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

        public RandomBoardSettings GetRandomBoard()
        {
            return _gameView.GetRandomBoard();
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
        public async Task<bool> PostMessage(LogHeader logHeader, CatanMessageType msgType)
        {
            CatanMessage message = new CatanMessage()
            {
                Data = logHeader,
                Origin = TheHuman.PlayerName,
                CatanMessageType = msgType
            };
            if (MainPageModel.IsServiceGame)
            {
                bool ret = await MainPageModel.Proxy.PostLogMessage(MainPageModel.GameInfo.Id, message);
                //  this.TraceMessage($"Sending {message}");
                if (!ret)
                {
                    await StaticHelpers.ShowErrorText($"Failed to Post Message to service.{Environment.NewLine}Error: {MainPageModel.Proxy.LastErrorString}");
                }
            }
            else
            {
                await ProcessMessage(message);
            }

            return MainPageModel.IsServiceGame;
        }

        public async Task<bool> RedoAsync()
        {
            LogHeader logHeader = Log.PeekUndo;
            if (logHeader == null) return false;
            UndoRedoTcs = new TaskCompletionSource<bool>();
            if (MainPageModel.IsServiceGame)
            {
                CatanMessage message = new CatanMessage()
                {
                    Data = logHeader,
                    Origin = TheHuman.PlayerName,
                    CatanMessageType = CatanMessageType.Redo
                };

                await MainPageModel.Proxy.PostLogMessage(MainPageModel.GameInfo.Id, message);
            }
            else
            {
                //
                // not a service game -- do the actual undo

                ILogController logController = logHeader as ILogController;
                await logController.Redo(this);
            }

            return await UndoRedoTcs.Task;
        }

        public void ResetAllBuildings()
        {
            _gameView.AllBuildings.ForEach((building) => building.Reset()); // turn off pips on the local machine
        }

        public void ShowRollsInPublicUi()
        {
            MainPageModel.PlayingPlayers.ForEach((p) =>
            {
                p.GameData.RollOrientation = TileOrientation.FaceUp;
            });
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

        /// <summary>
        ///     Not a lot to do when Start happens.  Just get ready for the board to get set and players to be added.
        ///     We do keep track of who created the game as they are the ones that have to click "Start" to stop the addition
        ///     of new players.
        /// </summary>
        /// <param name="logHeader"></param>
        /// <returns></returns>
        public async Task StartGame(StartGameLog logHeader)
        {
            if (Log.PeekAction?.LogId == logHeader.LogId) return; // this happens on the machine that starts the game.

            //
            //  the issue here is that we Log StartGame, Add Player, then Monitor under normal circumstances
            //  So when the second player does the same thing, they get all the log records for the game,
            //  including the StartGame and AddPlayers.  so you need to be careful to start the game only once -- which often
            //  isn't the locally started one.  if you don't, it will look like the players added before you connected aren't
            //  in the game because this StartGame resets PlayingPlayers
            //
            if (CurrentGameState != GameState.WaitingForNewGame) return;

            ResetDataForNewGame();
            MainPageModel.PlayingPlayers.Clear();
            MainPageModel.IsServiceGame = true;
            MainPageModel.GameStartedBy = FindPlayerByName(MainPageModel.AllPlayers, logHeader.SentBy);
            Contract.Assert(MainPageModel.GameStartedBy != null);
            _gameView.CurrentGame = _gameView.Games[logHeader.GameIndex];

            await AddPlayerLog.AddPlayer(this, TheHuman);
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
        /// <returns></returns>
        public async Task SynchronizedRoll(SynchronizedRollLog logEntry)
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
                    await _rollControl.Reset();
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
                    await WaitingForRollOrderToWaitingForStart.PostLog(this);
                }
                else
                {
                    this.TraceMessage($"didn't call WaitingForRollOrderToWaitingForStart for {logEntry}");
                }
                // else eat it...only need one of these
            }
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
            if (MainPageModel.IsServiceGame)
            {
                CatanMessage message = new CatanMessage()
                {
                    Data = logHeader,
                    Origin = TheHuman.PlayerName,
                    CatanMessageType = CatanMessageType.Undo
                };

                bool ret = await MainPageModel.Proxy.PostLogMessage(MainPageModel.GameInfo.Id, message);
                if (!ret) return false; // this is very bad! :(
            }
            else
            {
                //
                // not a service game -- do the actual undo

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

                logHeader.HighlightedTiles.ForEach((idx) => GameContainer.AllTiles[idx].HighlightTile(CurrentPlayer.BackgroundBrush));
            }
        }

        public async Task UndoSetRandomBoard(RandomBoardLog logHeader)
        {
            if (logHeader.PreviousRandomBoard == null) return;

            await _gameView.SetRandomCatanBoard(true, logHeader.PreviousRandomBoard);
            UpdateBoardMeasurements();
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

        public async Task StartGame()
        {
            await Proxy.StartGame(MainPageModel.GameInfo);
        }
    }
}