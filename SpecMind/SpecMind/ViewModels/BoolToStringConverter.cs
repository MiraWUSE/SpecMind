using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace SpecMind.ViewModels;

public class BoolToStringConverter : IValueConverter
{
    public static readonly BoolToStringConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isDark)
        {
            return isDark ? "Тёмная тема" : "Светлая тема";
        }
        return "Неизвестно";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}