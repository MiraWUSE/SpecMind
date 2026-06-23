using Avalonia;
using Avalonia.Media;
using SpecMind.Models;
using System;
using System.Collections.Generic;

namespace SpecMind.Services;

public class ThemeService
{
    private static readonly Dictionary<string, AppTheme> _themes = new();
    private static AppTheme _currentTheme;

    public static AppTheme CurrentTheme => _currentTheme;

    static ThemeService()
    {
        InitializeThemes();
    }

    private static void InitializeThemes()
    {
        _themes["nightowl"] = new AppTheme
        {
            Name = "Night Owl",
            Icon = "🌙",
            BackgroundPrimary = "#1A1B26",
            BackgroundSecondary = "#24253A",
            Border = "#2F3146",
            AccentPrimary = "#7AA9E0",
            AccentHover = "#9DBFE8",
            TextHeading = "#E4E6F1",
            TextPrimary = "#A9B1D6",
            StatusGood = "#9ECE6A",
            StatusWarning = "#E0AF68",
            StatusCritical = "#F7768E",
            IsDark = true
        };

        _themes["daylight"] = new AppTheme
        {
            Name = "Daylight",
            Icon = "☀️",
            BackgroundPrimary = "#F5F7FA",
            BackgroundSecondary = "#FFFFFF",
            Border = "#E1E4E8",
            AccentPrimary = "#0366D6",
            AccentHover = "#0256B9",
            TextHeading = "#24292E",
            TextPrimary = "#586069",
            StatusGood = "#28A745",
            StatusWarning = "#F0AD4E",
            StatusCritical = "#DC3545",
            IsDark = false
        };

        _themes["cyberpunk"] = new AppTheme
        {
            Name = "Cyberpunk",
            Icon = "🌆",
            BackgroundPrimary = "#0D0221",
            BackgroundSecondary = "#1A0533",
            Border = "#FF00FF",
            AccentPrimary = "#00FFFF",
            AccentHover = "#FF00FF",
            TextHeading = "#FFFFFF",
            TextPrimary = "#00FFFF",
            StatusGood = "#00FF00",
            StatusWarning = "#FFFF00",
            StatusCritical = "#FF0000",
            IsDark = true
        };

        _currentTheme = _themes["nightowl"];
    }

    public static List<AppTheme> GetAvailableThemes()
    {
        return new List<AppTheme>(_themes.Values);
    }

    public static void ApplyTheme(string themeKey)
    {
        if (_themes.TryGetValue(themeKey, out var theme))
        {
            ApplyTheme(theme);
        }
    }

    public static void ApplyTheme(AppTheme theme)
    {
        _currentTheme = theme;

        var app = Application.Current;
        if (app == null) return;

        // Обновляем существующие кисти вместо создания новых
        UpdateBrushResource(app, "SM.BackgroundPrimary", theme.BackgroundPrimary);
        UpdateBrushResource(app, "SM.BackgroundSecondary", theme.BackgroundSecondary);
        UpdateBrushResource(app, "SM.Border", theme.Border);
        UpdateBrushResource(app, "SM.AccentPrimary", theme.AccentPrimary);
        UpdateBrushResource(app, "SM.AccentHover", theme.AccentHover);
        UpdateBrushResource(app, "SM.TextHeading", theme.TextHeading);
        UpdateBrushResource(app, "SM.TextPrimary", theme.TextPrimary);
        UpdateBrushResource(app, "SM.StatusGood", theme.StatusGood);
        UpdateBrushResource(app, "SM.StatusWarning", theme.StatusWarning);
        UpdateBrushResource(app, "SM.StatusCritical", theme.StatusCritical);
    }

    private static void UpdateBrushResource(Application app, string key, string colorHex)
    {
        if (app.Resources.TryGetValue(key, out var resource))
        {
            if (resource is SolidColorBrush brush)
            {
                brush.Color = Color.Parse(colorHex);
            }
            else
            {
                // Если ресурс не SolidColorBrush, создаём новый
                app.Resources[key] = new SolidColorBrush(Color.Parse(colorHex));
            }
        }
        else
        {
            // Если ресурса нет, создаём новый
            app.Resources[key] = new SolidColorBrush(Color.Parse(colorHex));
        }
    }

    public static AppTheme GenerateRandomTheme()
    {
        var random = new Random();

        var bgPrimary = GenerateRandomDarkColor(random);
        var bgSecondary = GenerateRandomDarkColor(random);
        var accent = GenerateRandomBrightColor(random);
        var text = random.Next(2) == 0 ? "#FFFFFF" : "#E0E0E0";

        var theme = new AppTheme
        {
            Name = "Random Theme",
            Icon = "🎲",
            BackgroundPrimary = bgPrimary,
            BackgroundSecondary = bgSecondary,
            Border = accent,
            AccentPrimary = accent,
            AccentHover = GenerateRandomBrightColor(random),
            TextHeading = text,
            TextPrimary = random.Next(2) == 0 ? "#B0B0B0" : "#C0C0C0",
            StatusGood = "#00FF00",
            StatusWarning = "#FFFF00",
            StatusCritical = "#FF0000",
            IsDark = true,
            IsRandom = true
        };

        return theme;
    }

    private static string GenerateRandomDarkColor(Random random)
    {
        var r = random.Next(0, 50);
        var g = random.Next(0, 50);
        var b = random.Next(0, 50);
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    private static string GenerateRandomBrightColor(Random random)
    {
        var r = random.Next(150, 256);
        var g = random.Next(150, 256);
        var b = random.Next(150, 256);
        return $"#{r:X2}{g:X2}{b:X2}";
    }
}