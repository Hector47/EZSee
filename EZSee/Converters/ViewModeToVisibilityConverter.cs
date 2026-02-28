using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using EZSee.Models;

namespace EZSee.Converters
{
    public class ViewModeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ViewMode mode && parameter is string targetMode)
            {
                if (Enum.TryParse<ViewMode>(targetMode, out var target))
                    return mode == target ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
