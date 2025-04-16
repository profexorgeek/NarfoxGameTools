using LiteNetLib;
using Narfox.Logging;
using System.Text;
using static LiteNetLib.EventBasedNetListener;

namespace Narfox.Network;

public abstract class NetBase
{
    protected EventBasedNetListener _listener;
    protected NetManager _manager;
    protected ILogger _log;

    public NetBase(ILogger logger)
    {
        _log = logger;

        _log.Debug("Creating network listener...");
        _listener = new EventBasedNetListener();

        _log.Debug("Creating network manager...");
        _manager = new NetManager(_listener);

        _log.Debug("Binding network events...");
        _listener.ConnectionRequestEvent += OnConnectionRequest;
        _listener.NetworkReceiveEvent += OnNetworkDataReceived;
        _listener.NetworkErrorEvent += OnNetworkError;
        _listener.PeerConnectedEvent += OnPeerConnected;
        _listener.PeerDisconnectedEvent += OnPeerDisconnected;

        _log.Info("Net manager created, ready to Connect");
    }

    public void Update()
    {
        _manager.PollEvents();
    }

    public void Stop()
    {
        _log.Debug("Stopping Network manager...");
        _manager.Stop();
        _log.Info("Network manager stopped.");
    }


    protected virtual void OnConnectionRequest(ConnectionRequest request)
    {
        _log.Debug("Received connection request...");
    }

    protected virtual void OnPeerConnected(NetPeer peer)
    {
        _log.Info($"Peer {peer.Id} connected from: {peer.Address}:{peer.Port}({peer.Ping})");
        if (_log.Level == LogLevel.Debug)
        {
            var sb = new StringBuilder();
            foreach (var p in _manager.ConnectedPeerList)
            {
                sb.Append($"#{p.Id}({p.Ping}) - {p.Address}:{p.Port}\n");
            }
            _log.Debug($"These are our known peers:\n{sb.ToString()}");
        }
    }

    protected virtual void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        _log.Warn($"Peer #{peer.Id} from {peer.Address}:{peer.Port} disconnected.");
        if (_log.Level == LogLevel.Debug)
        {
            var sb = new StringBuilder();
            foreach (var p in _manager.ConnectedPeerList)
            {
                sb.Append($"#{p.Id}({p.Ping}) - {p.Address}:{p.Port}\n");
            }
            _log.Debug($"We now have these connected peers:\n{sb.ToString()}");
        }
    }

    protected virtual void OnNetworkError(System.Net.IPEndPoint endPoint, System.Net.Sockets.SocketError socketError)
    {
        _log.Error($"Socket error on {endPoint.Address}:{endPoint.Port} - {socketError.ToString()}");
    }

    protected virtual void OnNetworkDataReceived(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        var msg = reader.GetString(100);
        reader.Recycle();
        _log.Info($"{deliveryMethod.ToString()} message from {peer.Id} - {msg}");
    }
}
