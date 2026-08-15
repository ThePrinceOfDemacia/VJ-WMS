using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VjWms.Desktop.UI.Converters;

public class ZeroToVisibleConverter : IValueConverter
{
    public object Translate(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count)
        {
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Translate(value, targetType, parameter, culture);
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class NonEmptyToVisibleConverter : IValueConverter
{
    public object Translate(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return string.IsNullOrWhiteSpace(str) ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Translate(value, targetType, parameter, culture);
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class InverseBoolToVisConverter : IValueConverter
{
    public object Translate(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return b ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Visible; // Fallback
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Translate(value, targetType, parameter, culture);
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
