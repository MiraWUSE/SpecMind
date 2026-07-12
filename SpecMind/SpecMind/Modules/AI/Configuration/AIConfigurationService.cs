using System;
using System.Text.Json;
using System.IO;

namespace SpecMind.Modules.AI.Configuration;

public class AIConfigurationService
{
    private readonly string _configPath;

    public AIConfiguration Configuration { get; private set; }

    private readonly AIEnvironment _environment;

    public AIConfigurationService()
    {
        _environment = new AIEnvironment();

        _environment.Initialize();

        _configPath = _environment.ConfigurationFile;

        Configuration = Load();
    }

    /// Загружает конфигурацию.
    /// Если файла нет — создаёт его с настройками по умолчанию.

    private AIConfiguration Load()
    {
        try
        {
            if (!File.Exists(_configPath))
            {
                AIConfiguration configuration = new();

                Save(configuration);

                return configuration;
            }

            string json = File.ReadAllText(_configPath);

            AIConfiguration? configurationFromFile =
                JsonSerializer.Deserialize<AIConfiguration>(json);

            return configurationFromFile ?? new AIConfiguration();
        }
        catch
        {
            return new AIConfiguration();
        }
    }

    /// Сохраняет конфигурацию.
    public void Save(AIConfiguration configuration)
    {
        Configuration = configuration;

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        string json = JsonSerializer.Serialize(configuration, options);

        File.WriteAllText(_configPath, json);
    }

    /// Возвращает текущую конфигурацию.
    public AIConfiguration GetConfiguration()
    {
        return Configuration;
    }

    /// Сбрасывает настройки к значениям по умолчанию.
    public void Reset()
    {
        Configuration = new AIConfiguration();

        Save(Configuration);
    }
}