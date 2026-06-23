namespace SpecMind.Models;

public class AppTheme
{
    public string Name { get; set; } = "Unknown";
    public string Icon { get; set; } = "";

    // Основные цвета
    public string BackgroundPrimary { get; set; } = "#1A1B26";
    public string BackgroundSecondary { get; set; } = "#24253A";
    public string Border { get; set; } = "#2F3146";

    // Акценты
    public string AccentPrimary { get; set; } = "#7AA9E0";
    public string AccentHover { get; set; } = "#9DBFE8";

    // Текст
    public string TextHeading { get; set; } = "#E4E6F1";
    public string TextPrimary { get; set; } = "#A9B1D6";

    // Индикаторы
    public string StatusGood { get; set; } = "#9ECE6A";
    public string StatusWarning { get; set; } = "#E0AF68";
    public string StatusCritical { get; set; } = "#F7768E";

    // Метаданные
    public bool IsDark { get; set; } = true;
    public bool IsRandom { get; set; } = false;
}