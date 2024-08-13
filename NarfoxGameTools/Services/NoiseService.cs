using SimplexNoise;
using System.Collections.Generic;
using System;

namespace NarfoxGameTools.Services
{
    /// <summary>
    /// This is class is used to specify how to fill an int
    /// grid on a noise pass.
    /// </summary>
    public struct IntGridLayer
    {
        /// <summary>
        /// The integer to fill into the int grid
        /// </summary>
        public uint Integer { get; set; }

        /// <summary>
        /// How dense this integer should be generated
        /// </summary>
        public float Density { get; set; }

        /// <summary>
        /// Noise is generated as bytes, this is the byte value used in a
        /// collapse function that determines whether this int value overrides
        /// whatever may already be in the grid. If the noise value is greater or equal,
        /// this integer will NOT be used. If it is less, this integer will override
        /// whatever was already in the grid.
        /// </summary>
        public byte MaximumCollapseValue => (byte)(Density * byte.MaxValue);

        /// <summary>
        /// The scale of noise to generate when doing this layer pass.
        /// </summary>
        public float NoiseScale { get; set; }

        /// <summary>
        /// The random seed to use in this pass
        /// </summary>
        public int Seed { get; set; }

        public IntGridLayer(uint integer = 0, float density = 0.5f, float noiseScale = 0.5f, int seed = 0)
        {
            Integer = integer;
            Density = density;
            NoiseScale = noiseScale;
            Seed = seed;
        }
    }

    public class NoiseService
    {

        static NoiseService instance;
        Random rand;

        public static NoiseService Instance => instance ?? (instance = new NoiseService());

        private NoiseService() { }

        /// <summary>
        /// Initializes the service and creates a random object to generate
        /// seeds if no seed is passed to individual methods
        /// </summary>
        public void Initialize()
        {
            rand = new Random();
        }

        /// <summary>
        /// Gets a grid of bytes of the provided size.
        /// </summary>
        /// <param name="width">The width of the grid</param>
        /// <param name="height">The height of the grid</param>
        /// <param name="scale">The optional scale of the noise, default is 0.5</param>
        /// <param name="seed">The optional random seed to use</param>
        /// <returns></returns>
        public byte[,] GetNoise(int width, int height, float scale = 0.5f, int? seed = null)
        {
            byte[,] noise = new byte[width, height];
            Noise.Seed = seed ?? rand.Next();
            float[,] simplexNoise = Noise.Calc2D(width, height, scale);
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    // simplex generates values from -1 to 1 but we want a byte so
                    // we convert each value
                    noise[x, y] = (byte)(simplexNoise[x, y]);
                }
            }
            return noise;
        }

        /// <summary>
        /// Creates an int grid using the provided dictionary of passes.
        /// The passes will be done in the order provided by the dictionary.
        /// For each integer provided, a noise grid will be generated and 
        /// </summary>
        /// <param name="passes"></param>
        /// <returns></returns>
        public uint[,] GetIntGrid(int width, int height, List<IntGridLayer> layers)
        {
            var grid = new uint[width, height];
            foreach (var pass in layers)
            {
                var noise = GetNoise(width, height, pass.NoiseScale, pass.Seed);
                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        byte noiseByte = noise[x, y];
                        if (noiseByte < pass.MaximumCollapseValue)
                        {
                            grid[x, y] = pass.Integer;
                        }
                    }
                }
            }
            return grid;
        }
    }
}
