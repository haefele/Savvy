using System;
using Windows.UI.Xaml.Data;

namespace Savvy.Converter
{
    public class BooleanToObjectConverter : IValueConverter
    {
        public object NullValue { get; set; }
        public object TrueValue { get; set; }
        public object FalseValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool? == false)
                return this.NullValue;

            var boolValue = (bool?)value;
            
            if (boolValue == true)
                return this.TrueValue;

            if (boolValue == false)
                return this.FalseValue;

            return this.NullValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException();
        }
    }
}