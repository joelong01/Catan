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

namespace Catan10.CatanService
{
    public class CatanSignalRClient : IDisposable, ICatanService
    {
        #region Delegates + Fields + Events + Enums

        public delegate void AckHandler (CatanMessage message);

        public event AckHandler OnAck;

        public event BroadcastMessageReceivedHandler OnBroadcastMessageReceived;

        public event GameLifeTimeHandler OnGameCreated;

        public event DeleteGameHandler OnGameDeleted;

        public event GameLifeTimeHandler OnGameJoined;

        public event GameLifeTimeHandler OnGameLeft;

        public event PrivateMessageReceivedHandler OnPrivateMessage;

        private delegate void AllGamesReceivedHandler (List<GameInfo> games);

        private delegate void AllPlayersReceivedHandler (List<string> playerNames);

        private event AllGamesReceivedHandler OnAllGamesReceived;

        private event AllPlayersReceivedHandler OnAllPlayersReceived;

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        private ICollection<CatanMessage> MessageLog = null;
        public HubConnectionState ConnectionState => HubConnection.State;

        public int UnprocessedMessages { get; set; }
        private static Assembly CurrentAssembly { get; } = Assembly.GetExecutingAssembly();

        private HubConnection HubConnection { get; set; }

        private string ServiceUrl { get; set; } = "";

        #endregion Properties

        #region Methods

