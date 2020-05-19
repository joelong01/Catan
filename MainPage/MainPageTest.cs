using Catan.Proxy;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Media.Audio;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;




// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public class WSConnectInfo
    {
        public string SessionId { get; set; }
        public string PlayerName { get; set; }
    }


    public sealed partial class MainPage : Page, ILog
    {


        private void InitTest()
        {

        }


        // int toggle = 0;
        private async void OnTest1(object sdr, RoutedEventArgs rea)
        {

            MainPageModel.PlayingPlayers.Clear();
            var joe = NameToPlayer("Joe");
            Contract.Assert(joe != null);
            TheHuman = joe;
            CurrentPlayer = joe;
            SavedAppState.AllPlayers.Remove(joe);
            SavedAppState.AllPlayers.Insert(0, joe);
            //
            //  delete alls sessions
            List<SessionInfo> sessions = await Proxy.GetSessions();
            sessions.ForEach(async (session) =>
            {
                var s = await Proxy.DeleteSession(session.Id);
                if (s == null)
                {
                    var ErrorMessage = CatanProxy.Serialize(Proxy.LastError, true);
                    this.TraceMessage(ErrorMessage);
                }
            });



            // create a new session
            SessionInfo sessionInfo = new SessionInfo() { Id = Guid.NewGuid().ToString(), Description = "OnTest", Creator = CurrentPlayer.PlayerName };
            sessions = await Proxy.CreateSession(sessionInfo);
            Contract.Assert(sessions != null);
            MainPageModel.ServiceData.SessionInfo = sessionInfo;
            //
            //  start the game
            await StartGameLog.StartGame(this, "Joe", 0, true);

            //
            //  add players
            foreach (var p in SavedAppState.AllPlayers)
            {
                await Proxy.JoinSession(sessionInfo.Id, p.PlayerName);
                await AddPlayerLog.AddPlayer(this, p);

            }

            StartMonitoring();



        }
        private MessageWebSocket messageWebSocket;
        private DataWriter messageWriter;
        private async void OnTest2(object sdr, RoutedEventArgs rea)
        {
            Uri server = new Uri("ws://192.168.1.128:5000/catan/session/monitor/ws");
            messageWebSocket = new MessageWebSocket();
            messageWebSocket.Control.MessageType = SocketMessageType.Utf8;
            messageWebSocket.MessageReceived += MessageReceived;
            messageWebSocket.Closed += OnClosed;
            await messageWebSocket.ConnectAsync(server);
            messageWriter = new DataWriter(messageWebSocket.OutputStream);

            WSConnectInfo info = new WSConnectInfo()
            {
                SessionId = Guid.NewGuid().ToString(),
                PlayerName = "Joe"
            };
            var json = CatanProxy.Serialize<WSConnectInfo>(info);
            messageWriter.WriteString(json);
            await messageWriter.StoreAsync();

            messageWriter.DetachStream();
            messageWriter.Dispose();
            messageWebSocket.Close(1000, "Closed due to user request.");
            
        }

        private void OnClosed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            this.TraceMessage("closed");
        }

        private void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            var ignore = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
               
                using (DataReader reader = args.GetDataReader())
                {
                    reader.UnicodeEncoding = UnicodeEncoding.Utf8;

                    try
                    {
                        string read = reader.ReadString(reader.UnconsumedBufferLength);
                        this.TraceMessage(read);
                    }
                    catch (Exception ex)
                    {
                        this.TraceMessage(ex.ToString());
                        this.TraceMessage(ex.Message);
                    }
                }
            });
        }

        // Undo
        private async void OnTest3(object sdr, RoutedEventArgs rea)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".log");
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file == null) return;

            string json = await FileIO.ReadTextAsync(file);
            List<CatanMessage> messages = CatanProxy.Deserialize<List<CatanMessage>>(json);
            foreach (var message in messages)
            {
                Type type = CurrentAssembly.GetType(message.TypeName);
                if (type == null) throw new ArgumentException("Unknown type!");
                LogHeader logHeader = JsonSerializer.Deserialize(message.Data.ToString(), type, CatanProxy.GetJsonOptions()) as LogHeader;
                message.Data = logHeader;
                Contract.Assert(logHeader != null, "All messages must have a LogEntry as their Data object!");
                
                ILogController logController = logHeader as ILogController;
                Contract.Assert(logController != null, "every LogEntry is a LogController!");
                switch (logHeader.LogType)
                {
                    case LogType.Normal:
                         await logController.Redo(this, (LogHeader)message.Data);
                        break;
                    case LogType.Undo:
                        await MainPageModel.Log.Undo(message);
                        break;
                    case LogType.Replay:

                        await MainPageModel.Log.Redo(message);
                        break;
                    case LogType.DoNotLog:
                    case LogType.DoNotUndo:
                    default:
                        throw new InvalidDataException("These Logtypes shouldn't be set in a service game");
                }

            }

            StartMonitoring();

        }

        private void VerifyRoundTrip<T>(T model)
        {
            //var options = new JsonSerializerOptions() { WriteIndented = true };
            //options.Converters.Add(new JsonStringEnumConverter());
            //var jsonString = JsonSerializer.Serialize<T>(model, options);
            //T newModel = JsonSerializer.Deserialize<T>(jsonString, options);
            //var newJsonString = JsonSerializer.Serialize<T>(newModel, options);
            ////   this.TraceMessage(newJsonString);
            //Debug.Assert(newJsonString == jsonString);

        }

        readonly Random testRandom = new Random();

        private void OnTestRegularGame(object sender, RoutedEventArgs e)
        {
            //AnimationSpeedBase = 10; // speed up the animations


            //await this.Reset();
            ////   await MainPageModel.Log.Init(CreateSaveFileName("Test Game"));

            //await SetStateAsync(null, GameState.WaitingForNewGame, true);
            //_gameView.CurrentGame = _gameView.Games[0];

            ////  SavedGames.Insert(0, MainPageModel.Log);
            //await AddLogEntry(null, GameState.GamePicked, CatanAction.SelectGame, true, LogType.Normal, 0);
            //List<PlayerModel> PlayerDataList = new List<PlayerModel>
            //{
            //    SavedAppState.AllPlayers[0],
            //    SavedAppState.AllPlayers[1],
            //    SavedAppState.AllPlayers[2],
            //    SavedAppState.AllPlayers[3]
            //};
            //await StartGame(PlayerDataList, 0);
            //await NextState(); // simluates pushing "Start"
            //CurrentPlayer = MainPageModel.PlayingPlayers[0];
            //await PickSettlementsAndRoads();


        }
        private async void OnTestExpansionGame(object sender, RoutedEventArgs e)
        {
            AnimationSpeedBase = 10; // speed up the animations
            RandomGoldTileCount = 3;
            await this.Reset();
            // await MainPageModel.Log.Init(CreateSaveFileName("Expansion Game"));
            await SetStateAsync(null, GameState.WaitingForNewGame, true);
            _gameView.CurrentGame = _gameView.Games[1];

            //   SavedGames.Insert(0, MainPageModel.Log);
            //   await AddLogEntry(null, GameState.GamePicked, CatanAction.SelectGame, true, LogType.Normal, 1);
            List<PlayerModel> PlayerDataList = new List<PlayerModel>
            {
                SavedAppState.AllPlayers[0],
                SavedAppState.AllPlayers[1],
                SavedAppState.AllPlayers[2],
                SavedAppState.AllPlayers[3],
                SavedAppState.AllPlayers[4],
            };
            await StartGame(PlayerDataList, 1);
            await NextState(); // simluates pushing "Start"
            CurrentPlayer = MainPageModel.PlayingPlayers[0];
            await PickSettlementsAndRoads();


        }
    }
}