using Avalonia.Data.Converters;
using SpecMind.Services;
using System;
using System.Globalization;

namespace SpecMind.ViewModels;

public sealed class TemperatureConverter : IValueConverter
{
    public static readonly TemperatureConverter Instance = new();
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => TemperatureReading.Format(value as double?, "F0", culture);
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
