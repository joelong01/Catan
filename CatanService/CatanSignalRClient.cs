using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Catan.Proxy;

using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

using Windows.UI.Core;

namespace Catan10
{
    public class CatanSignalRClient : IDisposable, ICatanService
    {
        #region Properties + Fields 

        private static Assembly CurrentAssembly { get; } = Assembly.GetExecutingAssembly();
        private GameInfo GameInfo { get; set; }
        private HubConnection HubConnection { get; set; }

        private string ServiceUrl { get; set; } = "";

        #endregion Properties + Fields 

        #region Methods

        private async void BroadcastMessageReceived(string from, string jsonMessage)
        {
            if (OnBroadcastMessageReceived != null)
            {
                try
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        var message = ParseMessage(jsonMessage);
                        OnBroadcastMessageReceived.Invoke(message);
                    });
                }
                catch (Exception e)
                {
                    this.TraceMessage(e.ToString());
                }
            }
        }

        private CatanMessage ParseMessage(string jsonMessage)
        {
            CatanMessage message = JsonSerializer.Deserialize(jsonMessage, typeof(CatanMessage), GetJsonOptions()) as CatanMessage;
            Type type = CurrentAssembly.GetType(message.DataTypeName);
            if (type == null) throw new ArgumentException("Unknown type!");
            string json = message.Data.ToString();
            LogHeader logHeader = JsonSerializer.Deserialize(json, type, CatanProxy.GetJsonOptions()) as LogHeader;
            IMessageDeserializer deserializer = logHeader as IMessageDeserializer;
            if (deserializer != null)
            {
                logHeader = deserializer.Deserialize(json);
            }
            message.Data = logHeader;
            return message;
        }

        private async void PrivateMessage(string jsonMessage)
        {
            if (OnPrivateMessage != null)
            {
                try
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        var message = ParseMessage(jsonMessage);
                        OnPrivateMessage.Invoke(message);
                    });
                }
                catch (Exception e)
                {
                    this.TraceMessage(e.ToString());
                }
            }
        }

        #endregion Methods

        #region Constructors

        #endregion Constructors

        #region Delegates  + Events + Enums

        #endregion Delegates  + Events + Enums

        public HubConnectionState ConnectionState => HubConnection.State;

        public CatanSignalRClient()
        {
        }

        public event BroadcastMessageReceivedHandler OnBroadcastMessageReceived;

        public event CreateGameHandler OnGameCreated;

        public event DeleteGameHandler OnGameDeleted;

        public event PrivateMessageReceivedHandler OnPrivateMessage;

        public static JsonSerializerOptions GetJsonOptions(bool indented = false)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = indented
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }

        public async Task BroadcastMessage(CatanMessage message)
        {
            try
            {
                string json = JsonSerializer.Serialize(message, GetJsonOptions());
                await HubConnection.InvokeAsync("BroadcastMessage", GameInfo.Name, message.From, json);
            }
            catch (Exception ex)
            {
                this.TraceMessage($"Exception! [Message={ex.Message}");
            }
        }

        public async Task CreateGame()
        {
            Contract.Assert(!String.IsNullOrEmpty(GameInfo.Name));
            try
            {
                var json = JsonSerializer.Serialize(GameInfo, GetJsonOptions());

                await HubConnection.InvokeAsync("CreateGame", GameInfo.Name, GameInfo.Started, json);
            }
            catch (Exception ex)
            {
                this.TraceMessage($"Exception! [Message={ex.Message}");
            }
        }

        public async Task DeleteGame(GameInfo gameInfo, string by)
        {
            try
            {
                await HubConnection.InvokeAsync("DeleteGame", gameInfo.Name);
            }
            catch (Exception ex)
            {
                this.TraceMessage($"Exception! [Message={ex.Message}");
            }
        }

        public void Dispose()
        {
            HubConnection.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public Task<List<GameInfo>> GetAllGames()
        {
            return null;
        }

        public Task Initialize(string host, GameInfo gameInfo)
        {
            ServiceUrl = host + "/catan";
            GameInfo = gameInfo;

            HubConnection = new HubConnectionBuilder().WithAutomaticReconnect().WithUrl(ServiceUrl).ConfigureLogging((logging) =>
           {
               logging.AddConsole();
               logging.SetMinimumLevel(LogLevel.Debug);
           }).Build();

            HubConnection.Reconnecting += error =>
            {
                Debug.Assert(HubConnection.State == HubConnectionState.Reconnecting);

                // Notify users the connection was lost and the client is reconnecting.
                // Start queuing or dropping messages.

                return Task.CompletedTask;
            };

            HubConnection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await HubConnection.StartAsync();
            };

            HubConnection.On("BroadcastMessage", (string name, string json) => BroadcastMessageReceived(name, json));
            HubConnection.On("SendPrivateMessage", (string message) => PrivateMessage(message));

            HubConnection.On("CreateGame", async (string gameName, string playerName, string jsonGameInfo) =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    this.TraceMessage($"CreateGame: [GameName={gameName}] [By={playerName}] [info={jsonGameInfo}]");
                    OnGameCreated?.Invoke(GameInfo, playerName);
                });
            });
            HubConnection.On("DeleteGame", async (string gameName) =>
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    this.TraceMessage($"DeletedGame: [GameName={gameName}]");
                    OnGameDeleted?.Invoke(GameInfo);
                });
            });
            HubConnection.On("JoinedGame", (string gameName, string playerName) =>
            {
                this.TraceMessage($"JoinGame: [GameName={gameName}] [By={playerName}]");
                this.TraceMessage($"{playerName} joined {gameName}");
            });

            return Task.CompletedTask;
        }

        public async Task JoinGame(string playerName)
        {
            try
            {
                await HubConnection.InvokeAsync("JoinGame", GameInfo.Name, playerName);
            }
            catch (Exception ex)
            {
                this.TraceMessage($"Exception! [Message={ex.Message}");
            }
        }

        public async Task SendPrivateMessage(CatanMessage message)
        {
            if (string.IsNullOrEmpty(message.To))
            {
                throw new ArgumentException("message", nameof(message));
            }

            try
            {
                string json = JsonSerializer.Serialize(message, GetJsonOptions());
                await HubConnection.InvokeAsync("PrivateMessage", message.To, json);
            }
            catch (Exception ex)
            {
                this.TraceMessage($"Exception! [Message={ex.Message}");
            }
        }

        public async Task StartConnection(string playerName)
        {
            try
            {
                await HubConnection.StartAsync();
            }
            catch (Exception ex)
            {
                this.TraceMessage(ex.Message);
                await StaticHelpers.ShowErrorText(ex.Message, "Connect to SignalR");
            }
        }
    }
}
