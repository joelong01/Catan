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
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;

namespace Catan10
{
    public class AckTracker
    {
        public Guid MessageId { get; set; }
        public List<string> PlayerNames { get; set; }
        private TaskCompletionSource<object> TCS = new TaskCompletionSource<object>();
        public async Task<bool> WaitForAllAcks(CatanSignalRClient client, int timeoutMs)
        {
            client.OnAck += Client_OnAck;
            try
            {
                this.TraceMessage($"Waiting for acks on message: {MessageId}");
                await TCS.Task.TimeoutAfter(timeoutMs);
                return true;
            }
            catch (TimeoutException)
            {
                return false;
            }
            catch(Exception)
            {
                throw;
            }
            finally
            {
                client.OnAck -= Client_OnAck;
            }
                        
        }

        private void Client_OnAck(string fromPlayer, Guid messageId)
        {
            if (messageId == this.MessageId)
            {
                this.TraceMessage($"Received Ack from {fromPlayer} for message {messageId}");
                PlayerNames.Remove(fromPlayer);
                if (PlayerNames.Count == 0)
                {
                    this.TraceMessage($"Received all acks for message {messageId}");
                    TCS.TrySetResult(null);
                }
            }
        }
    }

    public class CatanSignalRClient : IDisposable, ICatanService
    {
        
        #region Delegates + Fields + Events + Enums

        public event BroadcastMessageReceivedHandler OnBroadcastMessageReceived;

        public event GameLifeTimeHandler OnGameCreated;

        public event DeleteGameHandler OnGameDeleted;

        public event GameLifeTimeHandler OnGameJoined;

        public event GameLifeTimeHandler OnGameLeft;

        public event PrivateMessageReceivedHandler OnPrivateMessage;
        private delegate void AllGamesReceivedHandler(List<GameInfo> games);
        private delegate void AllPlayersReceivedHandler(List<string> playerNames);
        public delegate void AckHandler (string fromPlayer, Guid messageId);
        public event AckHandler OnAck;

        private event AllGamesReceivedHandler OnAllGamesReceived;
        private event AllPlayersReceivedHandler OnAllPlayersReceived;


        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public HubConnectionState ConnectionState => HubConnection.State;

        public int UnprocessedMessages { get; set; }
        private static Assembly CurrentAssembly { get; } = Assembly.GetExecutingAssembly();

        private HubConnection HubConnection { get; set; }

        private string ServiceUrl { get; set; } = "";
        #endregion Properties





        #region Methods

