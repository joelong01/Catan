using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.Web;

namespace Catan10
{
    public enum MessageType
    {
        Roll = 0,
        Next,
        Undo,
        AllowMoveBaron,
        CardsLostToMonopoly,
        CardsLostToSeven,
        Admonishment
    }

    public class CatanMessage
    {
        public MessageType ID { get; set; }
        public string User { get; set; }
        public string Value { get; set; }
    }

    class CatanWebSocket
    {
        private MessageWebSocket _webSocket = new MessageWebSocket();
        private MainPage _mainPage = null;

        public CatanWebSocket(MainPage page)
        {
            _webSocket.Control.MessageType = SocketMessageType.Utf8;
            _webSocket.MessageReceived += MessageReceived;
            _webSocket.Closed += WebSocketClosed;
            _mainPage = page;
        }

        public async Task Connect(Uri server)
        {
            await _webSocket.ConnectAsync(server);
            _ = this.SendMessage(MessageType.Admonishment, "Connected");
        }

        private void WebSocketClosed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            _webSocket.Dispose();
            _webSocket = null;
        }

        private async void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            try
            {
                using (DataReader dataReader = args.GetDataReader())
                {
                    dataReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                    string message = dataReader.ReadString(dataReader.UnconsumedBufferLength);
                    Debug.WriteLine("Message received from MessageWebSocket: " + message);
                    CatanMessage msg = JsonConvert.DeserializeObject<CatanMessage>(message);
                    await MessageHandler(msg);

                }
            }
            catch (Exception ex)
            {
                WebErrorStatus webErrorStatus = WebSocketError.GetStatus(ex.GetBaseException().HResult);
                Debug.WriteLine($"Exception in MessageReceived: {ex}\nWebErrorStatus={webErrorStatus}");
                // Add additional code here to handle exceptions.
            }
        }

        private async Task MessageHandler(CatanMessage msg)
        {
            switch (msg.ID)
            {
                case MessageType.Roll:
                    if (_mainPage.GameState != GameState.WaitingForRoll)
                    {
                        await SendMessage(MessageType.Admonishment, $" {msg.User} stop fucking around!");
                        return;
                    }
                    await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        var ret = await _mainPage.ProcessRoll(msg.Value);
                        if (!ret)
                        {
                            await SendMessage(MessageType.Admonishment, $" {msg.User} ProcessRoll failed for {msg.Value}");
                        }

                    });                    
                    break;

                case MessageType.Next:
                    await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await _mainPage.NextState());
                    break;
                case MessageType.Undo:
                    if (_mainPage.GameState == GameState.WaitingForNewGame)
                    {
                        await SendMessage(MessageType.Admonishment, $" {msg.User} stop fucking around!");
                        return;
                    }
                    await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await _mainPage.OnUndo());
                    break;
                case MessageType.AllowMoveBaron:
                    if (_mainPage.GameState != GameState.WaitingForRoll)
                    {
                        await SendMessage(MessageType.Admonishment, $" {msg.User} stop fucking around!");
                        return;
                    }
                    await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        _mainPage.CanMoveBaronBeforeRoll = true;
                        
                    });
                    
                    break;
                case MessageType.CardsLostToMonopoly:
                    await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        foreach (var player in _mainPage.PlayingPlayers)
                        {
                            if (player.PlayerName == msg.User)
                            {
                                player.GameData.CardsLostToMonopoly = Convert.ToInt32(msg.Value);
                                break;
                            }
                        }
                    });
                    break;
                case MessageType.CardsLostToSeven:
                    await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        foreach (var player in _mainPage.PlayingPlayers)
                        {
                            if (player.PlayerName == msg.User)
                            {
                                player.GameData.CardsLostToSeven = Convert.ToInt32(msg.Value);
                                break;
                            }
                        }
                    });
                    break;                    
                default:
                    break;
            }

        }

        public async Task SendMessage(MessageType type, string msg)
        {
            try
            {
                CatanMessage message = new CatanMessage()
                {
                    ID = type,
                    Value = msg,
                    User = "MainGame"
                };
                var toWrite = JsonConvert.SerializeObject(message);
                Debug.WriteLine($"Sending message: {toWrite}");
                using (var dataWriter = new DataWriter(_webSocket.OutputStream))
                {
                    dataWriter.WriteString(toWrite);
                    await dataWriter.StoreAsync();
                    dataWriter.DetachStream();
                }
            }
            catch (Exception ex)
            {
                var webErrorStatus = WebSocketError.GetStatus(ex.GetBaseException().HResult);
                Debug.WriteLine($"exception thrown while sending message.  text: {ex}");
            }
        }


    }
}
