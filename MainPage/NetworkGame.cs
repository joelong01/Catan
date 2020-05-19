﻿using Catan.Proxy;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{

    public sealed partial class MainPage : Page
    {
        private static Assembly CurrentAssembly { get; } = Assembly.GetExecutingAssembly();
        Dictionary<string, GameInfo> KnownGames = new Dictionary<string, GameInfo>();
        
        /// <summary>
        ///     Monitor a game until you autojoin one - when a game ends, we should call this again
        /// </summary>
        private async void MonitorCatanGames()
        {

            List<GameInfo> games = await MainPageModel.ServiceData.Proxy.MonitorGames();
            if (games==null)
            {
                this.TraceMessage("Null games in MonitorGames!");
            }
            foreach (var game in games)
            {
                if (KnownGames.TryGetValue(game.Id, out GameInfo gameInfo)) continue;
                KnownGames[game.Id] = gameInfo;

                if (CurrentGameState == GameState.WaitingForNewGame && game.RequestAutoJoin && game.Name.Contains("Test"))
                {
                    
                    while (TheHuman == null)
                    {
                        await PickDefaultUser();
                    }
                    if (TheHuman.PlayerName != gameInfo.Creator)
                    {
                        this.MainPageModel.ServiceData.GameInfo = gameInfo;
                        await StartGameLog.StartGame(this, gameInfo.Creator, 0, true);
                        await AddPlayerLog.AddPlayer(this, TheHuman);
                        StartMonitoring();
                    }
                }
            }


        }
        private async void OnNewNetworkGame(object sender, RoutedEventArgs e)
        {
            await PickDefaultUser();
            if (TheHuman == null)
            {
                await PickDefaultUser();
            };
            if (TheHuman == null)
            {
                return;
            }

            var proxy = MainPageModel.ServiceData.Proxy;

            var existingGames = await proxy.GetGames();
            ServiceGameDlg dlg = new ServiceGameDlg(TheHuman, SavedAppState.AllPlayers, existingGames)
            {
                HostName = proxy.HostName
            };
            // this.TraceMessage($"Human={TheHuman}");
            await dlg.ShowAsync();
            if (dlg.IsCanceled) return;

            if (dlg.SelectedGame == null)
            {
                dlg.ErrorMessage = "Pick a game. Stop messing around Dodgy!";
                return;
            }


            MainPageModel.ServiceData.GameInfo = dlg.SelectedGame;
            MainPageModel.GameStartedBy = NameToPlayer(dlg.SelectedGame.Creator);

            this.TraceMessage($"Game: {dlg.SelectedGame.Name}");

            //
            // start a new game


            var startGameModel = await StartGameLog.StartGame(this, TheHuman.PlayerName, 0, true);
            Contract.Assert(startGameModel != null);

            //
            //  add the player
            var addPlayerLog = await AddPlayerLog.AddPlayer(this, TheHuman);
            Contract.Assert(addPlayerLog != null);




            StartMonitoring();

        }

        private async void OnStartDefaultNetworkGame(object sender, RoutedEventArgs e)
        {
            if (TheHuman == null)
            {
                await PickDefaultUser();
            }
            if (TheHuman == null) return;


            MainPageModel.PlayingPlayers.Clear();
            MainPageModel.Log = new NewLog();

            string gameName = "OnStartDefaultNetworkGame";



            CurrentPlayer = TheHuman;
            bool gameExists = false;
            GameInfo gameInfo = null;
            //
            //  delete alls games
            List<GameInfo> games = await Proxy.GetGames();
            foreach (var game in games)
            {
                if (game.Name == gameName)
                {
                    gameInfo = game;
                    // game exists
                    gameExists = true;
                    break;
                }
            };

            if (!gameExists)
            {

                // create a new game
                gameInfo = new GameInfo() { Id = Guid.NewGuid().ToString(), Name = gameName, Creator = CurrentPlayer.PlayerName };
                games = await Proxy.CreateGame(gameInfo);
                Contract.Assert(games != null);

            }

            MainPageModel.ServiceData.GameInfo = gameInfo;
            //
            //  start the game
            await StartGameLog.StartGame(this, MainPageModel.ServiceData.GameInfo.Creator, 0, true);

            //
            //  add players

            await Proxy.JoinGame(gameInfo.Id, TheHuman.PlayerName);
            await AddPlayerLog.AddPlayer(this, TheHuman);



            StartMonitoring();
        }

        private async void OnDeleteAllGames(object sender, RoutedEventArgs e)
        {
            List<GameInfo> games = await Proxy.GetGames();
            games.ForEach(async (game) =>
            {
                var s = await Proxy.DeleteGame(game.Id);
                if (s == null)
                {
                    var ErrorMessage = CatanProxy.Serialize(Proxy.LastError, true);
                    this.TraceMessage(ErrorMessage);
                }
            });
        }
        private PlayerModel FindPlayerByName(ICollection<PlayerModel> playerList, string playerName)
        {
            foreach (var player in playerList)
            {
                if (player.PlayerName == playerName)
                {
                    return player;
                }
            }
            return null;
        }


        private Task ReplayGame(GameInfo game, string playerName)
        {
            var Proxy = MainPageModel.ServiceData.Proxy;
            Contract.Assert(Proxy != null);
            return Task.CompletedTask;
            //   var messages = await Proxy.
        }



        private async void StartMonitoring()
        {


            var proxy = MainPageModel.ServiceData.Proxy;
            var gameId = MainPageModel.ServiceData.GameInfo.Id;

            var players = await proxy.GetPlayers(gameId);
            Contract.Assert(players.Contains(TheHuman.PlayerName));

            while (true)
            {
                List<CatanMessage> messages;
                try
                {
                    messages = await proxy.Monitor(gameId, TheHuman.PlayerName);
                }
                catch (Exception e)
                {
                    this.TraceMessage($"{e}");
                    return;
                }
                foreach (var message in messages)
                {
                    Type type = CurrentAssembly.GetType(message.TypeName);
                    if (type == null) throw new ArgumentException("Unknown type!");
                    LogHeader logHeader = JsonSerializer.Deserialize(message.Data.ToString(), type, CatanProxy.GetJsonOptions()) as LogHeader;
                    message.Data = logHeader;
                    MainPageModel.Log.RecordMessage(message);
                    Contract.Assert(logHeader != null, "All messages must have a LogEntry as their Data object!");



                    ILogController logController = logHeader as ILogController;
                    Contract.Assert(logController != null, "every LogEntry is a LogController!");
                    switch (logHeader.LogType)
                    {
                        case LogType.Normal:
                            if (logHeader.LocallyCreated == false) // Not created by the current machine
                            {
                                await MainPageModel.Log.PushAction(logHeader);
                            }
                            await logController.Do(this, logHeader);
                            break;
                        case LogType.Undo:
                            if (logHeader.LocallyCreated == false)
                            {
                                await MainPageModel.Log.Undo(message);
                            }
                            break;
                        case LogType.Replay:

                            await MainPageModel.Log.Redo(message);
                            break;
                        case LogType.DoNotLog:
                        case LogType.DoNotUndo:
                        default:
                            throw new InvalidDataException("These Logtypes shouldn't be set in a service game");
                    }

                }

            }

        }

    }
}
