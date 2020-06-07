using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Catan.Proxy;
using Microsoft.AspNetCore.SignalR.Client;
using Windows.Networking.Proximity;
using Windows.UI.Core;
using System.Reflection;

namespace Catan10
{
    internal class CatanSignalRClient : IDisposable

    {
        private static Assembly CurrentAssembly { get; } = Assembly.GetExecutingAssembly();

        private string ServiceUrl { get; set; }

        private HubConnection connection;

        private async void MessageReceived(string from, string jsonMessage)
        {
            this.TraceMessage($"[from={from}][json={jsonMessage}]");
            if (OnMessageReceived != null)
            {
                try
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
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            OnMessageReceived.Invoke(message);
                        });
                }
                catch (Exception e)
                {
                    this.TraceMessage(e.ToString());
                }
            }
        }

        public CatanSignalRClient(string url)
        {
            connection = new HubConnectionBuilder()
              .WithUrl(url)
              .Build();

            connection.Reconnecting += error =>
            {
                Debug.Assert(connection.State == HubConnectionState.Reconnecting);

                // Notify users the connection was lost and the client is reconnecting.
                // Start queuing or dropping messages.

                return Task.CompletedTask;
            };

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };

            connection.On<string, string>("BroadcastMessage", (string name, string json) => MessageReceived(name, json));
        }

        public event MessageReceivedHandler OnMessageReceived;

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

        public void Dispose()
        {
            connection.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task SendMessage(string user, CatanMessage message)
        {
            try
            {
                string json = JsonSerializer.Serialize(message, GetJsonOptions());
                await connection.InvokeAsync("BroadcastMessage", user, json);
            }
            catch (Exception ex)
            {
                this.TraceMessage($"Exception! [Message={ex.Message}");
            }
        }

        public async Task StartConnection()
        {
            try
            {
                await connection.StartAsync();
            }
            catch (Exception ex)
            {
                this.TraceMessage(ex.Message);
            }
        }
    }

    public delegate void MessageReceivedHandler(CatanMessage message);
}
