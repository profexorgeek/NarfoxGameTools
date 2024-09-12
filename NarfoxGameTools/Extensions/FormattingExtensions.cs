using System.Globalization;

namespace NarfoxGameTools.Extensions
{
    public static class FormattingExtensions
    {
        /// <summary>
        /// Formats a floating point number as a whole percent with no decimal places by default.
        /// For example 0.85f becomes 85%.
        /// </summary>
        /// <param name="value">The floating point number to format</param>
        /// <param name="decimals">The number of decimal places</param>
        /// <returns>A string like 0.85f -> "85%"</returns>
        public static string AsPercent(this float value, int decimals = 0)
        {
            return value.ToString($"P{decimals}", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Formats a floating point number as a currency value with two
        /// decimal places by default.
        /// For example 3.85f becomes "$3.85"
        /// </summary>
        /// <param name="value">The floating point number to format</param>
        /// <param name="decimals">The number of decimal places</param>
        /// <returns>A string like 3.85f -> "$3.85"</returns>
        public static string AsCurrency(this float value, int decimals = 2)
        {
            return value.ToString($"C{decimals}", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Formats a floating point number as a whole number string with
        /// commans and no decimal places by default
        /// </summary>
        /// <param name="value">The floating point number to format</param>
        /// <param name="decimals">The number of decimal places</param>
        /// <returns>A string like 4321.05 -> 4,321</returns>
        public static string AsNumber(this float value, int decimals = 0)
        {
            return value.ToString($"N{decimals}", CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Splits an object string into words, uppercases the first letter
        /// of each word, and returns a new string with every word
        /// capitalized.
        /// </summary>
        /// <param name="obj">An object to title case as a string</param>
        /// <returns>A title cased string</returns>
        public static string ToTitleCase(this object obj)
        {
            var titleCase = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(obj.ToString().ToLowerInvariant());
            return titleCase;
        }
    }
}