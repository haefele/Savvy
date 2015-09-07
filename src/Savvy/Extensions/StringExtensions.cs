using System;
using System.Globalization;

namespace Savvy.Extensions
{
    public static class StringExtensions
    {
        public static decimal? ToDecimal(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            value = value.Replace(',', '.');

            decimal output;

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out output))
                return output;

            return null;
        }

        public static string NormalizePath(this string value)
        {
            return value.Replace('\\', '/');
        }

        public static bool Contains(this string text, string compareTo, StringComparison comparison)
        {
            return text.IndexOf(compareTo ?? String.Empty, comparison) > -1;
        }

        public static bool StartsWith(this string text, string compareTo, StringComparison comparison)
        {
            return text.IndexOf(compareTo ?? string.Empty, comparison) == 0;
        }
    }
}