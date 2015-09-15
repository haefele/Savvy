using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace Savvy.Converter
{
    [ContentProperty(Name="Map")]
    public class ObjectToObjectConverter : IValueConverter
    {
        public object DefaultSource { get; set; }
        public object DefaultTarget { get; set; }

        public ObservableCollection<MapItem> Map { get; }

        public ObjectToObjectConverter()
        {
            this.Map = new ObservableCollection<MapItem>();
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            MapItem entry = this.Map.FirstOrDefault(this.MakeMapPredicate(item => item.Source, value));
            return entry == null ? this.DefaultTarget : entry.Target;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            MapItem entry = this.Map.FirstOrDefault(this.MakeMapPredicate(item => item.Target, value));
            return entry == null ? this.DefaultSource : entry.Source;
        }

        #region Private Methods
        private object Coerce(object value, Type targetType)
        {
            if (value == null || targetType == value.GetType())
            {
                return value;
            }
            if (targetType == typeof(string))
            {
                return value.ToString();
            }

            if (targetType.GetTypeInfo().IsEnum && value is string)
            {
                return Enum.Parse(targetType, (string)value, false);
            }
            try
            {
                return System.Convert.ChangeType(value, targetType, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return value;
            }
        }
        private bool SafeCompare(object left, object right)
        {
            if (left == null)
            {
                if (right == null)
                    return true;

                return right.Equals(null);
            }

            return left.Equals(right);
        }
        private Func<MapItem, bool> MakeMapPredicate(Func<MapItem, object> selector, object value)
        {
            return mapItem =>
            {
                object source = this.Coerce(selector(mapItem), (value ?? string.Empty).GetType());
                return this.SafeCompare(source, value);
            };
        }
        #endregion
    }


    public class MapItem
    {
        public MapItem()
        {
            
        }
        public MapItem(object source, object target)
        {
            this.Source = source;
            this.Target = target;
        }
        public object Source { get; set; }
        public object Target { get; set; }
    }
}