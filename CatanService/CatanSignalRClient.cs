using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        private delegate void AllGamesReceivedHandler(List<GameInfo> games);
        private event AllGamesReceivedHandler OnAllGamesReceived;
        private static Assembly CurrentAssembly { get; } = Assembly.GetExecutingAssembly();
        
        private HubConnection HubConnection { get; set; }

        private string ServiceUrl { get; set; } = "";

        #endregion Properties + Fields 

        #region Methods



        private async void OnToAllClients(string json)
        {
            if (OnBroadcastMessageReceived != null)
            {
                try
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        CatanMessage message = ParseMessage(json);
                        OnBroadcastMessageReceived.Invoke(message);
                    });
                }
                catch (Exception e)
                {
                    this.TraceMessage(e.ToString());
                }
            }
        }

        private async void OnToOneClient(string jsonMessage)
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
        #endregion Methods

        #region Constructors

        #endregion Constructors

        #region Delegates  + Events + Enums

        #endregion Delegates  + Events + Enums

        #region Delegates + Fields + Events + Enums

        public event BroadcastMessageReceivedHandler OnBroadcastMessageReceived;

        public event GameLifeTimeHandler OnGameCreated;

        public event DeleteGameHandler OnGameDeleted;

        public event GameLifeTimeHandler OnGameJoined;
        public event GameLifeTimeHandler OnGameLeft;

        public event PrivateMessageReceivedHandler OnPrivateMessage;

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public HubConnectionState ConnectionState => HubConnection.State;

        public int UnprocessedMessages { get; set; }

        #endregion Properties

        #region Constructors + Destructors

        public CatanSignalRClient()
        {
        }

        #endregion Constructors + Destructors

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

        public async Task CreateGame(GameInfo gameInfo)
        {
            Contract.Assert(!String.IsNullOrEmpty(gameInfo.Name));
            try
            {
               await HubConnection.InvokeAsync("CreateGame", gameInfo);
            }
            catch (Exception ex)
            {
                this.TraceMessage($"Exception! [Message={ex.Message}");
            }
        }

        public async Task DeleteGame(Guid id, string by)
        {
            try
            {
                await HubConnection.InvokeAsync("DeleteGame", id.ToString(), by);
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

        public async Task<List<GameInfo>> GetAllGames()
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            List<GameInfo> list = null;
            void CatanSignalRClient_OnAllGamesReceived(List<GameInfo> games)
            {
                list = games;
                this.OnAllGamesReceived -= CatanSignalRClient_OnAllGamesReceived;
                tcs.SetResult(null);

            }
            this.OnAllGamesReceived += CatanSignalRClient_OnAllGamesReceived;

            await HubConnection.InvokeAsync("GetAllGames");
            await tcs.Task;
            return list;


        }

        public async Task Initialize(string host)
        {
            try
            {
                ServiceUrl = "http://" + host + "/CatanHub";
                

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

                HubConnection.On("ToAllClients", (string message) => OnToAllClients(message));
                HubConnection.On("ToOneClient", (string message) => OnToOneClient(message));

                HubConnection.On("CreateGame", async (GameInfo gameInfo, string by) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        //this.TraceMessage($"CreateGame: {gameInfo}");
                        OnGameCreated?.Invoke(gameInfo, by);
                    });
                });
                HubConnection.On("DeleteGame", async (Guid id, string by) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                       // this.TraceMessage($"DeletedGame: [GameId={id}] [by={by}]");
                        OnGameDeleted?.Invoke(id, by);
                    });
                });
                HubConnection.On("JoinGame", (GameInfo gameInfo, string playerName) =>
                {
                   // this.TraceMessage($"JoinGame: [Info={gameInfo}] [By={playerName}]");
                    OnGameJoined?.Invoke(gameInfo, playerName);
                });
                HubConnection.On("LeaveGame", (GameInfo gameInfo, string playerName) =>
                {
                    //this.TraceMessage($"Leave: [Info={gameInfo}] [By={playerName}]");
                    OnGameLeft?.Invoke(gameInfo, playerName);
                });

                HubConnection.On("AllGames", (List<GameInfo> games) =>
                {
                    OnAllGamesReceived?.Invoke(games);
                });

                await HubConnection.StartAsync();
            }
            catch (Exception e)
            {
                await StaticHelpers.ShowErrorText($"Error connection to SignalR.  ServiceUrl: {ServiceUrl}\nException:{e}", "Catan");
            }
        }



        public async Task<GameInfo> JoinGame(GameInfo gameInfo, string playerName)
        {
            try
            {
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                GameInfo serviceGameInfo = null;
                void CatanSignalRClient_OnGameJoined(GameInfo info, string name)
                {
                    this.OnGameJoined -= CatanSignalRClient_OnGameJoined;
                    serviceGameInfo = info;
                    tcs.TrySetResult(null);                    

                };
                this.OnGameJoined += CatanSignalRClient_OnGameJoined;
                await HubConnection.InvokeAsync("JoinGame", gameInfo, playerName);
                await tcs.Task;
                return serviceGameInfo;
            }
            catch (Exception ex)
            {
                this.TraceMessage($"Exception! [Message={ex.Message}");

            }
            return null;
        }

      

        public async Task<bool> KeepAlive()
        {
            await Task.CompletedTask;
            return true;
        }

        public async Task<List<string>> LeaveGame(GameInfo gameInfo, string playerName)
        {
            try
            {               
                await HubConnection.InvokeAsync("LeaveGame", gameInfo, playerName);                
            }
            catch (Exception ex)
            {
                this.TraceMessage($"Exception! [Message={ex.Message}");
            }

            return null;
        }

        public async Task SendBroadcastMessage(Guid gameId, CatanMessage message)
        {
            try
            {
                string json = JsonSerializer.Serialize<object>(message, GetJsonOptions());
                await HubConnection.InvokeAsync("BroadcastMessage", gameId.ToString(), json);
            }
            catch (Exception ex)
            {
                this.TraceMessage($"Exception! [Message={ex.Message}");
            }
        }

        public async Task SendPrivateMessage(Guid id, CatanMessage message)
        {
            if (string.IsNullOrEmpty(message.To))
            {
                throw new ArgumentException("message", nameof(message));
            }

            try
            {
                string json = JsonSerializer.Serialize(message, GetJsonOptions());
                await HubConnection.InvokeAsync("SendPrivateMessage", message.To, json);
            }
            catch (Exception ex)
            {
                this.TraceMessage($"Exception! [Message={ex.Message}");
            }
        }

        public Task StartConnection(GameInfo info, string playerName)
        {
            return Task.CompletedTask;
        }
    }
}
