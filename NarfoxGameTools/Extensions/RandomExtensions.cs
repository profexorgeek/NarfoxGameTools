using Microsoft.Xna.Framework;
using System;

namespace NarfoxGameTools.Extensions
{
    public static class RandomExtensions
    {
        /// <summary>
        /// A float-precision version of PI
        /// </summary>
        public static float Pi => (float)Math.PI;

        /// <summary>
        /// Generates a random float between min and max
        /// </summary>
        /// <param name="rand">The random instance to use</param>
        /// <param name="min">The inclusive minimum value</param>
        /// <param name="max">The exclusive maximum value</param>
        /// <returns>A float within the provided range</returns>
        public static float InRange(this Random rand, double min, double max)
        {
            return rand.InRange((float)min, (float)max);
        }

        /// <summary>
        /// Generates a random float between min and max
        /// </summary>
        /// <param name="rand">The random instance to use</param>
        /// <param name="min">The inclusive minimum value</param>
        /// <param name="max">The exclusive maximum value</param>
        /// <returns>A float within the provided range</returns>
        public static float InRange(this Random rand, float min, float max)
        {
            // early out for equal, we return min because the return value is
            // supposed to be min inclusive but max exclusive. So returning min
            // is slightly more semantically accurate but the values are identical
            // so it doesn't actually matter.
            if (min == max)
            {
                return min;
            }

            var range = max - min;
            var randInRange = (float)(rand.NextDouble() * range);
            return min + randInRange;
        }

        /// <summary>
        /// Generates a random position within a radius with adjustment for
        /// even distribution
        /// </summary>
        /// <param name="rand">The random instance to use</param>
        /// <param name="radius">The radius to generate points within</param>
        /// <returns>A Vector2 position</returns>
        public static Vector2 PositionInRadius(this Random rand, float radius)
        {
            var coefficient = (float)Math.Sqrt(rand.NextDouble());
            var magnitude = coefficient * radius;
            var angle = rand.InRange(0, 2 * Pi);

            var vector = new Vector2(
                (float)(Math.Cos(angle) * magnitude),
                (float)(Math.Sin(angle) * magnitude));
            return vector;
        }

        /// <summary>
        /// Generates a random position within a 3D cube with 0 at center
        /// </summary>
        /// <param name="rand">The random instance to use</param>
        /// <param name="cubeSize">The size of the cuber</param>
        /// <returns>A random Vector3 within the cube</returns>
        public static Vector3 PositionInCube(this Random rand, float cubeSize)
        {
            var halfSize = cubeSize / 2f;
            return new Vector3(
                rand.InRange(-halfSize, halfSize),
                rand.InRange(-halfSize, halfSize),
                rand.InRange(-halfSize, halfSize));
        }

        /// <summary>
        /// Generates a random positive or negative sign
        /// </summary>
        /// <param name="rand">The random instance to use</param>
        /// <returns></returns>
        public static float Sign(this Random rand)
        {
            return rand.NextDouble() < 0.5 ? 1f : -1f;
        }

        /// <summary>
        /// Generates a random Color with an optional maximum and minimum tint
        /// </summary>
        /// <param name="rand">The random instance to use</param>
        /// <param name="minIntensity">The maximum channel intensity between 0-1 where 0 is pure black</param>
        /// <param name="maxIntensity">The minimum channel intensity between 0-1 where 1 is pure white</param>
        /// <returns>A random color where each channel is within the provided intensity</returns>
        public static Color Color(this Random rand, float minIntensity = 0f, float maxIntensity = 1f)
        {
            minIntensity = minIntensity.ClampTo(0f, 1f);
            maxIntensity = maxIntensity.ClampTo(0f, 1f);

            var r = rand.InRange(minIntensity, maxIntensity);
            var g = rand.InRange(minIntensity, maxIntensity);
            var b = rand.InRange(minIntensity, maxIntensity);

            return new Color(r, g, b);
        }

        /// <summary>
        /// Chooses a random value within an enum
        /// </summary>
        /// <typeparam name="T">The type of enum</typeparam>
        /// <param name="rand">The random instance to use</param>
        /// <returns>A random value from the enum</returns>
        public static T EnumValue<T>(this Random rand)
        {
            T[] vals = Enum.GetValues(typeof(T)) as T[];
            return vals.Random(rand);
        }

        /// <summary>
        /// The Fisher-Yates or Knuth shuffle to randomize a small
        /// array. Note that this mutates the array!
        /// </summary>
        /// <typeparam name="T">The type of array</typeparam>
        /// <param name="rand">The random instance to use</param>
        /// <param name="array">An array to shuffle, which will be mutated</param>
        public static void Shuffle<T>(this Random rand, T[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1); // Pick a random index from 0 to i
                (array[i], array[j]) = (array[j], array[i]); // Swap elements
            }
        }

        /// <summary>
        /// Generates a random boolean
        /// </summary>
        /// <param name="rand">The random instance to use</param>
        /// <returns>True or false at random</returns>
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
