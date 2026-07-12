using SpecMind.Modules.AI.Configuration;
using SpecMind.Modules.AI.Models;
using System;
using System.IO;

namespace SpecMind.Modules.AI.Services;

public class ModelDownloader
{
    public string ModelsDirectory { get; }

    private readonly AIEnvironment _environment;

    public ModelDownloader()
    {
        _environment = new AIEnvironment();

        _environment.Initialize();
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
            IsLoaded = false,
            IsActive = false
        };
    }
}