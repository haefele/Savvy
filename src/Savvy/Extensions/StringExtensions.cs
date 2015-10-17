using System;
using System.Globalization;
using Windows.Globalization.NumberFormatting;

namespace Savvy.Extensions
{
    public static class StringExtensions
    {
        public static decimal? ToDecimal(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var decimalFormatter = new DecimalFormatter();
            return (decimal?)decimalFormatter.ParseDouble(value);
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