        private void OnToAllClients (CatanMessage message)
        {
            if (OnBroadcastMessageReceived != null)
            {
                try
                {
                    if (message.To == "*" || message.To == MainPage.Current.TheHuman.PlayerName)
                    {

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

        private async Task PostHubMessage (CatanMessage message)
        {
            await EnsureConnection();
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

        private void OnToOneClient (CatanMessage message)
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

        private CatanMessage ParseMessage (CatanMessage msg)
        {
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

        #endregion Methods

        #region Constructors + Destructors

        public CatanSignalRClient ()
        {
        }

        #endregion Constructors + Destructors

        private DateTime _lastReconnected = new DateTime();

        private TaskCompletionSource<object> BroadcastTcs = null;

        private GameInfo GameInfo { get; set; }

        private string PlayerName { get; set; }

        static public T Deserialize<T> (string json)
        {
            return JsonSerializer.Deserialize<T>(json, GetJsonOptions());
        }

        public static JsonSerializerOptions GetJsonOptions (bool indented = false)
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

        static public string Serialize<T> (T obj, bool indented = false)
        {
            if (obj == null) return null;
            return JsonSerializer.Serialize<T>(obj, GetJsonOptions(indented));
        }

        public async Task CreateGame (GameInfo gameInfo)
        {
            Contract.Assert(!string.IsNullOrEmpty(gameInfo.Name));
            try
            {
                this.GameInfo = gameInfo;
                this.PlayerName = gameInfo.Creator;
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

        public async Task DeleteGame (GameInfo gameInfo, string by)
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

        public void Dispose ()
        {
            HubConnection.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public Task DisposeAsync ()
        {
            return HubConnection.DisposeAsync();
        }

        public async Task<List<GameInfo>> GetAllGames ()
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            List<GameInfo> list = null;
            void CatanSignalRClient_OnAllGamesReceived (List<GameInfo> games)
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

        public async Task<List<string>> GetAllPlayerNames (Guid gameId)
        {
            if (HubConnection.State != HubConnectionState.Connected)
            {
                this.TraceMessage("Why isn't the hub Connected?");
                await Task.Delay(2000);
                this.TraceMessage($"Waited 2 seconds = state now {HubConnection.State}");
            }

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            List<string> list = null;
            void OnPlayerNamesReceived (List<string> playerNames)
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

        public async Task Initialize (string host, ICollection<CatanMessage> messageLog)
        {
            MessageLog = messageLog;
            try
            {
                if (host.Contains("192"))
                {
                    ServiceUrl = "http://" + host + "/CatanHub";
                }
                else
                {
                    ServiceUrl = "https://" + host + "/CatanHub";
                }

                _lastReconnected = DateTime.Now;
                HubConnection = new HubConnectionBuilder().WithAutomaticReconnect().WithUrl(ServiceUrl).ConfigureLogging((logging) =>
                {
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Trace);
                }).Build();

                HubConnection.ServerTimeout = TimeSpan.FromMinutes(5);
                HubConnection.HandshakeTimeout = TimeSpan.FromSeconds(10);
                HubConnection.KeepAliveInterval = TimeSpan.FromSeconds(19);

                HubConnection.Reconnecting += async error =>
                {
                    //   this.TraceMessage("Hub reconnecting!!");
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
                    this.TraceMessage($"recieved id {message.MessageId}");
                    var ack = AckModel.CreateMessage(message);
                    this.TraceMessage($"sent Ack: {((AckModel)ack.Data).AckedMessageId}");
                    await PostHubMessage(ack);
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        MessageLog.Add(message);
                        OnToAllClients(message);
                    });

                });

                HubConnection.On("ToOneClient", async (CatanMessage message) =>
                {
                    //
                    //  when we get the message, immediately send the ack -- don't switch threads to do so.
                    using (new FunctionTimer($"Ack + Process.  Id={message.MessageId} Type={message.DataTypeName}", true))
                    {
                        var ack = AckModel.CreateMessage(message);
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

                HubConnection.On("CreateGame", async (GameInfo gameInfo, string by) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        var message = CreateGameModel.CreateMessage(gameInfo);
                        MessageLog.Add(message);
                        OnGameCreated?.Invoke(gameInfo, by);
                    });
                });
                HubConnection.On("DeleteGame", async (GameInfo gameInfo, string by) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        RecordGameMessage(MessageType.DeleteGame, CatanAction.GameDeleted, gameInfo, by);
                        OnGameDeleted?.Invoke(gameInfo, by);
                    });
                });
                HubConnection.On("JoinGame", async (GameInfo gameInfo, string playerName) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        RecordGameMessage(MessageType.JoinGame, CatanAction.GameJoined, gameInfo, playerName);

                        OnGameJoined?.Invoke(gameInfo, playerName);
                    });
                });
                HubConnection.On("LeaveGame", async (GameInfo gameInfo, string playerName) =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        RecordGameMessage(MessageType.LeaveGame, CatanAction.None, gameInfo, playerName);
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

        public async Task<GameInfo> JoinGame (GameInfo gameInfo, string playerName)
        {
            if (gameInfo == null) throw new ArgumentException("GameInfo can't be null");
            if (String.IsNullOrEmpty(playerName)) throw new ArgumentException("PlayerName can't be null");

            this.GameInfo = gameInfo;
            this.PlayerName = playerName;

            try
            {
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                GameInfo serviceGameInfo = null;
                void CatanSignalRClient_OnGameJoined (GameInfo info, string name)
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

        public async Task<bool> KeepAlive ()
        {
            await Task.CompletedTask;
            return true;
        }

        public async Task<List<string>> LeaveGame (GameInfo gameInfo, string playerName)
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

        public async Task Reset ()
        {
            await HubConnection.InvokeAsync("Reset");
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
        public async Task SendBroadcastMessage (CatanMessage message)
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
                    await EnsureConnection();

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
                    }
                }
            }
            finally
            {
                BroadcastTcs.TrySetResult(null);
            }
        }

        public async Task SendPrivateMessage (string playerName, CatanMessage message)
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

        public async Task StartConnection (GameInfo info, string playerName)
        {
            await RegisterClient();
        }

        private async Task EnsureConnection ()
        {
            if (HubConnection.State == HubConnectionState.Connected) return;

            TaskCompletionSource<object> connectionTCS = new TaskCompletionSource<object>(); ;
            HubConnection.Reconnected += Reconnected;
            Task Reconnected (string arg)
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

        private void RecordGameMessage (MessageType messageType, CatanAction action, GameInfo info, string by)
        {
            GameLog logHeader = new GameLog()
            {
                CanUndo = false,
                Action = action,
                GameInfo = info,
                Name = by
            };
            CatanMessage message = new CatanMessage()
            {
                Data = logHeader,
                From = info.Name,
                ActionType = ActionType.Normal,
                DataTypeName = logHeader.GetType().FullName,
                To = "*",
                MessageType = messageType
            };
            this.MessageLog.Add(message);
        }

        private async Task RegisterClient ()
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                // this will happily throw..

                //
                //  this has to be on the UI thread to access PlayerName
                await HubConnection.InvokeAsync("Register", MainPage.Current.TheHuman.PlayerName);
            });
        }

    }

    public class PlayerModelConverter : JsonConverter<PlayerModel>
    {
        #region Methods

        public override PlayerModel Read (ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

        public override void Write (Utf8JsonWriter writer, PlayerModel player, JsonSerializerOptions options)
        {
            writer.WriteStringValue(player.PlayerName);
        }

        #endregion Methods
    }
}