using LiteNetLib;
using LiteNetLib.Utils;
using Narfox.Data.Models;
using Narfox.Logging;
using Newtonsoft.Json;
using System.Text;
using static LiteNetLib.EventBasedNetListener;

namespace Narfox.Network;

public abstract class NetBase
{
    protected EventBasedNetListener _listener;
    protected NetManager _manager;
    protected ILogger _log;
    protected Dictionary<int, Client> _clients;
    protected JsonSerializerSettings _serializerSettings;


    public NetBase(ILogger logger)
    {
        _log = logger;
        _clients = new Dictionary<int, Client>();

        _log.Debug("Creating network listener...");
        _listener = new EventBasedNetListener();

        _log.Debug("Creating network manager...");
        _manager = new NetManager(_listener);

        _log.Debug("Configuring serializer...");
        _serializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
        };

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

    protected void SendDataToPeer<T>(T data, NetPeer peer, NetMessageType msgType = NetMessageType.SerializedDataMessage, DeliveryMethod method = DeliveryMethod.ReliableSequenced)
    {
        NetDataWriter writer = new NetDataWriter();

        // write the message overall type
        writer.Put((byte)msgType);

        // put the class name so we know how to deserialize on the other side
        writer.Put(typeof(T).ToString(), 32);

        // add the data payload
        var json = JsonConvert.SerializeObject(data, _serializerSettings);
        var bytes = Encoding.UTF8.GetBytes(json);
        if(bytes.Length > 1024)
        {
            var error = $"Too large of message, tried to put {bytes.Length} in a single packet!";
            _log.Error(error);
            throw new Exception(error);
        }
        writer.Put(json);

        // send the message
        peer.Send(writer, method);
    }

    protected T GetPayloadFromMessage<T>(NetPacketReader reader)
    {
        // TODO: error handling
        var className = reader.GetString(32);
        var json = reader.GetString();
        var obj = JsonConvert.DeserializeObject<T>(json);

        return obj;
    }

    protected void DebugLogPeers()
    {
        if (_log.Level == LogLevel.Debug)
        {
            var sb = new StringBuilder();
            foreach (var p in _manager.ConnectedPeerList)
            {
                var id = _clients.ContainsKey(p.Id) ? _clients[p.Id].Name : p.Id.ToString();
                sb.Append($"{id} ({p.Ping}ms) @ {p.Address}:{p.Port}\n");
            }
            _log.Debug($"These are our known peers:\n{sb.ToString()}");
        }
    }



    protected virtual void OnConnectionRequest(ConnectionRequest request)
    {
        _log.Debug("Received connection request...");
    }

    protected virtual void OnPeerConnected(NetPeer peer)
    {
        _log.Info($"Peer {peer.Id} connected from: {peer.Address}:{peer.Port}({peer.Ping})");
        DebugLogPeers();
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
        throw new NotImplementedException("This code should not be reachable - client or server needs to implement this.");
    }
}
