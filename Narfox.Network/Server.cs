using LiteNetLib;
using LiteNetLib.Utils;
using Narfox.Logging;
using System.Text;

namespace Narfox.Network;

public class Server : NetBase
{
    static object _padlock = new object();
    ushort _maxConnections = 10;

    public Server(ushort maxConnections, ILogger logger)
        : base(logger)
    {
        _maxConnections = maxConnections;
    }

    public void Start(ushort port)
    {
        _log.Debug("Starting server...");
        _manager.Start(port);
        _log.Info($"Server is listening on port {port}");
    }

    protected override void OnConnectionRequest(ConnectionRequest request)
    {
        _log.Debug("Server received connection request...");
        if (_manager.ConnectedPeersCount < _maxConnections)
        {
            _log.Info($"Accepting connection, now at {_manager.ConnectedPeersCount + 1}/{_maxConnections} connections.");
            request.Accept();
        }
        else
        {
            _log.Warn("Too many connections, denied new connection request.");
        }
    }

    protected override void OnPeerConnected(NetPeer peer)
    {
        _log.Info($"Peer {peer.Id} connected from: {peer.Address}:{peer.Port}({peer.Ping})");
        NetDataWriter writer = new NetDataWriter();
        writer.Put($"Welcome, your ID is #{peer.Id}");
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
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
}
