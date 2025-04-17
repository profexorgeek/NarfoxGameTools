using LiteNetLib;
using LiteNetLib.Utils;
using Narfox.Data.Models;
using Narfox.Logging;
using System.Net.Mail;
using System.Text;

namespace Narfox.Network;

public class NetServer : NetBase
{
    static object _padlock = new object();
    ushort _maxConnections = 10;

    // The LiteNetLib system assigns LOCAL ids such that each client has
    // different identifiers for clients. We need a shared identifier system
    // so that we can identify clients and entities uniquely on the network.
    // We start at an arbitrary number just to help avoid confusion between the
    // id kept by LiteNetLib and the id kept by this layer.
    ushort _clientId = 13;

    public NetServer(ushort maxConnections, ILogger logger)
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

        var newConnectionId = _clientId++;

        NetDataWriter writer = new NetDataWriter();
        writer.Put((byte)NetMessageType.ConnectionAccepted);
        writer.Put(newConnectionId);
        writer.Put($"Your unique ID is #{peer.Id}, please send your connection details!");

        // add a dictionary placeholder, we don't have any client data yet
        _clients.Add(newConnectionId, null);

        // send the message to the newly-connected client
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
        DebugLogPeers();
    }

    protected override void OnNetworkDataReceived(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
    {
        var msgType = (NetMessageType)reader.GetByte();

        _log.Info($"Got {msgType.ToString()} message ({deliveryMethod.ToString()}) from {peer.Id}");

        switch (msgType)
        {
            case (NetMessageType.ConnectionAccepted):
                /* NOOP */
                break;
            case (NetMessageType.ClientDetails):
                var client = GetPayloadFromMessage<Client>(reader);

                if (_clients.ContainsKey(peer.Id))
                {
                    _log.Debug($"Updating client #{peer.Id} with client data ({client.Name})");
                    _clients[peer.Id] = client;
                }
                else
                {
                    _log.Debug($"Adding client #{peer.Id} with client data ({client.Name})");
                    _clients.Add(peer.Id, client);
                }
                DebugLogPeers();
                break;
            case (NetMessageType.SerializedDataMessage):
                break;
        }

        reader.Recycle();
        _log.Info($"{deliveryMethod.ToString()} message from {peer.Id}");
    }
}
