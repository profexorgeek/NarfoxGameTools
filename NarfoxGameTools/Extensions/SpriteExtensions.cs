using FlatRedBall;
using FlatRedBall.Graphics;
using Microsoft.Xna.Framework;

namespace NarfoxGameTools.Extensions
{
    public static class SpriteExtensions
    {
        /// <summary>
        /// Helper method to colorize a sprite in a single line.
        /// </summary>
        /// <param name="sprite">The sprite to colorize</param>
        /// <param name="color">The color to apply</param>
        /// <param name="preserveAlpha">whether to preserve the sprite's alpha, defaults to false</param>
        /// <param name="operation">The color operation, defaults to Modulate (multiply)</param>
        public static void Colorize(this Sprite sprite, 
            Color color,
            bool preserveAlpha = false,
            ColorOperation operation = ColorOperation.Modulate)
        {
            sprite.ColorOperation = operation;
            sprite.Red = (float)(color.R / 255f);
            sprite.Green = (float)(color.G / 255f);
            sprite.Blue = (float)(color.B / 255f);

            if (!preserveAlpha)
            {
                sprite.Alpha = (float)(color.A / 255f);
            }
        }

        /// <summary>
        /// Returns a sprite's colorization as a Color
        /// </summary>
        /// <param name="sprite">The sprite to test</param>
        /// <returns>The colorization applied to the sprite as a Color</returns>
        public static Color GetColor(this Sprite sprite)
        {
            return new Color(sprite.Red, sprite.Green, sprite.Blue);
        }
    }
}
