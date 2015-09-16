using System;
using Windows.UI.Xaml.Data;

namespace Savvy.Converter
{
    public class DecimalToStringWithTwoDecimalsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var dec = (decimal)value;
            return dec.ToString("N");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}