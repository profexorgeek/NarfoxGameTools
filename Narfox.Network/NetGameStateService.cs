using LiteNetLib;
using LiteNetLib.Utils;
using Narfox.Data;
using Narfox.Data.Models;
using Narfox.Logging;
using Narfox.Network.Enums;
using Newtonsoft.Json;
using System.Text;

namespace Narfox.Network;

/// <summary>
/// This class extends the GameStateService to manage the
/// state across multiple clients via LiteNetLib networking
/// 
/// 
/// CONNECTION FORMING PROCESS
/// 1. Client calls Connect and reaches out to server
/// 2. Server receives ConnectionRequest event and accepts
/// 3. Both Client and Server receive PeerConnected events
/// 4. Server sends a new ID to new peer and increments ID counter
/// 5. Peer receives new ID and sends it's Client detail data to server
/// 6. Server propagates client detail to all clients
/// 7. All clients should now know about new client, server is not stored in the Clients list
/// 
/// TODO: How to determine who has authority on the network?
/// </summary>
public class NetGameStateService : GameStateService
{
    static object _padlock = new object();


    /// <summary>
    /// The EventBasedNetListener is a lightweight object that is
    /// mostly used by the NetManager to raise network events while
    /// it is polling the network. This class subscribes and reacts
    /// to the events raised on this listener
    /// </summary>
    EventBasedNetListener _listener;

    /// <summary>
    /// The NetManager does the heavy lifting in LiteNetLib and
    /// can act as either a server or a client. This is the inner
    /// library that this implementation wraps.
    /// </summary>
    NetManager _manager;

    /// <summary>
    /// This library logs robust information to help debut realtime
    /// networking via this log interface.
    /// </summary>
    ILogger _log;

    /// <summary>
    /// This dictionary maps a peer Id number, which is not unique across
    /// the network and may be different for every client, to a client
    /// metadata object which carries a unique Id, client name, etc.
    /// </summary>
    List<Client> _clients;

    /// <summary>
    /// Only used in server mode to associate Peer data with client data
    /// </summary>
    Dictionary<NetPeer, Client> _peerClients;

    /// <summary>
    /// These settings should be used when serializing and deserializing
    /// packet payloads to help minimize the packet sizes passed back and forth
    /// </summary>
    JsonSerializerSettings _serializerSettings;

    /// <summary>
    /// This identifies the role of this client on the network
    /// </summary>
    NetRole _role = NetRole.Unknown;

    /// <summary>
    /// This is the counter used to uniquely identify clients on the network.
    /// It starts with an arbitrary offset from zero just to help avoid debug 
    /// confusion between the LiteNetLib Id, which is only unique locally, and
    /// the Client Id defined by this wrapper class, which is unique globally.
    /// </summary>
    ushort _clientId = 13;



    /// <summary>
    /// The maximum number of accepted connections, only used by a server
    /// </summary>
    public ushort MaxConnections { get; set; } = 10;



