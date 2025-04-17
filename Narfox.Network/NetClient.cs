using LiteNetLib;
using LiteNetLib.Utils;
using Narfox.Data.Models;
using Narfox.Logging;

namespace Narfox.Network
{
    public class NetClient : NetBase
    {
        public NetClient(ILogger logger)
            : base(logger)
        {
            _log.Debug("Starting client...");
            _manager.Start();
        }

        public void Connect(string address, ushort port)
        {
            var tempId = Guid.NewGuid().ToString("N");

            _log.Debug($"Connecting to: {address}:{port}");
            _manager.Connect(address, port, tempId);
        }

        protected override void OnNetworkDataReceived(NetPeer peer, NetPacketReader reader, byte channel, DeliveryMethod deliveryMethod)
        {
            var msgType = (NetMessageType)reader.GetByte();

            _log.Info($"Got {msgType.ToString()} message ({deliveryMethod.ToString()}) from {peer.Id}");

            switch (msgType)
            {
                case (NetMessageType.ConnectionAccepted):
                    var myId = reader.GetUShort();
                    _log.Debug($"Server accepted connection, my id is {myId}...");
                    var client = new Client()
                    {
                        Id = myId,
                        Name = Guid.NewGuid().ToString("N")
                    };
                    SendClientInfo(client);
                    break;
            }

            reader.Recycle();
            _log.Info($"{deliveryMethod.ToString()} message from {peer.Id}");
        }

        protected void SendClientInfo(Client client)
        {
            _log.Debug($"Sending client data ({client.Name}) to server...");
            SendDataToPeer(client, _manager.FirstPeer, NetMessageType.ClientDetails, DeliveryMethod.ReliableOrdered);
        }

        
    }
}
