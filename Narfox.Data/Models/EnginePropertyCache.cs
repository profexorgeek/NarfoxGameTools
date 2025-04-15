namespace Narfox.Data.Models;

/// <summary>
/// A helper struct used to cache the last frame state on
/// a game engine object so engine-driven changes can be
/// merged with non-engine changes such as UI or network
/// data.
/// </summary>
public struct GameEntityFrameCache
{
    /// <summary>
    /// The cached X position
    /// </summary>
    public float X { get; set; }

    /// <summary>
    /// The cached Y position
    /// </summary>
    public float Y { get; set; }

    /// <summary>
    /// The rotation in Radians
    /// </summary>
    public float RotationRadians { get; set; }

    public GameEntityFrameCache(float x, float y, float rotationRadians)
    {
        X = x;
        Y = y;
        RotationRadians = rotationRadians;
    }
}
