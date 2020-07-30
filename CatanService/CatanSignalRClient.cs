using System;
using System.Collections.Concurrent;
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

namespace Catan10.CatanService
{
    public class CatanSignalRClient : IDisposable, ICatanService
    {
        #region Delegates + Fields + Events + Enums

        public delegate void AckHandler(CatanMessage message);

        public event AckHandler OnAck;

        public event BroadcastMessageReceivedHandler OnBroadcastMessageReceived;

        public event GameLifeTimeHandler OnGameCreated;

        public event DeleteGameHandler OnGameDeleted;

        public event GameLifeTimeHandler OnGameJoined;

        public event GameLifeTimeHandler OnGameLeft;

        public event GameLifeTimeHandler OnGameRejoined;

        public event PrivateMessageReceivedHandler OnPrivateMessage;

        private delegate void AllGamesReceivedHandler(List<GameInfo> games);

        private delegate void AllPlayersReceivedHandler(List<string> playerNames);

        private event AllGamesReceivedHandler OnAllGamesReceived;

        private event AllPlayersReceivedHandler OnAllPlayersReceived;

        private event PongHandler OnPong;

        private delegate void PongHandler();

        private ConcurrentQueue<CatanMessage> MessageQueue { get; } = new ConcurrentQueue<CatanMessage>();

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        private ICollection<CatanMessage> MessageLog = null;
        public HubConnectionState ConnectionState => HubConnection.State;

        public int UnprocessedMessages { get; set; }
        private static Assembly CurrentAssembly { get; } = Assembly.GetExecutingAssembly();

        private string Host { get; set; } = "";
        private HubConnection HubConnection { get; set; }

        #endregion Properties

        #region Methods

        public static CatanMessage ParseMessage(CatanMessage msg)
        {
            if (msg.Data == null) return msg;
            if (String.IsNullOrEmpty(msg.DataTypeName)) return msg;
            Type type = CurrentAssembly.GetType(msg.DataTypeName);
            if (type == null) throw new ArgumentException("Unknown type!");

            if (type == typeof(AckModel))
            {
                msg.Data = JsonSerializer.Deserialize(msg.Data.ToString(), typeof(AckModel), GetJsonOptions());
                return msg;
            }

            LogHeader logHeader = JsonSerializer.Deserialize(msg.Data.ToString(), type, GetJsonOptions()) as LogHeader;
            msg.Data = logHeader;
            return msg;
        }

        public async Task PostHubMessage(CatanMessage message)
        {
            if (message.Data != null && message.Data.GetType() != typeof(string))
            {
                //
                //  make it an object so we can get the whole message
                message.Data = JsonSerializer.Serialize<object>(message.Data, GetJsonOptions());
            }
            else
            {
                this.TraceMessage($"message.Data is a string for {message}");
            }

            await HubConnection.SendAsync("PostMessage", message);
        }

        private void OnToAllClients(CatanMessage message)
        {
            if (OnBroadcastMessageReceived != null)
            {
                try
                {
                    if (message.To == "*" || message.To == MainPage.Current.TheHuman.PlayerName)
                    {
                        message = ParseMessage(message);
                        MessageQueue.Enqueue(message);
                        ProcessQueueAsync();
                        // OnBroadcastMessageReceived.Invoke(message);
                    }
                }
                catch (Exception e)
                {
                    this.TraceMessage(e.ToString());
                }
            }
        }

        private void OnToOneClient(CatanMessage message)
        {
            if (OnPrivateMessage != null)
            {
                try
                {
                    message = ParseMessage(message);
                    OnPrivateMessage.Invoke(message);
                }
                catch (Exception e)
                {
                    this.TraceMessage(e.ToString());
                }
            }
        }

        /// <summary>
        ///     I'm being "clever" here and making this void.  this causes async await to continue without waiting
        ///     this works by "borrowing" the thread that either sends the message or recieves the message to draing the queu
        ///     of messages.
        /// </summary>
        private async void ProcessQueueAsync()
        {
            while (MessageQueue.TryDequeue(out CatanMessage message))
            {
                if (message.MessageDirection == MessageDirection.ClientToServer)
                {
                    await InternalSendBroadcastMessage(message);
                }
                else
                {
                    Contract.Assert(message.MessageDirection == MessageDirection.ServerToClient);
                    OnBroadcastMessageReceived.Invoke(message);
                }
            }
        }

