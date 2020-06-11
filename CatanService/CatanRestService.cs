using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

using Catan.Proxy;

using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Catan10
{
    internal class CatanRestService : ICatanService
    {
        #region Properties + Fields

        private static Assembly CurrentAssembly { get; } = Assembly.GetExecutingAssembly();

        private GameInfo GameInfo { get; set; }

        private CatanProxy Proxy { get; } = new CatanProxy();

        #endregion Properties + Fields



        #region Methods

        /// <summary>
        ///     The first time through we ask to Delete the game -- this happens iff we are the creator of the game.
        ///     We should get back 3 messages the first time -- delete, create, join
        ///
        ///     after the first time, we don't delete the game for obvious reasons...
        /// </summary>
        /// <param name="playerName"></param>
        private async void Monitor(string playerName)
        {
            bool delete = true;
            while (true)
            {
                List<CatanMessage> messages;
                try
                {
                    messages = await Proxy.Monitor(GameInfo, playerName, delete);
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

        public event CreateGameHandler OnGameCreated;

        public event DeleteGameHandler OnGameDeleted;

        public event JoinedGameHandler OnGameJoined;

        public event PrivateMessageReceivedHandler OnPrivateMessage;

        #endregion Delegates  + Events + Enums

        public Task BroadcastMessage(CatanMessage message)
        {
            return Proxy.BroadcastMessage(GameInfo.Id, message);
        }

        public Task CreateGame()
        {
            return Proxy.CreateGame(GameInfo);
        }

        public Task DeleteGame(GameInfo gameInfo, string by)
        {
            return Proxy.DeleteGame(gameInfo.Id, by);
        }

        public Task<List<GameInfo>> GetAllGames()
        {
            return Proxy.GetGames();
        }

        public Task Initialize(string hostName, GameInfo gameInfo)
        {
            Proxy.HostName = "http://" + hostName;
            GameInfo = gameInfo;
            return Task.CompletedTask;
        }

        public Task JoinGame(string playerName)
        {
            return Proxy.JoinGame(GameInfo, playerName);
        }

        public Task SendPrivateMessage(CatanMessage message)
        {
            return Proxy.BroadcastMessage(GameInfo.Id, message);
        }

        public async Task StartConnection(string playerName)
        {
            Contract.Assert(OnBroadcastMessageReceived != null);
            if (OnBroadcastMessageReceived != null)
            {
                try
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Monitor(playerName);
                    });
                }
                catch (Exception e)
                {
                    this.TraceMessage(e.ToString());
                    await StaticHelpers.ShowErrorText($"Error Monitoring Catan Rest Service.  Error:\n{e}", "Catan REST Service StartConnection");
                }
            }
        }
    }
}