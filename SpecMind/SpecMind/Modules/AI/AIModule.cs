using SpecMind.Modules.AI.Configuration;
using SpecMind.Modules.AI.Models;
using SpecMind.Modules.AI.Providers;
using SpecMind.Modules.AI.Runtime;
using SpecMind.Modules.AI.Services;

namespace SpecMind.Modules.AI;

public sealed class AIModule : IAsyncDisposable
{
    public const string ModelFileName = "qwen2.5-3b-instruct-q4_k_m.gguf";
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
        Configuration = new AIConfigurationService(Environment);
        Downloader = new ModelDownloader(Environment);
        ModelManager = new ModelManager();
        var configuration = Configuration.Configuration;
        PromptBuilder = new PromptBuilder(configuration.MaxHistoryMessages);
        SnapshotBuilder = new SnapshotBuilder();
        Runtime = new LLamaRuntime(configuration);

        string modelPath;
        try
        {
            var directory = string.IsNullOrWhiteSpace(configuration.ModelsDirectory)
                ? Environment.ModelsDirectory : Path.GetFullPath(configuration.ModelsDirectory, AppContext.BaseDirectory);
            modelPath = Path.Combine(directory, ModelFileName);
        }
        catch (Exception)
        {
            modelPath = Path.Combine(Environment.ModelsDirectory, ModelFileName);
        }
        ModelManager.Register(new AIModel
        {
            Name = "Qwen2.5 3B", Version = "Instruct", Path = modelPath,
            IsInstalled = File.Exists(modelPath), IsActive = true
        });
        // Missing model is reported on the AI page, never while starting the GUI.
        Provider = new QwenProvider(Runtime, modelPath);
        AIService = new AIService(PromptBuilder, SnapshotBuilder, Provider);
    }

    public ValueTask DisposeAsync() => Runtime.DisposeAsync();
}