    public NetGameStateService(ILogger logger)
    {
        _log = logger;
        _clients = new List<Client>();

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



    /// <summary>
    /// Starts this service in server mode
    /// </summary>
    /// <param name="port"></param>
    public void StartServer(ushort port)
    {
        _log.Debug("Starting server...");
        _peerClients = new Dictionary<NetPeer, Client>();
        _manager.Start(port);
        _role = NetRole.Server;
    }

    /// <summary>
    /// Starts this service in client mode
    /// </summary>
    public void StartClient()
    {
        _log.Debug("Starting client...");
        _manager.Start();
        _role = NetRole.Client;
    }

    /// <summary>
    /// Called to connect to a server when in client mode.
    /// </summary>
    /// <param name="address">The server address</param>
    /// <param name="port">The server port</param>
    /// <exception cref="InvalidOperationException">Invalid operation thrown if called as anything but Client</exception>
    public void Connect(string address, ushort port)
    {
        if(_role != NetRole.Client)
        {
            var msg = "Connect should only be called by a NetRole.Client!";
            _log.Error(msg);
            throw new InvalidOperationException(msg);
        }

        var tempId = Guid.NewGuid().ToString("N");

        _log.Debug($"Connecting to: {address}:{port}");
        _manager.Connect(address, port, tempId);
    }

    /// <summary>
    /// Called in the main game loop to poll all network events
    /// </summary>
    public void Update()
    {
        _manager.PollEvents();
    }

    /// <summary>
    /// Called to shut down this service
    /// </summary>
    public void Stop()
    {
        _log.Debug("Stopping Network manager...");
        _manager.Stop();
        _log.Info("Network manager stopped.");

        // TODO: force disconnect?
        // TODO: clear all clients?
    }



    #region Network Listener Events
    /// <summary>
    /// Called when a connection request message is received. This should only
    /// be handled by server instances.
    /// </summary>
    /// <param name="request">The incoming request</param>
    void OnConnectionRequest(ConnectionRequest request)
    {
        switch (_role)
        {
            case NetRole.Server:
                HandleConnectionRequest(request);
                break;
            default:
                var msg = "Received a connection request while not running as server!";
                _log.Error(msg);
                throw new InvalidOperationException(msg);
                break;
        }

    }

    /// <summary>
    /// Called when a new peer has connected. For a client, this will generally only
    /// be called when they connect to the server.
    /// </summary>
    /// <param name="peer">The new peer</param>
    void OnPeerConnected(NetPeer peer)
    {
        _log.Info($"Peer {peer.Id} connected from: {peer.Address}:{peer.Port}({peer.Ping})");
        if (_role == NetRole.Server)
        {
            SendNewConnectionInfo(peer);
        }
        DebugLogKnownClients();
    }

    /// <summary>
    /// Called when a peer has disconnected. For a client, this is called when the server
    /// rejects a connection or they lose their connection.
    /// </summary>
    /// <param name="peer">The peer that disconnected</param>
    /// <param name="disconnectInfo">Information about the disconnection</param>
    void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
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

    /// <summary>
    /// Fired when there is a network error.
    /// </summary>
    /// <param name="endPoint">The error endpoint</param>
    /// <param name="socketError">A socket error object</param>
    void OnNetworkError(System.Net.IPEndPoint endPoint, System.Net.Sockets.SocketError socketError)
    {
        _log.Error($"Socket error on {endPoint.Address}:{endPoint.Port} - {socketError.ToString()}");
    }

    /// <summary>
    /// Called when network data is received. This is the most common networking event once
    /// a stable connection is formed. This method checks the first byte to see what type
    /// of messgae it is, then calls the appropriate handler for that message type.
    /// </summary>
    /// <param name="peer">The message source.</param>
    /// <param name="reader">The message data object</param>
    /// <param name="channel">The receiving channel</param>
    /// <param name="deliveryMethod">The message delivery method</param>
    void OnNetworkDataReceived(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        var msgType = (NetMessageType)reader.GetByte();
        _log.Info($"Got {msgType.ToString()} message ({deliveryMethod.ToString()}) from {peer.Id}");

        switch (msgType)
        {
            case (NetMessageType.ConnectionAccepted):
                HandleConnectionAcceptedMessage(peer, reader);
                break;
            case (NetMessageType.ClientDetails):
                HandleClientDetailMessage(peer, reader);
                break;
            case (NetMessageType.DataPayload):
                HandleDataPayloadMessage(peer, reader);
                break;
        }
    }
    #endregion



    #region Send/Receive Helpers
    /// <summary>
    /// Sends a data message with the provided data payload to all peers. For a
    /// client, this should just be the server. For the server, it's all connected
    /// clients.
    /// </summary>
    /// <typeparam name="T">A data payload less than 1024 bytes</typeparam>
    /// <param name="data">The data payload object</param>
    /// <param name="msgType">The type of message to send</param>
    /// <param name="method">The send method</param>
    /// <exception cref="Exception">Throws exception if serialized message is larger than 1024 bytes</exception>
    void SendData<T>(T data, NetMessageType msgType = NetMessageType.DataPayload, DeliveryMethod method = DeliveryMethod.ReliableSequenced)
    {
        NetDataWriter writer = new NetDataWriter();

        // write the message overall type
        writer.Put((byte)msgType);

        // put the class name so we know how to deserialize on the other side
        writer.Put(typeof(T).ToString(), 32);

        // add the data payload
        var json = JsonConvert.SerializeObject(data, _serializerSettings);
        var bytes = Encoding.UTF8.GetBytes(json);
        if (bytes.Length > 1024)
        {
            var error = $"Too large of message, tried to put {bytes.Length} in a single packet!";
            _log.Error(error);
            throw new Exception(error);
        }
        writer.Put(json);

        // send the message to all peers
        _manager.SendToAll(writer, method);
    }

    /// <summary>
    /// Gets the payload from a data message
    /// </summary>
    /// <typeparam name="T">The type of incoming data</typeparam>
    /// <param name="reader">The message data object</param>
    /// <returns>The deserialized object</returns>
    T GetPayloadFromMessage<T>(NetPacketReader reader)
    {
        // TODO: error handling
        var className = reader.GetString(32);
        var json = reader.GetString();
        var obj = JsonConvert.DeserializeObject<T>(json);

        return obj;
    }

    /// <summary>
    /// Sends the provided client details to the network
    /// </summary>
    /// <param name="client"></param>
    void SendClientInfo(Client client)
    {
        _log.Debug($"Sending client data ({client.Name}) to server...");
        SendData(client, NetMessageType.ClientDetails, DeliveryMethod.ReliableOrdered);
    }

    /// <summary>
    /// Sends a newly-connected peer their unique ID and
    /// sends information about every other connected client
    /// </summary>
    /// <param name="peer">The newly-connected client</param>
    void SendNewConnectionInfo(NetPeer peer)
    {
        var newConnectionId = _clientId++;

        NetDataWriter writer = new NetDataWriter();
        writer.Put((byte)NetMessageType.ConnectionAccepted);
        writer.Put(newConnectionId);

        // add a dictionary placeholder, we don't have any client data yet
        _peerClients.Add(peer, null);

        // send the message to the newly-connected client
        peer.Send(writer, DeliveryMethod.ReliableOrdered);

        // also send any existing clients to the new client so they know about everyone
        foreach(var client in _clients)
        {
            if(client != null && client.Id != newConnectionId)
            {
                SendClientInfo(client);
            }
        }

        DebugLogKnownClients();
    }

    /// <summary>
    /// Lists all peers via logger IF the logger is in debug mode.
    /// Otherwise it's a noop because building the string is a waste
    /// of time if it's not going to be logged
    /// </summary>
    void DebugLogKnownClients()
    {
        if (_log.Level == LogLevel.Debug)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Listing known clients...");

            if(_role != NetRole.Server)
            {
                sb.AppendLine($"{LocalClient.Id} - {LocalClient.Name} (ME!)");
            }

            foreach (var client in _clients)
            {
                if(_role == NetRole.Server)
                {
                    var peer = _peerClients.FirstOrDefault(kvp => kvp.Value.Id == client.Id).Key;
                    sb.AppendLine($"{client.Id} - {client.Name} ({peer.Id} {peer.Address}:{peer.Port})");
                }
                else
                {
                    sb.AppendLine($"{client.Id} - {client.Name}");
                }
            }
            _log.Debug(sb.ToString());
        }
    }
    #endregion



