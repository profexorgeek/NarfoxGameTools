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

        /// <summary>
        /// Scales the sprite to the specified texture scale
        /// </summary>
        /// <param name="sprite">The sprite to scale</param>
        /// <param name="scaleFactor">The scale factor to apply</param>
        public static void ScaleTo(this Sprite sprite, float scaleFactor)
        {
            ScaleTo(sprite, scaleFactor, scaleFactor);
        }

        /// <summary>
        /// Scales the sprite to the specified texture scale
        /// </summary>
        /// <param name="sprite">The sprite to scale</param>
        /// <param name="xScale">The x scale to apply</param>
        /// <param name="yScale">The y scale to apply</param>
        public static void ScaleTo(this Sprite sprite, float xScale, float yScale)
        {
            // first make sure the texture starts at 1x scale
            sprite.TextureScale = 1f;

            // now apply scale factors
            sprite.Width *= xScale;
            sprite.Height *= yScale;
        }
    }
}
