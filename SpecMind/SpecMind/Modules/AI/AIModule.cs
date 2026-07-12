using System.IO;
using SpecMind.Modules.AI.Configuration;
using SpecMind.Modules.AI.Models;
using SpecMind.Modules.AI.Providers;
using SpecMind.Modules.AI.Runtime;
using SpecMind.Modules.AI.Services;

namespace SpecMind.Modules.AI;

public class AIModule
{
    public AIEnvironment Environment { get; }

    public AIConfigurationService Configuration { get; }

    public ModelDownloader Downloader { get; }

    public ModelManager ModelManager { get; }

    public PromptBuilder PromptBuilder { get; }

    public SnapshotBuilder SnapshotBuilder { get; }

    public LLamaRuntime Runtime { get; }

    public AIService AIService { get; }

    public IChatProvider Provider { get; }

    public AIModule()
    {
        Environment = new AIEnvironment();
        Environment.Initialize();

        Configuration = new AIConfigurationService();

        Downloader = new ModelDownloader();

        ModelManager = new ModelManager();

        PromptBuilder = new PromptBuilder();

        SnapshotBuilder = new SnapshotBuilder();

        Runtime = new LLamaRuntime();

        string modelPath = Path.Combine(
            AppContext.BaseDirectory,
            "Models",
            "qwen2.5-3b-instruct-q4_k_m.gguf");

        if (!File.Exists(modelPath))
            throw new FileNotFoundException(modelPath);

        var model = new AIModel
        {
            Name = "Qwen2.5 3B",
            Version = "Instruct",
            Path = modelPath,
            IsInstalled = true,
            IsActive = true
        };

        ModelManager.Register(model);

        Provider = new QwenProvider(Runtime, model.Path);

        AIService = new AIService(
            PromptBuilder,
            SnapshotBuilder,
            Provider);
    }
}