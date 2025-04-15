using LiteNetLib;
using LiteNetLib.Utils;

namespace Narfox.Network;

public class Server
{
    static object _padlock = new object();
    EventBasedNetListener _listener;
    NetManager _server;
    ushort _maxConnections = 10;

    public Server(ushort maxConnections)
    {
        _maxConnections = maxConnections;
        _listener = new EventBasedNetListener();
        _server = new NetManager(_listener);

        _listener.ConnectionRequestEvent += OnConnectionRequest;
        _listener.PeerConnectedEvent += OnPeerConnected;
    }

    void OnConnectionRequest(ConnectionRequest request)
    {
        if(_server.ConnectedPeersCount < _maxConnections)
        {
            request.Accept();
        }
    }

    void OnPeerConnected(NetPeer peer)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put("Hello new client");
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    

    public void Start(ushort port)
    {
        _server.Start(port);
    }

    public void Stop()
    {
        _server.Stop();
    }

    public void Update()
    {
        if(_server != null && _server.IsRunning)
        {
            _server.PollEvents();
        }
    }
}
