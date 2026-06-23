using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace SpecMind.ViewModels;

public class CategoryToBackgroundConverter : IValueConverter
{
    public static readonly CategoryToBackgroundConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string selected && parameter is string category)
        {
            // Если категория выбрана - возвращаем акцентный цвет, иначе прозрачный
            return selected == category ?
                (object)Color.Parse("#7AA9E0") :
                (object)Colors.Transparent;
        }
        return Colors.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}