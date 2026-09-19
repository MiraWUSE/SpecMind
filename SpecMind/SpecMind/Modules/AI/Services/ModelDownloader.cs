using SpecMind.Modules.AI.Configuration;
using SpecMind.Modules.AI.Models;
using System;
using System.IO;

namespace SpecMind.Modules.AI.Services;

public class ModelDownloader
{
    public string ModelsDirectory { get; }

    private readonly AIEnvironment _environment;

    public ModelDownloader(AIEnvironment environment = null)
    {
        _environment = environment ?? new AIEnvironment();
        ModelsDirectory = _environment.ModelsDirectory;
    }

    public AIModel RegisterModel(string filePath)
    {
        FileInfo file = new(filePath);

        return new AIModel
        {
            Name = Path.GetFileNameWithoutExtension(file.Name),
            Version = "Unknown",
            Path = file.FullName,
            Size = file.Length,
            InstalledAt = DateTime.Now,
            IsInstalled = file.Exists,
            IsLoaded = false,
            IsActive = false
        };
    }
}
