using FlatRedBall;
using FlatRedBall.AI.Pathfinding;
using Microsoft.Xna.Framework;
using System;

namespace NarfoxGameTools.Extensions
{
    public static class MathExtensions
    {
        /// <summary>
        /// Converts a float value from degrees to radians.
        /// </summary>
        /// <param name="degrees">A degrees value as a float</param>
        /// <returns>The value in radians</returns>
        public static float ToRadians(this float degrees)
        {
            return degrees * (float)(Math.PI / 180f);
        }

        /// <summary>
        /// Converts a float value from radians to degrees.
        /// </summary>
        /// <param name="degrees">A radians value as a float</param>
        /// <returns>The value in degrees</returns>
        public static float ToDegrees(this float degrees)
        {
            return degrees * (float)(180f / Math.PI);
        }

        /// <summary>
        /// Clamps a float value to be within the min and max parameters.
        /// </summary>
        /// <param name="f">The float value to clamp</param>
        /// <param name="min">The minimum value to clamp to</param>
        /// <param name="max">The maximum value to clamp to</param>
        /// <returns>A float that falls within the provided range</returns>
        public static float Clamp(this float f, float min, float max)
        {
            f = Math.Max(min, f);
            f = Math.Min(max, f);
            return f;
        }

        public static byte Clamp(this float f)
        {
            return (byte)f.Clamp(0, 255);
        }

        public static float DistanceTo(this PositionedObject o1, PositionedObject o2)
        {
            return o1.Position.ToVector2().DistanceTo(o2.Position.ToVector2());
        }

        public static float DistanceTo(this PositionedObject o1, PositionedNode node)
        {
            return o1.Position.ToVector2().DistanceTo(node.Position.ToVector2());
        }

        public static float DistanceTo(this PositionedObject o1, Vector2 target)
        {
            return o1.Position.ToVector2().DistanceTo(target);
        }

        public static float DistanceTo(this Vector2 vector, Vector2 vector2)
        {
            return (vector2 - vector).Length();
        }

        public static float DistanceTo(this Vector3 vector, Vector3 vector2)
        {
            return vector.ToVector2().DistanceTo(vector2.ToVector2());
        }
    }
}
