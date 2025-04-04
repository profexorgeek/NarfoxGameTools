using Narfox.Entities;
using System.Numerics;

namespace Narfox.Extensions
{
    public static class MathExtensions
    {
        public const float PiFloat = (float)MathF.PI;
        public const float RadiansPerDegree = (float)(MathF.PI / 180f);
        public const float DegreesPerRadian = (float)(180f / MathF.PI);
        public const float TwoPi = MathF.Tau;
        public const float HalfPi = MathF.PI / 2f;

        /// <summary>
        /// Converts a float value from degrees to radians.
        /// </summary>
        /// <param name="degrees">A degrees value as a float</param>
        /// <returns>The value in radians</returns>
        public static float ToRadians(this float degrees)
        {
            return degrees * RadiansPerDegree;
        }

        /// <summary>
        /// Converts a float value from radians to degrees.
        /// </summary>
        /// <param name="radians">A radians value as a float</param>
        /// <returns>The value in degrees</returns>
        public static float ToDegrees(this float radians)
        {
            return radians * DegreesPerRadian;
        }

        /// <summary>
        /// Normalizes an angle to be between 0 and TwoPi
        /// </summary>
        /// <param name="radians">The angle to normalize</param>
        /// <returns>A normalized angle</returns>
        public static float NormalizeAngle(float angle)
        {
            return (angle % TwoPi + TwoPi) % TwoPi;
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
            f = MathF.Max(min, f);
            f = MathF.Min(max, f);
            return f;
        }

        /// <summary>
        /// Clamps an int value to be within the min and max parameters.
        /// </summary>
        /// <param name="i">The int value to clamp</param>
        /// <param name="min">The minimum value to clamp to</param>
        /// <param name="max">The maximum value to clamp to</param>
        /// <returns>An int that falls within the provided range</returns>
        public static int Clamp(this int i, int min, int max)
        {
            i = System.Math.Max(min, i);
            i = System.Math.Min(max, i);
            return i;
        }

        /// <summary>
        /// Clamps a float value into byte range within min and max.
        /// </summary>
        /// <param name="f">The float to clamp</param>
        /// <returns>A byte within the specified range</returns>
        public static byte Clamp(this float f)
        {
            return (byte)f.Clamp(0, 255);
        }

        /// <summary>
        /// Rounds a provided floating point number to the nearest 0.5 using
        /// AwayFromZero rounding.
        /// 
        /// This is useful for finding the pixel-perfect midpoint of a rectangle
        /// that has an odd height or width - requiring the midpoint to be at
        /// exactly a half pixel.
        /// </summary>
        /// <param name="val">A float to be rounded</param>
        /// <returns>The number, rounded to the nearest 0.5</returns>
        public static float RoundToNearestHalf(this float val)
        {
            var doubled = val * 2;
            var rounded = (int)MathF.Round(doubled, MidpointRounding.AwayFromZero);
            return rounded / 2f;
        }


        /// <summary>
        /// Finds the absolute distance between the source vector and the
        /// provided target coordinates
        /// </summary>
        /// <param name="source">A Vector2 source</param>
        /// <param name="x">The target X coords</param>
        /// <param name="y">The target Y coord</param>
        /// <returns>The distance between the points</returns>
        public static float DistanceTo(this Vector2 source, float x, float y)
        {
            return (new Vector2(x, y) - source).Length();
        }

        /// <summary>
        /// Finds the absolute distance between the source vector and the
        /// provided target coordinates
        /// </summary>
        /// <param name="source">A Vector2 source</param>
        /// <param name="target">A Vector2 target</param>
        /// <returns>The distance between the points</returns>
        public static float DistanceTo(this Vector2 source, Vector2 target)
        {
            return (target - source).Length();
        }

        /// <summary>
        /// Finds the absolute distance between the source entity and the provided target
        /// </summary>
        /// <param name="source">An IPositionedEntity</param>
        /// <param name="target">A target</param>
        /// <returns>The distance from source to target</returns>
        public static float DistanceTo(this IPositionedEntity source, IPositionedEntity target)
        {
            return DistanceTo(source, target.X, target.Y);
        }

        /// <summary>
        /// Finds the absolute distance between the source entity and the provided
        /// coordinages
        /// </summary>
        /// <param name="source">A source entity</param>
        /// <param name="x">An x coord</param>
        /// <param name="y">A y coord</param>
        /// <returns></returns>
        public static float DistanceTo(this IPositionedEntity source, float x, float y)
        {
            return (new Vector2(x, y) - new Vector2(source.X, source.Y)).Length();
        }


        /// <summary>
        /// Returns a vector where the provided entity will be in some amount of
        /// seconds based on its current velocity
        /// </summary>
        /// <param name="entity">The entity to check</param>
        /// <param name="seconds">The amont of seconds to simulate</param>
        /// <returns>An estimated future position as a Vector2</returns>
        public static Vector2 GetPositionInTime(this IPositionedEntity entity, float seconds)
        {
            var dx = entity.VelocityX * seconds;
            var dy = entity.VelocityY * seconds;
            return new Vector2(entity.X + dx, entity.Y + dy);
        }

        /// <summary>
        /// Returns the rotation from the source to the target
        /// </summary>
        /// <param name="source">An entity to rotate</param>
        /// <param name="target">A target the entity should rotate to point towards</param>
        /// <returns>The rotation radians that will point the entity at the
        /// target</returns>
        public static float RotationTo(this IPositionedEntity source, IPositionedEntity target)
        {
            return RotationTo(source, target.X, target.Y);
        }

        /// <summary>
        /// Returns the rotation from the source to the target
        /// </summary>
        /// <param name="source">An entity to rotate</param>
        /// <param name="target">A target the entity should rotate to point towards</param>
        /// <returns>The rotation radians that will point the entity at the
        /// target</returns>
        public static float RotationTo(this IPositionedEntity source, float x, float y)
        {
            return NormalizeAngle(MathF.Atan2(y - source.Y, x - source.X));
        }


        /// <summary>
        /// Finds the shortest rotation (positive or negative) from the source angle
        /// to the target angle.
        /// </summary>
        /// <param name="source">The source angle</param>
        /// <param name="target">The target angle</param>
        /// <returns>The amount of radians that, when added to the source, will be
        /// the shortest rotation to match the target.</returns>
        public static float ShortestRotationTo(this float source, float target)
        {
            float difference = (target - source + MathF.Tau) % MathF.Tau;
            if (difference > MathF.PI)
            {
                difference -= MathF.Tau;
            }
            return difference;
        }
    }
}
