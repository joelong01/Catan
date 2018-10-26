using System;
using System.Collections.Generic;
using System.ServiceModel;
using Windows.Devices.WiFiDirect;
using Windows.Networking.Sockets;

namespace Catan10
{

    public enum PurchaseableItem { ResourceCard, Settlement, City, Road, Ship };
    [ServiceContract]
    interface ICatanRemoteServer
    {
        [OperationContract]
        double Roll(int roll);
        [OperationContract]
        double Undo();

        [OperationContract]
        double BuyResource(PurchaseableItem item, List<ResourceType> cards);

    }




    class CatanService : ICatanRemoteServer
    {
        private StreamSocketListener _tcpListener;
        //   private StreamSocket _connectedSocket = null;
        private const string PORT_NUMBER = "1337";

        public CatanService() { }

        public void Init()
        {
            // Create an Advertisement Publisher
            WiFiDirectAdvertisementPublisher publisher = new WiFiDirectAdvertisementPublisher();

            // Turn on Listen state
            publisher.Advertisement.ListenStateDiscoverability = WiFiDirectAdvertisementListenStateDiscoverability.Normal;

            // Register for connection requests
            WiFiDirectConnectionListener listener = new WiFiDirectConnectionListener();
            listener.ConnectionRequested += OnConnectionRequested;

            // Start the advertiser
            publisher.Start();

        }

        private async void OnConnectionRequested(WiFiDirectConnectionListener sender, WiFiDirectConnectionRequestedEventArgs args)
        {
            WiFiDirectConnectionRequest ConnectionRequest = args.GetConnectionRequest();

            // Prompt the user to accept/reject the connection request
            // If rejected, exit

            // Connect to the remote device
            WiFiDirectDevice wfdDevice = await WiFiDirectDevice.FromIdAsync(ConnectionRequest.DeviceInformation.Id);

            // Get the local and remote IP addresses
            IReadOnlyList<Windows.Networking.EndpointPair> EndpointPairs = wfdDevice.GetConnectionEndpointPairs();

            // Establish standard WinRT socket with above IP addresses
        }

        public async void StartListening()
        {
            _tcpListener = new StreamSocketListener();
            _tcpListener.ConnectionReceived += OnConnected;
            await _tcpListener.BindEndpointAsync(null, PORT_NUMBER);

        }

        private void OnConnected(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            throw new NotImplementedException();
        }

        double ICatanRemoteServer.Roll(int roll)
        {
            throw new NotImplementedException();
        }

        double ICatanRemoteServer.Undo()
        {
            throw new NotImplementedException();
        }

        double ICatanRemoteServer.BuyResource(PurchaseableItem item, List<ResourceType> cards)
        {
            throw new NotImplementedException();
        }
    }
}
