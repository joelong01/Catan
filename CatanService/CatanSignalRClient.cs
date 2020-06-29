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
            catch (Exception)
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
                //  this.TraceMessage($"Received Ack from {fromPlayer} for message {messageId}");
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
        public delegate void AckHandler(string fromPlayer, Guid messageId);
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


                try
                {
                    if (message.To == "*" || message.To == MainPage.Current.TheHuman.PlayerName)
                    {
                        int msDelay =0;
                        await Task.Delay(msDelay); // force a timout
                        await HubConnection.SendAsync("Ack", MainPage.Current.MainPageModel.ServiceGameInfo.Id, MainPage.Current.TheHuman.PlayerName, message.From, message.MessageId);
                        message = ParseMessage(message);
                        OnBroadcastMessageReceived.Invoke(message);
                    }

                }
                catch (Exception e)
                {
                    this.TraceMessage(e.ToString());
                }
            }
        }

        private async void OnToOneClient(CatanMessage message)
        {
            if (OnPrivateMessage != null)
            {
                try
                {

                    await HubConnection.SendAsync("Ack", MainPage.Current.MainPageModel.ServiceGameInfo.Id, MainPage.Current.TheHuman.PlayerName, message.From, message.MessageId);

                    //
                    //  make sure we didn't process it - maybe the ACK was lost.

                    CatanMessage lastMessage = MainPage.Current.MainPageModel.Log.PeekMessageLog();
                    if (lastMessage == null || lastMessage.MessageId != message.MessageId)
                    {

                        //
                        //  not the last message.
                        message = ParseMessage(message);
                        OnPrivateMessage.Invoke(message);
                    }

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
            if (HubConnection.State != HubConnectionState.Connected)
            {
                this.TraceMessage("Why isn't the hub Connected?");
                await Task.Delay(2000);
                this.TraceMessage($"Waited 2 seconds = state now {HubConnection.State}");

            }

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            List<string> list = null;
            void OnPlayerNamesReceived(List<string> playerNames)
            {
                list = playerNames;
                this.OnAllPlayersReceived -= OnPlayerNamesReceived;
                tcs.TrySetResult(null);
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
                    HubConnection.ServerTimeout = TimeSpan.FromMinutes(30);
                    HubConnection.HandshakeTimeout = TimeSpan.FromMinutes(30);
                    HubConnection.KeepAliveInterval = TimeSpan.FromMinutes(15);
                }



                HubConnection.Reconnecting += error =>
                {
                    this.TraceMessage("Hub reconnecting!!");
                    Debug.Assert(HubConnection.State == HubConnectionState.Reconnecting);

                    // Notify users the connection was lost and the client is reconnecting.
                    // Start queuing or dropping messages.

                    return Task.CompletedTask;
                };
                HubConnection.Reconnected += async (connectionId) =>
                {
                    this.TraceMessage($"Reconnected.  new id: {connectionId}");
                    await RegisterClient();
                };

                HubConnection.Closed += async (error) =>
                {
                    this.TraceMessage($"HubConnection closed!  Error={error}");
                    await Task.Delay(new Random().Next(0, 5) * 1000);
                    await HubConnection.StartAsync();
                };

                HubConnection.On("ToAllClients", async (CatanMessage message) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        OnToAllClients(message);
                    });
                });

                HubConnection.On("ToOneClient", async (CatanMessage message) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        OnToOneClient(message);
                    });
                });
                HubConnection.On("OnAck", async (string fromPlayer, Guid messageId) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        OnAck?.Invoke(fromPlayer, messageId);
                    });
                });

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
                HubConnection.On("JoinGame", async (GameInfo gameInfo, string playerName) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        OnGameJoined?.Invoke(gameInfo, playerName);
                    });
                });
                HubConnection.On("LeaveGame", async (GameInfo gameInfo, string playerName) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        OnGameLeft?.Invoke(gameInfo, playerName);
                    });
                });

                HubConnection.On("AllGames", async (List<GameInfo> games) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        OnAllGamesReceived?.Invoke(games);
                    });
                });

                HubConnection.On("AllPlayers", async (ICollection<string> playerNames) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        OnAllPlayersReceived?.Invoke(new List<string>(playerNames));
                    });
                });

                await HubConnection.StartAsync();
            }
            catch (Exception e)
            {
                await StaticHelpers.ShowErrorText($"Error connection to SignalR.  ServiceUrl: {ServiceUrl}\nException:{e}", "Catan");
            }
        }

        private async Task RegisterClient()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                //
                //  this has to be on the UI thread to access PlayerName
                await HubConnection.InvokeAsync("Register", MainPage.Current.TheHuman.PlayerName);

            });
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
            //  this are the players the messag is going to -- we will wait for acks from all these
            List<string> targets = new List<string>();
            MainPage.Current.MainPageModel.PlayingPlayers.ForEach((p) => targets.Add(p.PlayerName));

            //
            //  No playing players...broadcast
            if (targets.Count == 0)
            {
                this.TraceMessage("No playing players.  not sending message");
                return;
            }



            //
            //  make it an object so we can get the whole message
            message.Data = JsonSerializer.Serialize<object>(message.Data, GetJsonOptions());



            //
            //  the app tracker is not getting Acks called on it

            int timeout = 3 * 1000; // for debuggin...one second timeout
            while (true)
            {

                await EnsureConnection();

                AckTracker ackTracker = new AckTracker()
                {
                    PlayerNames = targets,
                    MessageId = message.MessageId
                };

                //
                //  call the hub
                await HubConnection.InvokeAsync("BroadcastMessage", gameId, message);

                //
                //  this will return after timeout, or after we get acks from everything
                bool succeeded = await ackTracker.WaitForAllAcks(this, timeout);
                if (succeeded) break;
                if (!succeeded)
                {
                    //
                    //  got a timeout
                    string s = "";
                    targets.ForEach((p) => s += p + ", ");
                    s = s.Substring(0, s.Length - 1);
                    await StaticHelpers.ShowErrorText($"Timed out waiting for an Ack from {s}.\n Message={message.DataTypeName}\n\nRetry after ok.", "Catan");

                }
            }

        }

        private async Task EnsureConnection()
        {

            if (HubConnection.State == HubConnectionState.Connected) return;


            TaskCompletionSource<object> connectionTCS = new TaskCompletionSource<object>(); ;
            HubConnection.Reconnected += Reconnected;
            Task Reconnected(string arg)
            {
                connectionTCS.TrySetResult(null);
                HubConnection.Reconnected -= Reconnected;
                return Task.CompletedTask;
            }

            int n = 0;
            //
            //  make sure we are connected to the service
            while (HubConnection.State != HubConnectionState.Connected)
            {
                n++;
                await StaticHelpers.ShowErrorText("Lost Connection to the Catan Service.  Click Ok and I'll try to connect.", "Catan");
                await connectionTCS.Task.TimeoutAfter(5000);
                connectionTCS = new TaskCompletionSource<object>();
            }

        }

        private async Task SendDirectAcknowledgedMessage(string player, Guid gameId, CatanMessage message)
        {
            TaskCompletionSource<object> connectionTCS = null;
            HubConnection.Reconnected += Reconnected;
            Task Reconnected(string arg)
            {
                connectionTCS.TrySetResult(null);
                HubConnection.Reconnected -= Reconnected;
                return Task.CompletedTask;
            }


            int n = 0;
            //
            //  make sure we are connected to the service
            while (HubConnection.State != HubConnectionState.Connected)
            {
                n++;
                this.TraceMessage("Waiting to reconnect to hub...");
                connectionTCS = new TaskCompletionSource<object>();
                await connectionTCS.Task.TimeoutAfter(5000);

            }

            //
            //  now we should be connected again



            AckTracker ackTracker = new AckTracker()
            {
                PlayerNames = new List<string>() { player },
                MessageId = message.MessageId
            };
            bool ret = false;
            n = 0;
            do
            {
                this.TraceMessage($"count = {n++} player={player} message={message.DataTypeName} id={gameId}");
                await SendPrivateMessage(player, message);
                ret = await ackTracker.WaitForAllAcks(this, 1000);
            } while (ret == false);

        }


        public async Task SendPrivateMessage(string playerName, CatanMessage message)
        {
            if (string.IsNullOrEmpty(message.To))
            {
                throw new ArgumentException("message", nameof(message));
            }

            try
            {
                await HubConnection.InvokeAsync("SendPrivateMessage", playerName, message);
            }
            catch (Exception ex)
            {
                this.TraceMessage($"Exception! [Message={ex.Message}");
            }
        }

        public async Task StartConnection(GameInfo info, string playerName)
        {
            await RegisterClient();
        }
    }
}