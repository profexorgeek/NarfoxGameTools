using FlatRedBall.Math;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NarfoxGameTools.Extensions
{
    public static class EntityExtensions
    {
        public static float DistanceTo(this IPositionable entity, IPositionable target)
        {
            return DistanceTo(entity, target.X, target.Y);
        }

        public static float DistanceTo(this IPositionable entity, float x, float y)
        {
            return new Vector2(x - entity.X, y - entity.Y).Length();
        }

        public static float DistanceToStop(float velocityLength, float drag)
        {
            // NOTE: a more precise calculation involves simulating out
            // the drag each frame. However, for most velocities used in
            // games it is precise enough to do this fast and simple calculation
            return velocityLength / drag;
        }

        public static float RotationTo(this IPositionable entity, IPositionable target)
        {
            return RotationTo(entity, target.X, target.Y);
        }

        public static float RotationTo(this IPositionable entity, float x, float y)
        {
            var dx = x - entity.X;
            var dy = y - entity.Y;
            var rot = (float)Math.Atan2(dy, dx);

            // NOTE: destination distance CAN be infinity. If so rotation would be NaN so we override this
            return float.IsNaN(rot) ? 0 : rot;
        }

        public static float RotationTo(this Vector3 v1, Vector3 v2)
        {
            var delta = v2 - v1;
            return (float)Math.Atan2(delta.Y, delta.X);
        }
    }
}