        private async void OnToAllClients(CatanMessage message)
        {
            if (OnBroadcastMessageReceived != null)
            {
                //
                //  todo: what is the Ack call back fails?
                this.TraceMessage($"Sending Ack for messageId: {message.MessageId}");
                
                try
                {

                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await HubConnection.SendAsync("Ack", MainPage.Current.MainPageModel.ServiceGameInfo.Id, MainPage.Current.TheHuman.PlayerName, message.From, message.MessageId);
                        message = ParseMessage(message);
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
                        CatanMessage message = JsonSerializer.Deserialize<CatanMessage>(jsonMessage);
                        message = ParseMessage(message);
                        OnPrivateMessage.Invoke(message);
                    });
                }
                catch (Exception e)
                {
                    this.TraceMessage(e.ToString());
                }
            }
        }

        private CatanMessage ParseMessage(CatanMessage msg)
        {
            
            Type type = CurrentAssembly.GetType(msg.DataTypeName);
            if (type == null) throw new ArgumentException("Unknown type!");
            
            LogHeader logHeader = JsonSerializer.Deserialize(msg.Data.ToString(), type, CatanProxy.GetJsonOptions()) as LogHeader;
            IMessageDeserializer deserializer = logHeader as IMessageDeserializer;
            if (deserializer != null)
            {
                logHeader = deserializer.Deserialize(msg.Data.ToString());
            }
            msg.Data = logHeader;
            return msg;
        }

        #endregion Methods







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

        public async Task<List<string>> GetAllPlayerNames(Guid gameId)
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            List<string> list = null;
            void OnPlayerNamesReceived(List<string> playerNames)
            {
                list = playerNames;
                this.OnAllPlayersReceived -= OnPlayerNamesReceived;
                tcs.SetResult(null);
            }
            this.OnAllPlayersReceived += OnPlayerNamesReceived;

            await HubConnection.InvokeAsync("GetPlayersInGame", gameId);
            await tcs.Task;
            return list;
        }

        public async Task Reset()
        {
            await HubConnection.InvokeAsync("Reset");
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

                if (Debugger.IsAttached)
                {
                    HubConnection.ServerTimeout = TimeSpan.FromMinutes(60);
                    HubConnection.HandshakeTimeout = TimeSpan.FromMinutes(5);
                }

                HubConnection.Reconnecting += error =>
                {
                    this.TraceMessage("Hub reconnecting!!");
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

                HubConnection.On("ToAllClients", (CatanMessage message) => OnToAllClients(message));
                HubConnection.On("ToOneClient", (string message) => OnToOneClient(message));
                HubConnection.On("OnAck", (string fromPlayer, Guid messageId) => OnAck?.Invoke(fromPlayer, messageId));

                HubConnection.On("CreateGame", async (GameInfo gameInfo, string by) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        OnGameCreated?.Invoke(gameInfo, by);
                    });
                });
                HubConnection.On("DeleteGame", async (Guid id, string by) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        OnGameDeleted?.Invoke(id, by);
                    });
                });
                HubConnection.On("JoinGame", (GameInfo gameInfo, string playerName) =>
                {

                    OnGameJoined?.Invoke(gameInfo, playerName);
                });
                HubConnection.On("LeaveGame", (GameInfo gameInfo, string playerName) =>
                {

                    OnGameLeft?.Invoke(gameInfo, playerName);
                });

                HubConnection.On("AllGames", (List<GameInfo> games) =>
                {
                    OnAllGamesReceived?.Invoke(games);
                });

                HubConnection.On("AllPlayers", (ICollection<string> playerNames) =>
                {
                    OnAllPlayersReceived?.Invoke(new List<string>(playerNames));
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
        /// <summary>
        ///     Send a message to all the clients.
        ///     
        ///     1. create a list of Ack's you expect
        ///     2. create a TCS to wait on with a timeout
        ///     3. send the broadcast,
        ///     5. wait for the Acks.
        ///     6. if timeout, send a message to the one client and wait for acks
        ///     
        /// </summary>
        /// <param name="gameId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendBroadcastMessage(Guid gameId, CatanMessage message)
        {
            //
            //  the Task we will wait on
                    
            List<string> targets = new List<string>();
           
            MainPage.Current.MainPageModel.PlayingPlayers.ForEach((p) => targets.Add(p.PlayerName));

            if (targets.Count == 0)
            {
                await HubConnection.InvokeAsync("BroadcastMessage", gameId, message);
                return;
            }
            AckTracker ackTracker = new AckTracker()
            {
                PlayerNames = targets,
                MessageId = message.MessageId
            };

            message.Data = JsonSerializer.Serialize<object>(message.Data, GetJsonOptions());
            await HubConnection.InvokeAsync("BroadcastMessage", gameId, message);
            int timeout = 30000;
            try
            {
                bool succeeded = await ackTracker.WaitForAllAcks(this, timeout);
                if (!succeeded)
                {
                    string s = "";
                    targets.ForEach((p) => s += p + ", ");
                    s = s.Substring(0, s.Length - 1);
                    await StaticHelpers.ShowErrorText($"Timed out waiting for an Ack from {s}.\n Message={CatanProxy.Serialize<CatanMessage>(message, true)}\n\nClick Ok to Retry.", "Catan");
                    foreach (var p in targets)
                    {
                        await SendDirectAcknowledgedMessage(p, gameId, message);
                    }
                }
                

            }
            catch (Exception e)
            {
                this.TraceMessage($"You need to deal with this exception... {e}");
            }

            

        }

        private Task SendDirectAcknowledgedMessage(string p, Guid guid,  CatanMessage message)
        {
            Debug.Assert(false);
            return Task.CompletedTask;
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