        #endregion Methods

        #region Constructors + Destructors

        public CatanSignalRClient()
        {
        }

        #endregion Constructors + Destructors

        private DateTime _lastReconnected = new DateTime();

        private TaskCompletionSource<object> BroadcastTcs = null;

        private GameInfo GameInfo { get; set; }

        private string PlayerName { get; set; }

        static public T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, GetJsonOptions());
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

        static public string Serialize<T>(T obj, bool indented = false)
        {
            if (obj == null) return null;
            return JsonSerializer.Serialize<T>(obj, GetJsonOptions(indented));
        }

        public async Task CreateGame(GameInfo gameInfo)
        {
            Contract.Assert(!string.IsNullOrEmpty(gameInfo.Name));
            try
            {
                this.GameInfo = gameInfo;
                CatanMessage message = new CatanMessage()
                {
                    MessageType = MessageType.CreateGame,
                    ActionType = ActionType.Normal,
                    Data = null,
                    DataTypeName="",
                    From = gameInfo.Creator,
                    To="*",
                    GameInfo = gameInfo,
                };
                await PostHubMessage(message);
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
                CatanMessage message = new CatanMessage()
                {
                    MessageType = MessageType.DeleteGame,
                    ActionType = ActionType.Normal,
                    Data = null,
                    DataTypeName="",
                    From = by,
                    To="*",
                    GameInfo = gameInfo,
                };
                await PostHubMessage(message);
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

        public async Task<bool> DoPingPong()
        {
            var  watch = new Stopwatch();
            watch.Start();
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            this.OnPong += PongReceived;

            void PongReceived()
            {
                watch.Stop();
                this.TraceMessage($"pong recieved took: {watch.ElapsedMilliseconds}ms");
                this.OnPong -= PongReceived;
                tcs.TrySetResult(true);
            }
            this.TraceMessage("sending ping");
            await HubConnection.InvokeAsync("Ping");
            try
            {
                await tcs.Task.TimeoutAfter(500);
                return true;
            }
            catch (TimeoutException)
            {
                //
                //  timedout with a ping -- get a new HubConnection

                this.OnPong -= PongReceived;
                watch.Stop();
                this.TraceMessage($"pong timed out: {watch.ElapsedMilliseconds}ms");
                await this.Initialize(this.Host, MessageLog, this.PlayerName);
                await this.RejoinGame(this.GameInfo, this.PlayerName);
                return false;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
            }
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
            await EnsureConnection();

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

        public async Task Initialize(string host, ICollection<CatanMessage> messageLog, string theHumanName)
        {
            Contract.Assert(!String.IsNullOrEmpty(theHumanName));
            Contract.Assert(messageLog != null);
            Contract.Assert(!String.IsNullOrEmpty(host));

            MessageLog = messageLog;
            this.PlayerName = theHumanName;
            this.Host = host;
            try
            {
                UnsubscribeAll();
                string serviceUrl;
                if (host.Contains("192") || host.Contains("local"))
                {
                    serviceUrl = "http://" + host + "/CatanHub";
                }
                else
                {
                    serviceUrl = "https://" + host + "/CatanHub";
                }

                _lastReconnected = DateTime.Now;
                HubConnection = new HubConnectionBuilder().WithAutomaticReconnect().WithUrl(serviceUrl).ConfigureLogging((logging) =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Trace);
                }).Build();

                //
                //  7/23/2020:  these options made the whole thing fall apart.  no idea why.

                //.AddJsonProtocol(options =>
                // {
                //     options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                //     options.PayloadSerializerOptions.PropertyNameCaseInsensitive = true;
                //     options.PayloadSerializerOptions.IgnoreNullValues = true;
                //     options.PayloadSerializerOptions.Converters.Add(new PlayerModelConverter());
                //     options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());

                // })

                HubConnection.ServerTimeout = TimeSpan.FromMinutes(5);
                HubConnection.HandshakeTimeout = TimeSpan.FromSeconds(10);
                HubConnection.KeepAliveInterval = TimeSpan.FromSeconds(19);

                HubConnection.Reconnecting += async error =>
                {
                    this.TraceMessage("Hub reconnecting!!");
                    Debug.Assert(HubConnection.State == HubConnectionState.Reconnecting);
                    if (GameInfo != null) // this puts us back into the channel with the other players.
                    {
                        CatanMessage message = new CatanMessage()
                        {
                            MessageType = MessageType.JoinGame,
                            ActionType = ActionType.Normal,
                            Data = null,
                            DataTypeName="",
                            From = this.PlayerName,
                            To="*",
                            GameInfo = this.GameInfo,
                        };
                        await PostHubMessage(message);
                    }
                };
                HubConnection.Reconnected += async (connectionId) =>
                {
                    TimeSpan delta = DateTime.Now - _lastReconnected;
                    this.TraceMessage($"Reconnected.  new id: {connectionId}.  It has been {delta.TotalSeconds} seconds ");
                    await RegisterClient();
                    _lastReconnected = DateTime.Now;
                };

                HubConnection.Closed += async (error) =>
                {
                    this.TraceMessage($"HubConnection closed!  Error={error}");
                    await Task.Delay(new Random().Next(0, 5) * 1000);
                    await HubConnection.StartAsync();
                };

                HubConnection.On("ToAllClients", async (CatanMessage message) =>
                {
                    //
                    //  when we get the message, immediately send the ack -- don't switch threads to do so.
                    // this.TraceMessage($"recieved id {message.MessageId}");
                    var ack = AckModel.CreateMessage(message, PlayerName);
                    //  this.TraceMessage($"sent Ack: {((AckModel)ack.Data).AckedMessageId}");
                    await PostHubMessage(ack);
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MessageLog.Add(message);
                        OnToAllClients(message);
                    });
                });

                HubConnection.On("Pong", () =>
                {
                    OnPong?.Invoke();
                });

                HubConnection.On("ToOneClient", async (CatanMessage message) =>
                {
                    //
                    //  when we get the message, immediately send the ack -- don't switch threads to do so.
                    using (new FunctionTimer($"Ack + Process.  Id={message.MessageId} Type={message.DataTypeName}", true))
                    {
                        var ack = AckModel.CreateMessage(message, PlayerName);
                        await PostHubMessage(ack);
                    }

                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MessageLog.Add(message);
                        OnToOneClient(message);
                    });
                });
                HubConnection.On("OnAck", (CatanMessage message) =>
                {
                    message = ParseMessage(message);

                    //
                    //  tell everybody we got an ack
                    OnAck?.Invoke(message);

                    //
                    //  this does not give me an RPC_E_WRONG_THREAD...much be because all it does is add it to a collection.
                    MessageLog.Add(message);
                });

                HubConnection.On("CreateGame", async (CatanMessage message) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MessageLog.Add(message);
                        OnGameCreated?.Invoke(message.GameInfo, message.GameInfo.Creator);
                    });
                });
                HubConnection.On("DeleteGame", async (CatanMessage message) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MessageLog.Add(message);
                        OnGameDeleted?.Invoke(message.GameInfo, message.From);
                    });
                });
                HubConnection.On("JoinGame", async (CatanMessage message) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MessageLog.Add(message);

                        OnGameJoined?.Invoke(message.GameInfo, message.From);
                    });
                });
                HubConnection.On("RejoinGame", async (CatanMessage message) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        OnGameRejoined?.Invoke(message.GameInfo, message.From);
                    });
                });
                HubConnection.On("LeaveGame", async (CatanMessage message) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MessageLog.Add(message);
                        OnGameLeft?.Invoke(message.GameInfo, message.From);
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
                this.TraceMessage($"Error connection to SignalR.  ServiceUrl: {Host}\nException:{e}");
                // await StaticHelpers.ShowErrorText($"Error connection to SignalR.  ServiceUrl: {ServiceUrl}\nException:{e}", "Catan");
                throw;
            }
        }

        public async Task<GameInfo> JoinGame(GameInfo gameInfo, string playerName)
        {
            if (gameInfo == null) throw new ArgumentException("GameInfo can't be null");
            if (String.IsNullOrEmpty(playerName)) throw new ArgumentException("PlayerName can't be null");

            this.GameInfo = gameInfo;
            this.PlayerName = playerName;

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
                CatanMessage message = new CatanMessage()
                {
                    MessageType = MessageType.JoinGame,
                    ActionType = ActionType.Normal,
                    Data = null,
                    DataTypeName="",
                    From = playerName,
                    To="*",
                    GameInfo = gameInfo,
                };
                this.OnGameJoined += CatanSignalRClient_OnGameJoined;
                await PostHubMessage(message);
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
                CatanMessage message = new CatanMessage()
                {
                    MessageType = MessageType.LeaveGame,
                    ActionType = ActionType.Normal,
                    Data = null,
                    DataTypeName="",
                    From = playerName,
                    To="*",
                    GameInfo = gameInfo,
                };
                await HubConnection.InvokeAsync("PostMessage", message);
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

        public Task SendBroadcastMessage(CatanMessage message)
        {
            message.MessageDirection = MessageDirection.ClientToServer;
            MessageQueue.Enqueue(message);
            ProcessQueueAsync();
            return Task.CompletedTask;
        }

        public async Task SendPrivateMessage(string playerName, CatanMessage message)
        {
            if (string.IsNullOrEmpty(message.To))
            {
                throw new ArgumentException("message", nameof(message));
            }

            try
            {
                await PostHubMessage(message);
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
            await DoPingPong();

            if (HubConnection.State == HubConnectionState.Connected)
            {
                return;
            }

            TaskCompletionSource<object> connectionTCS = new TaskCompletionSource<object>();
            HubConnection.Reconnected += Reconnected;
            async Task Reconnected(string arg)
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    this.TraceMessage($"{MainPage.Current.TheHuman.PlayerName} reconnected");
                    connectionTCS.TrySetResult(null);
                    HubConnection.Reconnected -= Reconnected;
                });
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
        private async Task InternalSendBroadcastMessage(CatanMessage message)
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
                //  Ack timeout
                //
                int timeout = 5 * 1000;
                while (true)
                {
                    AckTracker ackTracker = new AckTracker()
                    {
                        PlayerNames = targets,
                        MessageId = message.MessageId,
                        Client = this,
                    };

                    //
                    //  call the hub
                    await PostHubMessage(message);

                    //
                    //  this will return after timeout, or after we get acks from everything
                    bool succeeded = await ackTracker.WaitForAllAcks(timeout);
                    if (succeeded) break;
                    if (!succeeded)
                    {
                        if (targets.Count == 0)
                        {
                            //
                            //  there is a race condition where you can timeout between the time you remove the player and setting the timeout...found it while debugging.  might not happen in practice.
                            break;
                        }

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
                                await DeleteGame(message.GameInfo, "system");
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

                        await EnsureConnection();
                    }
                }
            }
            finally
            {
                BroadcastTcs.TrySetResult(null);
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

        private async Task<GameInfo> RejoinGame(GameInfo gameInfo, string playerName)
        {
            if (gameInfo == null) throw new ArgumentException("GameInfo can't be null");
            if (String.IsNullOrEmpty(playerName)) throw new ArgumentException("PlayerName can't be null");

            this.GameInfo = gameInfo;

            try
            {
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                GameInfo serviceGameInfo = null;
                void CatanSignalRClient_OnGameRejoined(GameInfo info, string name)
                {
                    this.OnGameRejoined -= CatanSignalRClient_OnGameRejoined;
                    serviceGameInfo = info;
                    tcs.TrySetResult(null);
                };
                CatanMessage message = new CatanMessage()
                {
                    MessageType = MessageType.RejoinGame,
                    ActionType = ActionType.Normal,
                    Data = null,
                    DataTypeName="",
                    From = playerName,
                    To="*",
                    GameInfo = gameInfo,
                };
                this.OnGameRejoined += CatanSignalRClient_OnGameRejoined;
                await PostHubMessage(message);
                await tcs.Task;
                return serviceGameInfo;
            }
            catch (Exception ex)
            {
                this.TraceMessage($"Exception! [Message={ex.Message}");
            }
            return null;
        }

        private void UnsubscribeAll()
        {
            if (HubConnection == null) return;

            HubConnection.Remove("OnAck");
            HubConnection.Remove("AllGames");
            HubConnection.Remove("AllPlayers");
            HubConnection.Remove("AllMessages");
            HubConnection.Remove("CreateGame");
            HubConnection.Remove("DeleteGame");
            HubConnection.Remove("JoinGame");
            HubConnection.Remove("RejoinGame");
            HubConnection.Remove("LeaveGame");
            HubConnection.Remove("ToAllClients");
            HubConnection.Remove("ToOneClient");
            HubConnection.Remove("ServiceError");
            HubConnection.Remove("Pong");
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