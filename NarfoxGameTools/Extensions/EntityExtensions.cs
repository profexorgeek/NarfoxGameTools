using FlatRedBall.Math;
using Microsoft.Xna.Framework;
using System;

namespace NarfoxGameTools.Extensions
{
    public static class EntityExtensions
    {
        /// <summary>
        /// The 2D distance from one positioned object to another.
        /// 
        /// Note that Z-space is not considered.
        /// </summary>
        /// <param name="entity">The first positioned object</param>
        /// <param name="target">The second positioned object</param>
        /// <returns>The linear distance (Vector hypotenuse) between the objects</returns>
        public static float DistanceTo(this IPositionable entity, IPositionable target)
        {
            return DistanceTo(entity, target.X, target.Y);
        }

        /// <summary>
        /// The 2D distance from one positioned object to a 2D x and y
        /// </summary>
        /// <param name="entity">The first positioned object</param>
        /// <param name="x">The x position</param>
        /// <param name="y">The y position</param>
        /// <returns>The linear distance (Vector hypotenuse) between the object and x,y set</returns>
        public static float DistanceTo(this IPositionable entity, float x, float y)
        {
            return new Vector2(x - entity.X, y - entity.Y).Length();
        }

        /// <summary>
        /// The 2D distance from one positioned object to a 3d vector.
        /// 
        /// Note that this discards the Z component and only measures X/Y distance
        /// </summary>
        /// <param name="entity">The first positioned object</param>
        /// <param name="position">The position to test</param>
        /// <returns>The linear distance (Vector hypotenuse) between the object and x,y set</returns>
        public static float DistanceTo(this IPositionable entity, Vector3 position)
        {
            return DistanceTo(entity, position.X, position.Y);
        }

        /// <summary>
        /// A cheap way to measure how long it takes for a given drag to reduce
        /// velocity to zero. This is helpful for determining when a bot should
        /// quit accelerating to arrive near a specific point.
        /// 
        /// Note that this is only crudely accurate for a range of speeds.
        /// </summary>
        /// <param name="velocityLength">The linear velocity/length/vector magnitude</param>
        /// <param name="drag">The applied drag</param>
        /// <returns>And estimated distance to stop</returns>
        public static float DistanceToStop(float velocityLength, float drag)
        {
            // NOTE: a more precise calculation involves simulating out
            // the drag each frame. However, for most velocities used in
            // games it is precise enough to do this fast and simple calculation
            return velocityLength / drag;
        }

        /// <summary>
        /// The target rotation for the provided entity to face the provided target.
        /// Often used to make something such as a turret rotate to point at a target.
        /// </summary>
        /// <param name="entity">The entity to test</param>
        /// <param name="target">The entity's target</param>
        /// <returns>The rotation angle in radians from entity to target</returns>
        public static float RotationTo(this IPositionable entity, IPositionable target)
        {
            return RotationTo(entity, target.X, target.Y);
        }

        /// <summary>
        /// The target rotation for the provided entity to face the provided coordinates.
        /// Often used to make something such as a turret rotate to point at a target.
        /// </summary>
        /// <param name="entity">The entity to test</param>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <returns>The rotation angle in radians from entity to coordinates</returns>
        public static float RotationTo(this IPositionable entity, float x, float y)
        {
            var dx = x - entity.X;
            var dy = y - entity.Y;
            var rot = (float)Math.Atan2(dy, dx);

            // NOTE: destination distance CAN be infinity. If so rotation would be NaN so we override this
            return float.IsNaN(rot) ? 0 : rot;
        }

        /// <summary>
        /// The target rotation for the provided entity to face the provided coordinates.
        /// Often used to make something such as a turret rotate to point at a target.
        /// </summary>
        /// <param name="v1">The origin vector</param>
        /// <param name="v2">The target vector</param>
        /// <returns>The rotation angle in radians from v1 to v2</returns>
        public static float RotationTo(this Vector3 v1, Vector3 v2)
        {
            var delta = v2 - v1;
            return (float)Math.Atan2(delta.Y, delta.X);
        }
    }
}
