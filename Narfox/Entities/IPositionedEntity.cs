namespace Narfox.Entities;

/// <summary>
/// This interface guarantees an implementing entity
/// will have a current position, velocity, and rotation
/// </summary>
public interface IPositionedEntity
{
    public float X { get; set; }

    public float Y { get; set; }

    public float VelocityX { get; set; }

    public float VelocityY { get; set; }

    public float RotationZ { get; set; }

    public float Drag { get; set; }
}
