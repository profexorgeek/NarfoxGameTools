namespace Narfox.Network.Enums;

/// <summary>
/// This enum is for "primative" message types used by this
/// library for connection forming and other meta information.
/// 
/// Once a set of connected peers is established, most messages
/// should be implementation-specific DataPayload messages that
/// carry information unique to the game that implements this lib
/// </summary>
public enum NetMessageType
{
    ConnectionAccepted = 0,
    ClientDetails = 1,
    EntityData = 2,
}
