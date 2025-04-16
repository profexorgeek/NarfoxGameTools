using LiteNetLib;
using Narfox.Logging;
using System.Text;

namespace Narfox.Network
{
    public class Client
    {
        EventBasedNetListener _listener;
        NetManager _client;
        ILogger _log;

        public Client(ILogger logger)
        {
            _log = logger;

            _log.Debug("Creating network listener...");
            _listener = new EventBasedNetListener();

            _log.Debug("Creating network client...");
            _client = new NetManager(_listener);

            _log.Debug("Starting client...");
            _client.Start();

            _log.Debug("Binding network events...");
            _listener.NetworkReceiveEvent += OnNetworkDataReceived;
            _listener.NetworkErrorEvent += OnNetworkError;
            _listener.PeerConnectedEvent += OnPeerConnected;
            _listener.PeerDisconnectedEvent += OnPeerDisconnected;

            _log.Info("Client created, ready to Connect");
        }

        public void Update()
        {
            _client.PollEvents();
        }

        public void Connect(string address, ushort port)
        {
            _log.Debug($"Connecting to: {address}:{port}");
            _client.Connect(address, port, "Hi I'm client!");
        }

        public void Stop()
        {
            _log.Debug("Stopping client...");
            _client.Stop();
            _log.Info("Client stopped.");
        }

        private void OnPeerConnected(NetPeer peer)
        {
            _log.Info($"Peer {peer.Id} connected from: {peer.Address}:{peer.Port}({peer.Ping})");
            if (_log.Level == LogLevel.Debug)
            {
                var sb = new StringBuilder();
                foreach (var p in _client.ConnectedPeerList)
                {
                    sb.Append($"#{p.Id}({p.Ping}) - {p.Address}:{p.Port}\n");
                }
                _log.Debug($"We now have these connected peers:\n{sb.ToString()}");
            }
        }

        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            _log.Warn($"Peer #{peer.Id} from {peer.Address}:{peer.Port} disconnected.");
            if (_log.Level == LogLevel.Debug)
            {
                var sb = new StringBuilder();
                foreach (var p in _client.ConnectedPeerList)
                {
                    sb.Append($"#{p.Id}({p.Ping}) - {p.Address}:{p.Port}\n");
                }
                _log.Debug($"We now have these connected peers:\n{sb.ToString()}");
            }
        }

        private void OnNetworkError(System.Net.IPEndPoint endPoint, System.Net.Sockets.SocketError socketError)
        {
            _log.Error($"Socket error on {endPoint.Address}:{endPoint.Port} - {socketError.ToString()}");
        }

        private void OnNetworkDataReceived(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            var msg = reader.GetString(100);
            reader.Recycle();
            _log.Info($"{deliveryMethod.ToString()} message from {peer.Id} - {msg}");
        }
    }
}
