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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Catan10
{


    public class CatanSignalRClient : IDisposable, ICatanService
    {
        #region Delegates + Fields + Events + Enums

        public delegate void AckHandler(string fromPlayer, Guid messageId);

        public event AckHandler OnAck;

        public event BroadcastMessageReceivedHandler OnBroadcastMessageReceived;

        public event GameLifeTimeHandler OnGameCreated;

        public event DeleteGameHandler OnGameDeleted;

        public event GameLifeTimeHandler OnGameJoined;

        public event GameLifeTimeHandler OnGameLeft;

        public event PrivateMessageReceivedHandler OnPrivateMessage;

        private delegate void AllGamesReceivedHandler(List<GameInfo> games);

        private delegate void AllPlayersReceivedHandler(List<string> playerNames);

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
                        using (new FunctionTimer($"Ack + Process.  Id={message.MessageId} Type={message.DataTypeName}", true))
                        {
                            await HubConnection.SendAsync("Ack", MainPage.Current.MainPageModel.ServiceGameInfo.Id, MainPage.Current.TheHuman.PlayerName, message.From, message.MessageId);
                            message = ParseMessage(message);
                            OnBroadcastMessageReceived.Invoke(message);
                        }
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
                    this.TraceMessage($"{MainPage.Current.TheHuman.PlayerName} Sent Ack for {message}");
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

            LogHeader logHeader = JsonSerializer.Deserialize(msg.Data.ToString(), type, GetJsonOptions()) as LogHeader;
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
        static public T Deserialize<T>(string json)
        {         
            return JsonSerializer.Deserialize<T>(json, GetJsonOptions());
        }
        static public string Serialize<T>(T obj, bool indented = false)
        {
            if (obj == null) return null;
            return JsonSerializer.Serialize<T>(obj, GetJsonOptions(indented));
        }
        public static JsonSerializerOptions GetJsonOptions(bool indented = false)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = indented
            };
            options.Converters.Add(new JsonStringEnumConverter());
            options.Converters.Add(new PlayerModelConverter());
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

        public Task DisposeAsync()
        {
            return HubConnection.DisposeAsync();
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
                this.TraceMessage($"Error connection to SignalR.  ServiceUrl: {ServiceUrl}\nException:{e}");
                // await StaticHelpers.ShowErrorText($"Error connection to SignalR.  ServiceUrl: {ServiceUrl}\nException:{e}", "Catan");
                throw;
            }
        }

        public async Task<GameInfo> JoinGame(GameInfo gameInfo, string playerName)
        {
            if (gameInfo == null) throw new ArgumentException("GameInfo can't be null");
            if (String.IsNullOrEmpty(playerName)) throw new ArgumentException("PlayerName can't be null");

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

        public async Task Reset()
        {
            await HubConnection.InvokeAsync("Reset");
        }

        private TaskCompletionSource<object> BroadcastTcs = null;

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
            if (BroadcastTcs != null && !BroadcastTcs.Task.IsCompleted)
            {
                this.TraceMessage($"STARTED waiting for BroadcastTcs for id={message.MessageId} Type={message.DataTypeName}", 1);
                await BroadcastTcs.Task;
                this.TraceMessage($"FINISHED waiting for BroadcastTcs for id={message.MessageId} Type={message.DataTypeName}", 1);
            }

            BroadcastTcs = new TaskCompletionSource<object>();
            try
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
                //  Ack timeout
                // 
                int timeout = 5 * 1000;
                while (true)
                {
                    await EnsureConnection();

                    AckTracker ackTracker = new AckTracker()
                    {
                        PlayerNames = targets,
                        MessageId = message.MessageId,
                        Client = this,
                    };

                    //
                    //  call the hub
                    await HubConnection.InvokeAsync("BroadcastMessage", gameId, message);

                    //
                    //  this will return after timeout, or after we get acks from everything
                    bool succeeded = await ackTracker.WaitForAllAcks(timeout);
                    if (succeeded) break;
                    if (!succeeded)
                    {
                        //
                        //  got a timeout
                        string s = "";
                        targets.ForEach((p) => s += p + ", ");
                        s = s.Substring(0, s.Length - 1);
                        ContentDialog dlg = new ContentDialog()
                        {
                            Title = "Catan Networking Error",
                            Content = $"Timed out waiting for an Ack from {s}.\nTimeout={timeout}ms\nMessage={message.DataTypeName}\n\nHit Cancel to end the game.  For everybody.",
                            CloseButtonText = "Retry",
                            SecondaryButtonText = "Cancel"
                        };
                        try
                        {
                            while (VisualTreeHelper.GetOpenPopups(Window.Current).Count > 0)
                            {
                                this.TraceMessage("Wating for Dialogbox to close");
                                await Task.Delay(5000);
                            }
                            this.TraceMessage($"calling retry dialog for id={message.MessageId}");
                            var ret = await dlg.ShowAsync();
                            this.TraceMessage($"return from retry dialog for id={message.MessageId}");
                            if (ret == ContentDialogResult.Secondary)
                            {
                                await DeleteGame(gameId, "system");
                            }
                        }
                        catch
                        {
                            //
                            //  we have cases where we notifiy the user twice that we are waiting for a Retry .. this will retry w/o telling the user
                            this.TraceMessage($"exception thrown for rety notification for id ={message.MessageId}\nWaiting 3 seconds");
                            await Task.Delay(3000);
                        }
                        //
                        //  note: Targets gets modified by the AckTracker so it will only have the list of Players that need to send us an acc
                        //  
                        message.ActionType = ActionType.Retry;
                        s = "";
                        targets.ForEach((p) => s += p + ", ");
                        s = s.Substring(0, s.Length - 1);
                        this.TraceMessage($"need acks from {s}");
                        ackTracker.Cancel();
                        ackTracker = null;
                        if (targets.Count == 0)
                            break; // don't have anybody more to Ack, but for some reason the system didn't fire
                    }
                }
            }
            finally
            {
                BroadcastTcs.TrySetResult(null);

            }
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
                await MainPage.Current.ShowErrorMessage("Lost Connection to the Catan Service.  Click Ok and I'll try to connect.", "Catan", "");
                await connectionTCS.Task.TimeoutAfter(5000);
                connectionTCS = new TaskCompletionSource<object>();
            }
        }

        private async Task RegisterClient()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                // this will happily throw..

                //
                //  this has to be on the UI thread to access PlayerName
                await HubConnection.InvokeAsync("Register", MainPage.Current.TheHuman.PlayerName);
            });
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
                ret = await ackTracker.WaitForAllAcks(3000);
            } while (ret == false);
        }
    }

    public class PlayerModelConverter : JsonConverter<PlayerModel>
    {
        #region Methods

        public override PlayerModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert == typeof(PlayerModel))
            {
                string playerName = reader.GetString();
                if (MainPage.Current.MainPageModel.AllPlayers.Count > 0)
                {
                    var player = MainPage.Current.NameToPlayer(playerName);
                    if (player != null)
                    {
                        return player;
                    }
                }
                //
                //  we are probably loading MainPage Model from disk

                

                return MainPage.Current.NameToPlayer(playerName);
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, PlayerModel player, JsonSerializerOptions options)
        {
            writer.WriteStringValue(player.PlayerName);
        }

        #endregion Methods
    }
}