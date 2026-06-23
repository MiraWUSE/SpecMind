#pragma warning disable CS8765, CS8600, CS8601, CS8602, CS8603, CS8604
using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace SpecMind.ViewModels;

public class StatusConverter : IValueConverter
{
    public static readonly StatusConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double temp && parameter is string type)
        {
            if (type == "cpu_temp")
            {
                if (temp < 60) return "Норма";
                if (temp < 80) return "Повышена";
                return "Критично";
            }
            else if (type == "gpu_temp")
            {
                if (temp < 70) return "Норма";
                if (temp < 85) return "Повышена";
                return "Критично";
            }
            else if (type == "usage")
            {
                if (temp < 70) return "Норма";
                if (temp < 90) return "Высокая";
                return "Критично";
            }
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StatusColorConverter : IValueConverter
{
    public static readonly StatusColorConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double temp && parameter is string type)
        {
            if (type == "cpu_temp")
            {
                if (temp < 60) return new SolidColorBrush(Color.FromRgb(158, 206, 106));
                if (temp < 80) return new SolidColorBrush(Color.FromRgb(224, 175, 104));
                return new SolidColorBrush(Color.FromRgb(247, 118, 142));
            }
            else if (type == "gpu_temp")
            {
                if (temp < 70) return new SolidColorBrush(Color.FromRgb(158, 206, 106));
                if (temp < 85) return new SolidColorBrush(Color.FromRgb(224, 175, 104));
                return new SolidColorBrush(Color.FromRgb(247, 118, 142));
            }
            else if (type == "usage")
            {
                if (temp < 70) return new SolidColorBrush(Color.FromRgb(158, 206, 106));
                if (temp < 90) return new SolidColorBrush(Color.FromRgb(224, 175, 104));
                return new SolidColorBrush(Color.FromRgb(247, 118, 142));
            }
        }
        return new SolidColorBrush(Color.FromRgb(160, 160, 180));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}