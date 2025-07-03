using System.Drawing;

namespace Narfox.Extensions;

public static class ColorExtensions
{
    /// <summary>
    /// Converts the provided color into a six-digit hexidecimal string
    /// such as used in CSS and graphics programs.
    /// </summary>
    /// <param name="color">The color to convert</param>
    /// <returns>A six-digit hex string with no prefix</returns>
    public static string ToHexString(this Color color)
    {
        return $"{color.R:X2}{color.G:X2}{color.B:X2}";
    }
    
    /// <summary>
    /// Converts a 3 or 6 character hexidecimal string with no prefix to an RGB Color.
    /// Does not support alpha as part of the hex string.
    ///
    /// Valid examples are "F90" (CSS shorthand) or "FF9900" (full hex string)
    /// 
    /// Will return black and debug assert if bad values are provided.
    /// </summary>
    /// <param name="str">A valid, 6-digit hex string</param>
    /// <returns>The color from the provided string, will return pure black on error.</returns>
    public static Color HexStringToColor(this string str)
    {
        Color outColor = Color.Black;
        
        // strip hex notation
        if(str.StartsWith("0x"))
            str = str.Replace("0x", "");

        if (String.IsNullOrWhiteSpace(str) == false)
        {
            if (str.Length == 3)
            {
                var r = str.Substring(0, 1);
                var g = str.Substring(1, 1);
                var b = str.Substring(2, 1);
                str = r + r + g + g + b + b;
            }

            if (str.Length == 6)
            {
                try
                {
                    var R = Convert.ToInt16(str.Substring(0, 2), 16);
                    var G = Convert.ToInt16(str.Substring(2, 2), 16);
                    var B = Convert.ToInt16(str.Substring(4, 2), 16);
                    outColor = Color.FromArgb(byte.MaxValue, R, G, B);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.Assert(false, $"Bad values were provided to color conversion: {e.Message}");
                }

            }
            else
            {
                System.Diagnostics.Debug.Assert(false, $"Bad hex string provided: {str}");
            }
        }

        return outColor;
    }
    
    /// <summary>
    /// Tints a color by the provided percent, which can be
    /// negative or positive.
    /// 
    /// A positive value will darken the color, a negative value will lighten it.
    /// </summary>
    /// <param name="color">The color to tint.</param>
    /// <param name="percent">The tint amount as a percent from -1 to 1</param>
    /// <returns>A tinted color</returns>
    public static Color Tint(this Color color, float percent)
    {
        var R = (byte)(color.R * (1f - percent)).Clamp(0, 255);
        var G = (byte)(color.G * (1f - percent)).Clamp(0, 255);
        var B = (byte)(color.B * (1f - percent)).Clamp(0, 255);
        var A = color.A;
        return Color.FromArgb(A, R, G, B);
    }
}