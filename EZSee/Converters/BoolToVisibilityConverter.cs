using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EZSee.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool boolValue = value is bool b && b;
            if (parameter is string s && s == "Invert")
                boolValue = !boolValue;
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = value is Visibility v && v == Visibility.Visible;
            if (parameter is string s && s == "Invert")
                result = !result;
            return result;
        }
    }
}
