﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Catan.Proxy;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;
using System.Diagnostics.Contracts;

namespace Catan10
{
    class WebSocketClient : ICatanService
    {
        public event BroadcastMessageReceivedHandler OnBroadcastMessageReceived;
        public event CreateGameHandler OnGameCreated;
        public event DeleteGameHandler OnGameDeleted;
        public event PrivateMessageReceivedHandler OnPrivateMessage;
        public event JoinedGameHandler OnGameJoined;

        private static Assembly CurrentAssembly { get; } = Assembly.GetExecutingAssembly();
        private MessageWebSocket MessageWebSocket { get; set; }
        private DataWriter MessageWriter { get; set; }
        private Uri ServerUri { get; set; }
        private GameInfo GameInfo { get; set; }

        public bool WebSocketConnected { get; set; } = false;


        public async Task BroadcastMessage(CatanMessage message)
        {
            try
            {
                message.MessageType = MessageType.BroadcastMessage;
                MessageWriter = new DataWriter(MessageWebSocket.OutputStream);
                var json = CatanProxy.Serialize<CatanMessage>(message);
                MessageWriter.WriteString(json);
                await MessageWriter.StoreAsync();
            }
            catch (Exception e)
            {
                await StaticHelpers.ShowErrorText($"Unable to make WebSocketConnection.{Environment.NewLine}" + e.Message, "Catan");
            }


        }

        public async Task CreateGame()
        {

            Contract.Assert(!String.IsNullOrEmpty(GameInfo.Name));
            try
            {
                
                CatanMessage message = new CatanMessage()
                {
                    ActionType = ActionType.Normal,
                    Data = (object)GameInfo,
                    DataTypeName = typeof(GameInfo).FullName,
                    MessageType = MessageType.CreateGame
                };

                await BroadcastMessage(message);
            }
            catch (Exception ex)
            {
                this.TraceMessage($"Exception! [Message={ex.Message}");
            }
        }

        public async Task DeleteGame(GameInfo gameInfo, string by)
        {
            Contract.Assert(!String.IsNullOrEmpty(GameInfo.Name));
            try
            {               
                CatanMessage message = new CatanMessage()
                {
                    ActionType = ActionType.Normal,
                    Data = (object)GameInfo,
                    DataTypeName = typeof(GameInfo).FullName,
                    From = by,
                    MessageType = MessageType.DeleteGame
                };

                await BroadcastMessage(message);
            }
            catch (Exception ex)
            {
                this.TraceMessage($"Exception! [Message={ex.Message}");
            }
        }

        public Task<List<GameInfo>> GetAllGames()
        {
            throw new NotImplementedException();
        }

        public async Task Initialize(string hostName, GameInfo gameInfo)
        {
            try
            {
                GameInfo = gameInfo;
                ServerUri = new Uri($"ws://{hostName}/ws");
                MessageWebSocket = new MessageWebSocket();
                MessageWebSocket.Control.MessageType = SocketMessageType.Utf8;
                MessageWebSocket.MessageReceived += MessageReceived;
                MessageWebSocket.Closed += OnClosed;
            }
            catch (Exception e)
            {
                await StaticHelpers.ShowErrorText($"Unable to make WebSocketConnection.{Environment.NewLine}" + e.Message, "Catan");
            }
        }

        private void OnClosed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            WebSocketConnected = true;
            this.TraceMessage("closed");
        }

        public async Task JoinGame(string playerName)
        {
            Contract.Assert(!String.IsNullOrEmpty(GameInfo.Name));
            try
            {
                var json = CatanProxy.Serialize(GameInfo);
                CatanMessage message = new CatanMessage()
                {
                    ActionType = ActionType.Normal,
                    Data = (object)GameInfo,
                    DataTypeName = typeof(GameInfo).FullName,
                    From = playerName,
                    MessageType = MessageType.JoinGame
                };

                await BroadcastMessage(message);
            }
            catch (Exception ex)
            {
                this.TraceMessage($"Exception! [Message={ex.Message}");
            }
        }

