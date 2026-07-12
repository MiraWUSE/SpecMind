using System;
using System.IO;

namespace SpecMind.Modules.AI.Configuration;

public class AIEnvironment
{
    public string RootDirectory { get; }

    public string ModelsDirectory { get; }

    public string ChatsDirectory { get; }

    public string CacheDirectory { get; }

    public string DownloadsDirectory { get; }

    public string LogsDirectory { get; }

    public string ConfigurationFile { get; }

    public AIEnvironment()
    {
        RootDirectory = AppContext.BaseDirectory;

        ModelsDirectory = Path.Combine(RootDirectory, "Models");

        ChatsDirectory = Path.Combine(RootDirectory, "Chats");

        CacheDirectory = Path.Combine(RootDirectory, "Cache");

        DownloadsDirectory = Path.Combine(RootDirectory, "Downloads");

        LogsDirectory = Path.Combine(RootDirectory, "Logs");

        ConfigurationFile = Path.Combine(RootDirectory, "ai-settings.json");
    }

    /// Создает необходимые папки AI.
    public void Initialize()
    {
        Directory.CreateDirectory(ModelsDirectory);

        Directory.CreateDirectory(ChatsDirectory);

        Directory.CreateDirectory(CacheDirectory);

        Directory.CreateDirectory(DownloadsDirectory);

        Directory.CreateDirectory(LogsDirectory);
    }
}