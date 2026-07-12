using System;
using System.Text.Json;
using System.IO;

namespace SpecMind.Modules.AI.Configuration;

public class AIConfiguration
{
    /// Папка хранения моделей.
    public string ModelsDirectory { get; set; } = Path.Combine(AppContext.BaseDirectory, "Models");

    /// <summary>
    /// Папка хранения истории чатов.
    /// </summary>
    public string ChatsDirectory { get; set; } = Path.Combine(AppContext.BaseDirectory, "Chats");

    /// Название модели по умолчанию.
    public string DefaultModel { get; set; } = "Qwen2.5-3B-Instruct";

    /// Максимальное количество сообщений в истории.
    public int MaxHistoryMessages { get; set; } = 30;

    /// Максимальное количество токенов ответа.
    public int MaxTokens { get; set; } = 2048;

    /// Размер контекстного окна.
    public int ContextSize { get; set; } = 8192;

    /// Температура генерации.
    public float Temperature { get; set; } = 0.7f;

    /// Верхняя граница вероятности (Top-P).
    public float TopP { get; set; } = 0.95f;

    /// Использовать ли GPU.
    public bool UseGpu { get; set; } = true;

    /// Максимальное количество потоков CPU.
    public int Threads { get; set; } = Environment.ProcessorCount;

    /// Автоматически загружать последнюю модель.
    public bool AutoLoadLastModel { get; set; } = true;
}