namespace SpecMind.Modules.AI.Models;

public class AIModel
{
    /// <summary>
    /// Название модели.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Версия модели.
    /// </summary>
    public string Version { get; set; } = "";

    /// <summary>
    /// Полный путь к GGUF.
    /// </summary>
    public string Path { get; set; } = "";

    /// <summary>
    /// Размер модели.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Дата установки.
    /// </summary>
    public DateTime InstalledAt { get; set; }

    /// <summary>
    /// Установлена ли модель.
    /// </summary>
    public bool IsInstalled { get; set; }

    /// <summary>
    /// Загружена ли модель в память.
    /// </summary>
    public bool IsLoaded { get; set; }

    /// <summary>
    /// Является ли активной.
    /// </summary>
    public bool IsActive { get; set; }
}