using Microsoft.Xna.Framework;
using System;

namespace NarfoxGameTools.Extensions
{
    public static class RandomExtensions
    {
        public static float InRange(this Random rand, float min, float max)
        {
            // early out for equal. This may not be perfectly accurate
            // but it makes no difference, all it's doing is saving
            // a calculation
            if (min == max)
            {
                return max;
            }

            var range = max - min;
            var randInRange = (float)(rand.NextDouble() * range);
            return min + randInRange;
        }

        public static float Sign(this Random rand)
        {
            return rand.NextDouble() < 0.5 ? 1f : -1f;
        }

        public static Color Color(this Random rand, float minTint = 0f, float maxTint = 1f)
        {
            var r = rand.InRange(minTint, maxTint);
            var g = rand.InRange(minTint, maxTint);
            var b = rand.InRange(minTint, maxTint);

            return new Color(r, g, b);
        }

        public static T EnumValue<T>(this Random rand)
        {
            T[] vals = Enum.GetValues(typeof(T)) as T[];
            return vals.Random(rand);
        }

        public static bool Bool(this Random rand)
        {
            if(rand.NextDouble() < 0.5)
            {
                return true;
            }
            return false;
        }
    }
}
