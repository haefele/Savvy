﻿using System.Globalization;

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
    }
}