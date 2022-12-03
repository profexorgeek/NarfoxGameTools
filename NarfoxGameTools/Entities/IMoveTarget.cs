namespace NarfoxGameTools.Entities
{
    public interface IMoveTarget
    {
        float X { get; }
        float Y { get; }
        float XVelocity { get; }
        float YVelocity { get; }
    }
}
