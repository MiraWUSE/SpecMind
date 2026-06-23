#pragma warning disable CS8765, CS8600, CS8601, CS8602, CS8603, CS8604
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace SpecMind.ViewModels;

public class CategoryToVisibilityConverter : IValueConverter
{
    public static readonly CategoryToVisibilityConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string selected && parameter is string category)
        {
            return selected == category;
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}