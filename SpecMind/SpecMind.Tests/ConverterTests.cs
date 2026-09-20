using System.Globalization;
using Avalonia.Media;
using SpecMind.ViewModels;
using Xunit;

namespace SpecMind.Tests;

public class ConverterTests
{
    [Theory]
    [InlineData("cpu_temp", 59, "Норма", "#9ECE6A")]
    [InlineData("cpu_temp", 60, "Повышена", "#E0AF68")]
    [InlineData("cpu_temp", 80, "Критично", "#F7768E")]
    [InlineData("gpu_temp", 69, "Норма", "#9ECE6A")]
    [InlineData("gpu_temp", 70, "Повышена", "#E0AF68")]
    [InlineData("gpu_temp", 85, "Критично", "#F7768E")]
    [InlineData("usage", 0, "Норма", "#9ECE6A")]
    [InlineData("usage", 70, "Высокая", "#E0AF68")]
    [InlineData("usage", 90, "Критично", "#F7768E")]
    public void StatusTextAndColorAgreeAtThresholds(string kind, double value, string text, string hex)
    {
        Assert.Equal(text, StatusConverter.Instance.Convert(value, typeof(string), kind, CultureInfo.InvariantCulture));
        var brush = Assert.IsType<SolidColorBrush>(StatusColorConverter.Instance.Convert(value, typeof(IBrush), kind, CultureInfo.InvariantCulture));
        Assert.Equal(Color.Parse(hex), brush.Color);
    }

    [Theory]
    [InlineData(true, "Тёмная тема")]
    [InlineData(false, "Светлая тема")]
    [InlineData(null, "Неизвестно")]
    public void ThemeLabels(object? value, string expected)
        => Assert.Equal(expected, BoolToStringConverter.Instance.Convert(value, typeof(string), null, CultureInfo.InvariantCulture));

    [Theory]
    [InlineData("themes", "themes", true)]
    [InlineData("themes", "general", false)]
    [InlineData(null, "themes", false)]
    public void CategoryVisibilityAndHighlightAgree(string? selected, string category, bool active)
    {
        Assert.Equal(active, CategoryToVisibilityConverter.Instance.Convert(selected, typeof(bool), category, CultureInfo.InvariantCulture));
        Assert.Equal(active ? Color.Parse("#7AA9E0") : Colors.Transparent,
            CategoryToBackgroundConverter.Instance.Convert(selected, typeof(Color), category, CultureInfo.InvariantCulture));
    }
}