        public async Task SendPrivateMessage(CatanMessage message)
        {
            try
            {
                message.MessageType = MessageType.PrivateMessage;                
                MessageWriter = new DataWriter(MessageWebSocket.OutputStream);
                var json = CatanProxy.Serialize<CatanMessage>(message);
                MessageWriter.WriteString(json);
                await MessageWriter.StoreAsync();
            }
            catch (Exception e)
            {
                await StaticHelpers.ShowErrorText($"Unable to make WebSocketConnection.{Environment.NewLine}" + e.Message, "Catan");
            }
        }

        public async Task StartConnection(string playerName)
        {
            await MessageWebSocket.ConnectAsync(ServerUri);
            MessageWriter = new DataWriter(MessageWebSocket.OutputStream);

            WsMessage message = new WsMessage() { MessageType = CatanWsMessageType.RegisterForGameNotifications };
            var json = CatanProxy.Serialize<WsMessage>(message);
            MessageWriter.WriteString(json);
            await MessageWriter.StoreAsync();
            WebSocketConnected = true;
        }

        private async Task Ack(CatanMessage message)
        {
            message.MessageType = MessageType.Ack;
            var json = CatanProxy.Serialize(message);
            MessageWriter.WriteString(json);
            await MessageWriter.StoreAsync();
        }

        private async void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {

                using (DataReader reader = args.GetDataReader())
                {
                    reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    CatanMessage message;
                    try
                    {
                        string messageJson = reader.ReadString(reader.UnconsumedBufferLength);
                        message = CatanProxy.Deserialize<CatanMessage>(messageJson);
                        await Ack(message);

                        LogHeader logHeader;
                        string json = message.Data.ToString();
                        CatanServiceMessage serviceMessage = null;
                        if (message.MessageType == MessageType.BroadcastMessage || message.MessageType == MessageType.PrivateMessage)
                        {
                            Type type = CurrentAssembly.GetType(message.DataTypeName);
                            if (type == null) throw new ArgumentException("Unknown type!");
                            logHeader = JsonSerializer.Deserialize(json, type, CatanProxy.GetJsonOptions()) as LogHeader;
                            message.Data = logHeader;
                        }
                        else
                        {
                            serviceMessage = CatanProxy.Deserialize<CatanServiceMessage>(json);
                        }

                        switch (message.MessageType)
                        {
                            case MessageType.BroadcastMessage:
                                OnBroadcastMessageReceived?.Invoke(message);
                                break;

                            case MessageType.PrivateMessage:
                                OnPrivateMessage?.Invoke(message);
                                break;

                            case MessageType.CreateGame:
                                OnGameCreated?.Invoke(serviceMessage.GameInfo, serviceMessage.PlayerName);
                                break;

                            case MessageType.DeleteGame:
                                OnGameDeleted?.Invoke(serviceMessage.GameInfo);
                                break;

                            case MessageType.JoinGame:
                                break;

                            default:
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        this.TraceMessage($"{e}");
                    }
                }
            });



        }

        public Task BroadcastMessage(Guid id, CatanMessage message)
        {
            throw new NotImplementedException();
        }

        public Task CreateGame(GameInfo gameInfo)
        {
            throw new NotImplementedException();
        }

        public Task DeleteGame(Guid id, string by)
        {
            throw new NotImplementedException();
        }

        public Task Initialize(string hostName)
        {
            throw new NotImplementedException();
        }

        public Task<GameInfo> JoinGame(GameInfo info, string playerName)
        {
            throw new NotImplementedException();
        }

        public Task SendPrivateMessage(Guid id, CatanMessage message)
        {
            throw new NotImplementedException();
        }

        public Task StartConnection(GameInfo info, string playerName)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> LeavGame(GameInfo gameInfo, string playerName)
        {
            throw new NotImplementedException();
        }
    }
}