    #region Handlers For Specific NetMessage types
    /// <summary>
    /// Called by a server when a new client tries to connect. This is a unique
    /// primitive message type that is not in the NetMessage enum
    /// </summary>
    /// <param name="request">The incoming request</param>
    /// <exception cref="InvalidOperationException">Thrown if non-server tries to handle an incoming request</exception>
    void HandleConnectionRequest(ConnectionRequest request)
    {
        if(_role != NetRole.Server)
        {
            var msg = "Trying to handle a connection request when not in Server mode!";
            _log.Error(msg);
            throw new InvalidOperationException(msg);
        }

        _log.Debug("Server received connection request...");
        if (_manager.ConnectedPeersCount < MaxConnections)
        {
            _log.Info($"Accepting connection, now at {_manager.ConnectedPeersCount + 1}/{MaxConnections} connections.");
            request.Accept();
        }
        else
        {
            _log.Warn("Too many connections, denied new connection request.");
        }
    }

    /// <summary>
    /// Should be overridden to handle a connection message depending on whether
    /// the recipient is acting as client or server
    /// </summary>
    /// <param name="peer">The message source</param>
    /// <param name="reader">The message data object</param>
    void HandleConnectionAcceptedMessage(NetPeer peer, NetPacketReader reader)
    {
        var myId = reader.GetUShort();
        _log.Debug($"Server accepted connection, my id is {myId}...");

        // Rebuild the LocalClient
        LocalClient = new Client()
        {
            Id = myId,
            Name = LocalClient != null ? LocalClient.Name : Guid.NewGuid().ToString("N"),
        };

        // send our more detailed data to the network
        SendClientInfo(LocalClient);
    }

    /// <summary>
    /// Called when a peer has sent new client data
    /// </summary>
    /// <param name="peer"></param>
    /// <param name="reader"></param>
    protected virtual void HandleClientDetailMessage(NetPeer peer, NetPacketReader reader)
    {
        var client = GetPayloadFromMessage<Client>(reader);

        // lock to avoid accidental double insertion
        lock(_padlock)
        {
            if(client.Id != LocalClient.Id && _clients.Any(c => c.Id == client.Id) == false)
            {
                _log.Debug($"Discovered new client: {client.Id} {client.Name}");
                _clients.Add(client);
            }
            
            if(_role == NetRole.Server)
            {
                if(_peerClients.ContainsKey(peer))
                {
                    _peerClients[peer] = client;
                }
                else
                {
                    _peerClients.Add(peer, client);
                }
            }
        }
        
        DebugLogKnownClients();

        // if we are the server, pass this message on to all peers
        if(_role == NetRole.Server)
        {
            SendClientInfo(client);
        }
    }

    /// <summary>
    /// Called when a data message is received.
    /// </summary>
    /// <param name="peer">The message source</param>
    /// <param name="reader">The message data object</param>
    protected virtual void HandleDataPayloadMessage(NetPeer peer, NetPacketReader reader) { }
    #endregion
}
