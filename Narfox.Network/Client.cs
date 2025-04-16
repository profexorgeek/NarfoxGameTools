using Narfox.Logging;

namespace Narfox.Network
{
    public class Client : NetBase
    {
        public Client(ILogger logger)
            : base(logger)
        {
            _log.Debug("Starting client...");
            _manager.Start();
        }

        public void Connect(string address, ushort port)
        {
            _log.Debug($"Connecting to: {address}:{port}");
            _manager.Connect(address, port, "Hi I'm client!");
        }
    }
}
