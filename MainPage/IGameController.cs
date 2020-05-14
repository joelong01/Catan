using Catan.Proxy;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Text.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public interface IGameController
    {
        /// <summary>
        ///     The current player in the game
        /// </summary>
        PlayerModel CurrentPlayer { get; }

        Task UndoSetRandomBoard(RandomBoardLog logHeader);

        /// <summary>
        ///     Adds a player to the game.  if the Player is already in the game, return false.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        Task AddPlayer(AddPlayerLog playerLogHeader);
        Task UndoSetState(SetStateLog setStateLog);

        /// <summary>
        ///     Given a playerName, return the Model by looking up in the AllPlayers collection
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        PlayerModel NameToPlayer(string playerName);

        Task UndoAddPlayer(AddPlayerLog playerLogHeader);

        /// <summary>
        ///     The current state of the game
        /// </summary>
        GameState CurrentGameState { get; }
        CatanGames CatanGame { get; set; }

        List<int> CurrentRandomGoldTiles { get; }

        List<int> NextRandomGoldTiles { get; }
        List<int> HighlightedTiles { get; }

        Task StartGame(StartGameLog model);
        RandomBoardSettings GetRandomBoard();
        Task SetRandomBoard(RandomBoardLog randomBoard);
        RandomBoardSettings CurrentRandomBoard();
        Task SynchronizedRoll(SynchronizedRollLog log);
        Task ChangePlayer(ChangePlayerLog log);
        Task UndoChangePlayer(ChangePlayerLog log);
        Task SetState(SetStateLog log);
    }

    public sealed partial class MainPage : Page, ILog, IGameController
    {
        public CatanGames CatanGame { get; set; } = CatanGames.Regular;

        public GameState CurrentGameState
        {
            get
            {
                if (MainPageModel.Log == null) return GameState.WaitingForNewGame;

                return MainPageModel.Log.GameState;
            }
        }
        public CatanProxy Proxy => MainPageModel.ServiceData.Proxy;
        public SessionInfo SessionInfo => MainPageModel.ServiceData.SessionInfo;



        public List<int> CurrentRandomGoldTiles => _gameView.CurrentRandomGoldTiles;

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

        public RandomBoardSettings GetRandomBoard()
        {
            return _gameView.GetRandomBoard();
        }

        public PlayerModel NameToPlayer(string playerName)
        {

            foreach (var player in SavedAppState.AllPlayers)
            {
                if (player.PlayerName == playerName)
                {
                    return player;
                }
            }
            return null;
        }
        /// <summary>
        ///     Called when a Player is added to a Service game
        ///     we want to keep all games with identical state, so we have a race condition where each person joins a session, which creates a new game
        ///     and adds a user.  so each person would be the first user to join the game and order isn't served.
        ///     
        ///     to fix this, we go to the service and get the list of users and add it in the service order.
        ///     
        ///     this can also be called directly by the user, so we need to make sure that it works even if the AddPlayer() hasn't been logged
        ///     
        /// </summary>
        /// <param name="playerLogHeader"></param>
        /// <returns></returns>
        public async Task AddPlayer(AddPlayerLog playerLogHeader)
        {
            Contract.Assert(CurrentGameState == GameState.WaitingForPlayers);

            var proxy = MainPageModel.ServiceData.Proxy;
            Contract.Assert(proxy != null);

            var players = await proxy.GetPlayers(MainPageModel.ServiceData.SessionInfo.Id);
            Contract.Assert(players != null);
            if (!players.Contains(playerLogHeader.PlayerName))
            {
                players.Add(playerLogHeader.PlayerName);
            }

            List<PlayerModel> playingPlayers = new List<PlayerModel>();
            //
            //  we go through and figure out what the playing player list looks like
            players.ForEach((name) =>
            {

                var player = NameToPlayer(name);
                Contract.Assert(player != null, "Player Can't Be Null");
                if (MainPageModel.PlayingPlayers.Contains(player))
                {
                    playingPlayers.Add(player);
                }
                else
                {
                    player.Reset();
                    player.GameData.OnCardsLost += OnPlayerLostCards;
                    AddPlayerMenu(player);
                    //
                    //  need to give the players some data about the game
                    player.GameData.MaxCities = _gameView.CurrentGame.MaxCities;
                    player.GameData.MaxRoads = _gameView.CurrentGame.MaxRoads;
                    player.GameData.MaxSettlements = _gameView.CurrentGame.MaxSettlements;
                    player.GameData.MaxShips = _gameView.CurrentGame.MaxShips;

                    playingPlayers.Add(player);
                }
            });

            MainPageModel.PlayingPlayers.Clear();
            playingPlayers.ForEach((player) => MainPageModel.PlayingPlayers.Add(player));

            CurrentPlayer = MainPageModel.PlayingPlayers[0];
            await MainPageModel.Log.PushAction(playerLogHeader);
        }
        /// <summary>
        ///     we don't call the Log to push the undo action -- that is done by the log since the log
        ///     initiates all Undo
        /// </summary>
        /// <param name="playerLogHeader"></param>
        /// <returns></returns>
        public Task UndoAddPlayer(AddPlayerLog playerLogHeader)
        {

            var player = NameToPlayer(playerLogHeader.PlayerName);
            Contract.Assert(player != null, "Player Can't Be Null");
            MainPageModel.PlayingPlayers.Remove(player);
            return Task.CompletedTask;

        }

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
            await MainPageModel.Log.PushAction(randomBoard);
            UpdateBoardMeasurements();
        }

        public async Task UndoSetRandomBoard(RandomBoardLog logHeader)
        {

            if (logHeader.PreviousRandomBoard == null) return;

            await _gameView.SetRandomCatanBoard(true, logHeader.PreviousRandomBoard);
            UpdateBoardMeasurements();
            //
            //  DO NOT LOG UNDO
            //
        }

        /// <summary>
        ///     Not a lot to do when Start happens.  Just get ready for the board to get set and players to be added.
        ///     We do keep track of who created the game as they are the ones that have to click "Start" to stop the addition 
        ///     of new players.
        /// </summary>
        /// <param name="logHeader"></param>
        /// <returns></returns>
        public Task StartGame(StartGameLog logHeader)
        {

            MainPageModel.Log = new NewLog();

            ResetDataForNewGame();
            MainPageModel.PlayingPlayers.Clear();


            MainPageModel.IsServiceGame = true;
            MainPageModel.GameStartedBy = FindPlayerByName(SavedAppState.AllPlayers, logHeader.PlayerName);



            _gameView.CurrentGame = _gameView.Games[logHeader.GameIndex];


            //
            //   5/12/2020: You have to log so that the GameState is set correctly.  the function is protected at the top by looking at the state
            //              and not starting unless we need to start                            

            return MainPageModel.Log.PushAction(logHeader);


        }
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

        public async Task ChangePlayer(ChangePlayerLog log)
        {

            PlayerModel player = NameToPlayer(log.PlayerName);

            int idx = MainPageModel.PlayingPlayers.IndexOf(player);
            Contract.Assert(idx != -1, "The player needs to be playing!");

            idx += log.Move;
            int count = MainPageModel.PlayingPlayers.Count;
            if (idx >= count) idx -= count;
            if (idx < 0) idx += count;

            // this controller is the one spot where the CurrentPlayer is changed.  it should update all the bindings
            // the setter will update all the associated state changes that happen when the CurrentPlayer
            // changes

            CurrentPlayer = MainPageModel.PlayingPlayers[idx];

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
            if ((log.NewState == GameState.WaitingForRoll) || (log.NewState == GameState.WaitingForNext))
            {

                await SetRandomTileToGold(log.NewRandomGoldTiles);
            }

            await MainPageModel.Log.PushAction(log);
        }

        public async Task UndoChangePlayer(ChangePlayerLog logHeader)
        {

            CurrentPlayer = NameToPlayer(logHeader.PlayerName);

            if (logHeader.OldState == GameState.WaitingForNext)
            {
                if (logHeader.OldRandomGoldTiles != null)
                {
                    await SetRandomTileToGold(logHeader.OldRandomGoldTiles);
                }

                logHeader.HighlightedTiles.ForEach((idx) => GameContainer.AllTiles[idx].HighlightTile(CurrentPlayer.GameData.BackgroundBrush));

            }

        }

        public RandomBoardSettings CurrentRandomBoard()
        {
            return _gameView.RandomBoardSettings;
        }

        /// <summary>
        ///     Need to clean up any UI actions  -- e.g. if the GameStarter clicks on "Roll to See who goes first", then that will cause
        ///     messages to be Posted to the other clients.  They end up calling this function.  it needs to be exactly the same as if the
        ///     button was clicked on.
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public async Task SetState(SetStateLog log)
        {
            var stateLeaving = log.OldState;
            switch (stateLeaving)
            {
                case GameState.Uninitialized:
                    break;
                case GameState.WaitingForNewGame:
                    break;
                case GameState.WaitingForStart:
                    MainPageModel.PlayingPlayers.ForEach((p) =>
                    {
                        p.GameData.RollOrientation = TileOrientation.FaceDown;
                        p.GameData.SyncronizedPlayerRolls.DiceOne = 0;
                        p.GameData.SyncronizedPlayerRolls.DiceTwo = 0;

                    }); // I hate this hack but I couldn't figure out how to do it with DataBinding

                    break;
                case GameState.WaitingForPlayers:
                    break;
                case GameState.PickingBoard:

                    //
                    //  when leaving PickingBoard, WaitingForRollOrder is next
                    Contract.Assert(log.NewState == GameState.WaitingForRollForOrder);
                    await _rollControl.Reset();
                    MainPageModel.PlayingPlayers.ForEach((p) => p.GameData.RollOrientation = TileOrientation.FaceUp); // I hate this hack but I couldn't figure out how to do it with DataBinding
                    break;
                case GameState.WaitingForRollForOrder:
                    //
                    //  When leaving WaitingForRollOrder, next state is going to be AllocationResorucesForward -- get rid of the UI that shows all the player rolls


                    break;
                case GameState.AllocateResourceForward:

                    break;
                case GameState.AllocateResourceReverse:
                    break;
                case GameState.DoneResourceAllocation:
                    break;
                case GameState.WaitingForRoll:
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

            await MainPageModel.Log.PushAction(log);

        }
        //
        //  State in the game is stored at the top of the NewLog.ActionStack
        //  so "Undoing" state is just moving the record from the ActionStack
        //  to the Undo stack
        public Task UndoSetState(SetStateLog setStateLog)
        {
            throw new NotImplementedException();
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

            if (CurrentGameState != GameState.WaitingForRollForOrder)
            {
                await SetStateLog.SetState(this, GameState.WaitingForRollForOrder);
            }

            Contract.Assert(logEntry.NewState == GameState.WaitingForRollForOrder);

            PlayerModel player = PlayerNameToPlayer(logEntry.PlayerName);

            Contract.Assert(player != null);
            Contract.Assert(logEntry.DiceOne > 0 && logEntry.DiceOne < 7);
            Contract.Assert(logEntry.DiceTwo > 0 && logEntry.DiceTwo < 7);
            Contract.Assert(logEntry.RollCount == player.GameData.SyncronizedPlayerRolls.Rolls.Count);
            player.GameData.SyncronizedPlayerRolls.AddRoll(logEntry.DiceOne, logEntry.DiceTwo);


            //
            //  look at all the rolls and see if the current player needs to roll again
            foreach (var p in MainPageModel.PlayingPlayers)
            {
                if (p == TheHuman) continue; // don't compare yourself to yourself
                if (p.GameData.SyncronizedPlayerRolls.CompareTo(TheHuman.GameData.SyncronizedPlayerRolls) == 0)
                {
                    await _rollControl.Reset();
                    
                }
            }


            //
            //  this will eitehr be the logEntry passed in for another player or the one that we generated
            //  and filled out from this machine.
            //

            await MainPageModel.Log.PushAction(logEntry);


            //
            //  TODO how do we know to stop? - go through PlayingPlayers and ask if any are tied and if any need to roll
            //

            int count = MainPageModel.PlayingPlayers.Count;

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
                        return;
                    }
                }
                i++;
            }

            // done

            await SetStateLog.SetState(this, GameState.WaitingForStart);

        }
    }
}
