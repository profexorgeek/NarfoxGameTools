using Narfox.Entities;
using System.Numerics;

namespace Narfox.Extensions
{
    public static class MathExtensions
    {
        public const float PiFloat = (float)Math.PI;
        public const float RadiansPerDegree = (float)(Math.PI / 180f);
        public const float DegreesPerRadian = (float)(180f / Math.PI);
        public const float TwoPi = (float)Math.PI * 2f;
        public const float HalfPi = (float)Math.PI / 2f;

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
            f = Math.Max(min, f);
            f = Math.Min(max, f);
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
            var rounded = (int)Math.Round(doubled, MidpointRounding.AwayFromZero);
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
            return NormalizeAngle((float)Math.Atan2(y - source.Y, x - source.X));
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
            float difference = (target - source + TwoPi) % TwoPi;
            if (difference > PiFloat)
            {
                difference -= TwoPi;
            }
            return difference;
        }


        /// <summary>
        /// Checks if the provided type is a numeric type.
        /// </summary>
        /// <param name="type">An unknown type</param>
        /// <returns>True or false depending on if the provided type is numeric</returns>
        public static bool IsNumericType(Type type)
        {
            return type == typeof(byte) ||
                   type == typeof(sbyte) ||
                   type == typeof(short) ||
                   type == typeof(ushort) ||
                   type == typeof(int) ||
                   type == typeof(uint) ||
                   type == typeof(long) ||
                   type == typeof(ulong) ||
                   type == typeof(float) ||
                   type == typeof(double) ||
                   type == typeof(decimal);
        }

        /// <summary>
        /// Lerps two unknown values if they are numeric and returns the lerped value.
        /// 
        /// If the objects are non-numeric, it will return the toValue.
        /// </summary>
        /// <param name="fromValue">The value to lerp from</param>
        /// <param name="toValue">The value to lerp to</param>
        /// <param name="t">The amount to lerp</param>
        /// <returns>A lerped value if numeric or the "to" value if non-numeric</returns>
        public static object LerpNumeric(object fromValue, object toValue, float t)
        {
            // EARLY OUT: null value or mismatched types returns a null value
            if (fromValue == null || toValue == null)
                return null;

            if(IsNumericType(toValue.GetType()))
            {
                // We'll upcast everything to double for math precision
                double fromDouble = Convert.ToDouble(fromValue);
                double toDouble = Convert.ToDouble(toValue);
                double lerped = fromDouble + (toDouble - fromDouble) * t;

                // Now cast back to the original type
                Type valueType = fromValue.GetType();

                if (valueType == typeof(byte)) return (byte)lerped;
                if (valueType == typeof(sbyte)) return (sbyte)lerped;
                if (valueType == typeof(short)) return (short)lerped;
                if (valueType == typeof(ushort)) return (ushort)lerped;
                if (valueType == typeof(int)) return (int)lerped;
                if (valueType == typeof(uint)) return (uint)lerped;
                if (valueType == typeof(long)) return (long)lerped;
                if (valueType == typeof(ulong)) return (ulong)lerped;
                if (valueType == typeof(float)) return (float)lerped;
                if (valueType == typeof(double)) return lerped;
                if (valueType == typeof(decimal)) return (decimal)lerped;
            }

            return toValue;
        }

        /// <summary>
        /// Gets the delta between two unknown types. If they are numeric it
        /// will upcast them to doubles and return the absolute value of a
        /// subtraction. If they are non-numeric, it returns 0 as the delta.
        /// </summary>
        /// <param name="a">An unknown type that should be numeric</param>
        /// <param name="b">An unknown type that should be numeric</param>
        /// <returns>The absolute value of the delta if arguments are numeric, otherwise returns 0</returns>
        public static double GetDelta(object a, object b)
        {
            if(IsNumericType(a.GetType()) && IsNumericType(b.GetType()))
            {
                return Math.Abs(Convert.ToDouble(a) - Convert.ToDouble(b));
            }

            return 0;
        }
    }
}
