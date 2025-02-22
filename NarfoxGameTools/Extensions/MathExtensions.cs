﻿using FlatRedBall;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Math;
using FlatRedBall.Math.Geometry;
using Microsoft.Xna.Framework;
using System;

namespace NarfoxGameTools.Extensions
{
    public static class MathExtensions
    {
        public const float RadiansPerDegree = (float)(Math.PI / 180f);
        public const float DegreesPerRadian = (float)(180f / Math.PI);
        public const float PiAsFloat = (float)Math.PI;
        public const float TwoPi = PiAsFloat * 2f;
        public const float HalfPi = PiAsFloat / 2f;

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
        /// Normalizes an angle to be between 0 and Two Pi
        /// </summary>
        /// <param name="radians">The angle to normalize</param>
        /// <returns>A normalized angle</returns>
        public static float NormalizeAngle(this float radians)
        {
            return FlatRedBall.Math.MathFunctions.RegulateAngle(radians);
        }

        /// <summary>
        /// Clamps a float value to be within the min and max parameters.
        /// </summary>
        /// <param name="f">The float value to clamp</param>
        /// <param name="min">The minimum value to clamp to</param>
        /// <param name="max">The maximum value to clamp to</param>
        /// <returns>A float that falls within the provided range</returns>
        public static double ClampTo(this double d, double min, double max)
        {
            d = Math.Max(min, d);
            d = Math.Min(max, d);
            return d;
        }
        public static float ClampTo(this float f, float min, float max)
        {
            f = Math.Max(min, f);
            f = Math.Min(max, f);
            return f;
        }
        public static int ClampTo(this int i, int min, int max)
        {
            i = Math.Max(min, i);
            i = Math.Min(max, i);
            return i;
        }
        public static byte ClampTo(this float f)
        {
            return (byte)f.ClampTo(0, 255);
        }

        /// <summary>
        /// Rounds a provided floating point number to the nearest 0.5.
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


        public static Vector2 PositionAsVector2(this IPositionable entity)
        {
            Vector2 returnVector;
            if (entity != null)
            {
                returnVector = new Vector2(entity.X, entity.Y);
            }
            else
            {
                returnVector = Vector2.Zero;
            }
            return returnVector;
        }

        public static Vector2 GetPositionInTime(this IPositionable entity, float seconds)
        {
            var dx = entity.XVelocity * seconds;
            var dy = entity.YVelocity * seconds;
            return new Vector2(entity.X + dx, entity.Y + dy);
        }

        public static float RotationTo(this PositionedObject source, PositionedObject target)
        {
            return RotationTo(source, target.X, target.Y);
        }
        public static float RotationTo(this PositionedObject source, float x, float y)
        {
            return (float)MathFunctions.RegulateAngle(Math.Atan2(y - source.Y, x - source.X));
        }

        public static float ShortestRotationDelta(this float angle1, float angle2)
        {
            return MathFunctions.AngleToAngle(angle1, angle2);
        }
    }
}
