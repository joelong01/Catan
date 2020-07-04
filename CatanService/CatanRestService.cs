﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.ServiceModel.Channels;
using System.Text.Json;
using System.Threading.Tasks;

using Catan.Proxy;

using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Catan10
{
    internal class CatanRestService 
    {
        #region Properties + Fields

        private static Assembly CurrentAssembly { get; } = Assembly.GetExecutingAssembly();

        private CatanProxy Proxy { get; } = new CatanProxy();
        public int UnprocessedMessages { get; set; }

        #endregion Properties + Fields

        #region Methods

        /// <summary>
        ///     The first time through we ask to Delete the game -- this happens iff we are the creator of the game.
        ///     We should get back 3 messages the first time -- delete, create, join
        ///
        ///     after the first time, we don't delete the game for obvious reasons...
        /// </summary>
        /// <param name="playerName"></param>
        private async void Monitor(GameInfo gameInfo, string playerName)
        {
            bool delete = true;
            while (true)
            {
                List<CatanMessage> messages;
                try
                {
                    messages = await Proxy.Monitor(gameInfo, playerName, delete);
                    delete = false;
                }
                catch (Exception e)
                {
                    this.TraceMessage($"{e}");
                    return;
                }
                foreach (var message in messages)
                {
                    LogHeader logHeader;
                    string json = message.Data.ToString();
                    CatanServiceMessage serviceMessage = null;
                    if (message.MessageType == MessageType.BroadcastMessage || message.MessageType == MessageType.PrivateMessage)
                    {
                        Type type = CurrentAssembly.GetType(message.DataTypeName);
                        if (type == null) throw new ArgumentException("Unknown type!");
                        logHeader = JsonSerializer.Deserialize(json, type, CatanSignalRClient.GetJsonOptions()) as LogHeader;
                        message.Data = logHeader;
                    }
                    else
                    {
                        serviceMessage = CatanSignalRClient.Deserialize<CatanServiceMessage>(json);
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
                            OnGameDeleted?.Invoke(serviceMessage.GameInfo.Id, message.From);
                            OnGameLeft?.Invoke(null, "");
                            break;

                        case MessageType.JoinGame:
                            OnGameJoined?.Invoke(serviceMessage.GameInfo, serviceMessage.PlayerName);
                            break;

                        default:
                            break;
                    }
                }
            }
        }

        #endregion Methods

        #region Delegates  + Events + Enums

        public event BroadcastMessageReceivedHandler OnBroadcastMessageReceived;

        public event GameLifeTimeHandler OnGameCreated;

        public event DeleteGameHandler OnGameDeleted;

        public event GameLifeTimeHandler OnGameJoined;
        public event GameLifeTimeHandler OnGameLeft;

        public event PrivateMessageReceivedHandler OnPrivateMessage;

        #endregion Delegates  + Events + Enums

        public void SendingMessage()
        {
            UnprocessedMessages++;
        }

        public void RecievedMessage(CatanMessage message)
        {
            if (message.From == MainPage.Current.TheHuman.PlayerName)
            {
                UnprocessedMessages--;
            }
        }
        
        public Task SendBroadcastMessage(Guid gameId, CatanMessage message)
        {
            
            SendingMessage();
            return Proxy.BroadcastMessage(gameId, message);
        }

        public Task CreateGame(GameInfo gameInfo)
        {
            return Proxy.CreateGame(gameInfo);
        }

        public Task DeleteGame(Guid id, string by)
        {
            return Proxy.DeleteGame(id, by);
        }

        public async Task<List<GameInfo>> GetAllGames()
        {
            var games = await Proxy.GetGames();
            if (games == null)
            {
                games = new List<GameInfo>();
            }
            return games;
        }

        public Task Initialize(string hostName)
        {
            Proxy.HostName = "http://" + hostName;
            Proxy.Timeout = TimeSpan.FromHours(12);
            return Task.CompletedTask;
        }

        public Task<GameInfo> JoinGame(GameInfo gameInfo, string playerName)
        {
            return Proxy.JoinGame(gameInfo, playerName);
        }

        public Task<List<string>> LeaveGame(GameInfo gameInfo, string playerName)
        {
            return Proxy.LeaveGame(gameInfo, playerName);
        }

        public Task SendPrivateMessage(Guid id, CatanMessage message)
        {
            return Proxy.BroadcastMessage(id, message);
        }

        public async Task StartConnection(GameInfo gameInfo, string playerName)
        {
            Contract.Assert(OnBroadcastMessageReceived != null);
            if (OnBroadcastMessageReceived != null)
            {
                try
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Monitor(gameInfo, playerName);
                    });
                }
                catch (Exception e)
                {
                    this.TraceMessage(e.ToString());
                    await MainPage.Current.ShowErrorMessage($"Error Monitoring Catan Rest Service.", "Catan REST Service StartConnection", e.ToString());
                }
            }
        }

        public Task<bool> KeepAlive()
        {
            return Proxy.KeepAlive();
        }

        public Task<List<string>> GetAllPlayerNames(Guid gameId)
        {
            throw new NotImplementedException();
        }

        public Task Reset()
        {
            throw new NotImplementedException();
        }

        public Task SendPrivateMessage(string to, CatanMessage message)
        {
            throw new NotImplementedException();
        }
    }
}