using Microsoft.Xna.Framework;

namespace NarfoxGameTools.Entities
{
    /// <summary>
    /// This is just a Vector that satisfies the IMoveTarget
    /// interface making it easy to assign move targets that are
    /// a simple vector or a full game entity
    /// </summary>
    public class VectorMoveTarget : IMoveTarget
    {
        private Vector3 positionVector;
        private Vector3 velocityVector;

        public float X { get => positionVector.X; set => positionVector.X = value; }
        public float Y { get => positionVector.Y; set => positionVector.Y = value; }
        public float XVelocity { get => velocityVector.X; set => velocityVector.X = value; }
        public float YVelocity { get => velocityVector.Y; set => velocityVector.Y = value; }

        public VectorMoveTarget() { }
        public VectorMoveTarget(float x, float y)
        {
            positionVector.X = x;
            positionVector.Y = y;
        }

        public VectorMoveTarget(Vector3 position)
        {
            this.positionVector = position;
        }
    }
}
