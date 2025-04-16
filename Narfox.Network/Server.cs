using LiteNetLib;
using LiteNetLib.Utils;
using Narfox.Logging;
using System.ComponentModel.Design;

namespace Narfox.Network;

public class Server
{
    static object _padlock = new object();
    EventBasedNetListener _listener;
    NetManager _server;
    ushort _maxConnections = 10;
    ILogger _log;

    public Server(ushort maxConnections, ILogger logger)
    {
        _log = logger;
        _maxConnections = maxConnections;

        _log.Debug("Creating network event listener...");
        _listener = new EventBasedNetListener();

        _log.Debug("Creating network manager...");
        _server = new NetManager(_listener);

        _log.Debug("Binding network events...");
        _listener.ConnectionRequestEvent += OnConnectionRequest;
        _listener.PeerConnectedEvent += OnPeerConnected;
        
        _log.Info($"Server is now ready for up to {maxConnections} connections.");
    }


    public void Start(ushort port)
    {
        _log.Debug("Starting server...");
        _server.Start(port);
        _log.Info($"Server is listening on port {port}");
    }

    public void Stop(string reason = null)
    {
        _server.Stop();

        if(reason != null)
        {
            _log.Info($"Server stopped for: {reason}");
        }
        else
        {
            _log.Info($"Server stopped.");
        }
        
    }

    public void Update()
    {
        if(_server != null && _server.IsRunning)
        {
            _server.PollEvents();
        }
    }



    void OnConnectionRequest(ConnectionRequest request)
    {
        _log.Debug("Received connection request...");
        if (_server.ConnectedPeersCount < _maxConnections)
        {
            _log.Info($"Accepting connection, now at {_server.ConnectedPeersCount + 1}/{_maxConnections} connections.");
            request.Accept();
        }
        else
        {
            _log.Warn("Too many connections, denied new connection request.");
        }
    }

    void OnPeerConnected(NetPeer peer)
    {
        _log.Info($"Peer connected from: {peer.Address}:{peer.Port}({peer.Ping})");
        NetDataWriter writer = new NetDataWriter();
        writer.Put("Hello new client");
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }
}
