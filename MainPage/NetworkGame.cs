﻿using Catan.Proxy;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{

    public sealed partial class MainPage : Page
    {
        private static Assembly CurrentAssembly { get; } = Assembly.GetExecutingAssembly();
        Dictionary<string, GameInfo> KnownGames = new Dictionary<string, GameInfo>();

        private MessageWebSocket MessageWebSocket { get; set; }
        private DataWriter MessageWriter { get; set; }

        private async Task WsConnect()
        {
            try
            {

                Uri server = new Uri("ws://192.168.1.128:5000/catan/game/monitor/ws");
                MessageWebSocket = new MessageWebSocket();
                MessageWebSocket.Control.MessageType = SocketMessageType.Utf8;
                MessageWebSocket.MessageReceived += MessageReceived;
                MessageWebSocket.Closed += OnClosed;
                await MessageWebSocket.ConnectAsync(server);
                MessageWriter = new DataWriter(MessageWebSocket.OutputStream);

                WsMessage message = new WsMessage() { MessageType = WebSocketMessage.RegisterForGameNotifications };
                var json = CatanProxy.Serialize<WsMessage>(message);
                MessageWriter.WriteString(json);
                await MessageWriter.StoreAsync();
                MainPageModel.WebSocketConnected = true;
            }
            catch(Exception e)
            {
                await StaticHelpers.ShowErrorText($"Unable to make WebSocketConnection.{Environment.NewLine}" + e.Message);
            }
        }
        private void OnClosed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            MainPageModel.WebSocketConnected = true;
            this.TraceMessage("closed");
        }

        private void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            var ignore = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {

                using (DataReader reader = args.GetDataReader())
                {
                    reader.UnicodeEncoding = UnicodeEncoding.Utf8;

                    try
                    {
                        string json = reader.ReadString(reader.UnconsumedBufferLength);
                        //
                        //  get our message, which now is only a WsGameMessage
                        WsMessage message = CatanProxy.Deserialize<WsMessage>(json);
                        var gameMessage = CatanProxy.Deserialize<WsGameMessage>(message.Data.ToString());
                        //
                        //  ack back to the service
                        var ack= new WsMessage() { MessageType = WebSocketMessage.Ack };
                        json = CatanProxy.Serialize<WsMessage>(ack);
                        MessageWriter.WriteString(json);
                        await MessageWriter.StoreAsync();

                        if (gameMessage.GameInfo.RequestAutoJoin && gameMessage.GameInfo.Name.Contains("OnStartDefault") && TheHuman != null &&
                            gameMessage.GameInfo.Creator != TheHuman.PlayerName && message.MessageType == WebSocketMessage.GameAdded && MainPageModel.AutoJoinGames)
                        {
                            await this.Reset();
                            await Proxy.JoinGame(gameMessage.GameInfo.Id, TheHuman.PlayerName);
                            this.MainPageModel.GameInfo = gameMessage.GameInfo;
                            await StartGameLog.StartGame(this, gameMessage.GameInfo.Creator, 0, true);
                            await AddPlayerLog.AddPlayer(this, TheHuman);
                            
                            StartMonitoring();
                        }

                        if (message.MessageType == WebSocketMessage.GameDeleted && gameMessage.GameInfo.Id == this.GameInfo.Id) // deleting the game I'm playing!
                        {
                            await this.Reset();
                        }

                    }
                    catch (Exception ex)
                    {
                        this.TraceMessage(ex.ToString());
                        this.TraceMessage(ex.Message);
                    }
                }
            });
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

            var proxy = MainPageModel.Proxy;

            var existingGames = await proxy.GetGames();
            ServiceGameDlg dlg = new ServiceGameDlg(TheHuman, MainPageModel.AllPlayers, existingGames)
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


            MainPageModel.GameInfo = dlg.SelectedGame;
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
                gameInfo = new GameInfo() { Id = Guid.NewGuid().ToString(), Name = gameName, Creator = CurrentPlayer.PlayerName, RequestAutoJoin=true };
                games = await Proxy.CreateGame(gameInfo);
                Contract.Assert(games != null);

            }

            MainPageModel.GameInfo = gameInfo;
            //
            //  start the game
            await StartGameLog.StartGame(this, MainPageModel.GameInfo.Creator, 0, true);

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
            var Proxy = MainPageModel.Proxy;
            Contract.Assert(Proxy != null);
            return Task.CompletedTask;
            //   var messages = await Proxy.
        }



        private async void StartMonitoring()
        {


            var proxy = MainPageModel.Proxy;
            var gameId = MainPageModel.GameInfo.Id;

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