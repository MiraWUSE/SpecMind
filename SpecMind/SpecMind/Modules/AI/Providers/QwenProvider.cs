using SpecMind.Modules.AI.Runtime;
using System.Threading;

namespace SpecMind.Modules.AI.Providers;

public class QwenProvider : IChatProvider
{
    private readonly LLamaRuntime _runtime;
    private readonly string _modelPath;

    public QwenProvider(LLamaRuntime runtime, string modelPath)
    {
        _runtime = runtime;
        _modelPath = modelPath;
    }

    public Task<string> SendMessageAsync(string prompt, IProgress<string> progress = null,
        CancellationToken cancellationToken = default)
        => Task.Run(() => _runtime.InferAsync(_modelPath, prompt, progress, cancellationToken), cancellationToken);

    public IAsyncEnumerable<string> StreamMessageAsync(string prompt, IProgress<string> progress = null,
        CancellationToken cancellationToken = default)
        => _runtime.InferStreamAsync(_modelPath, prompt, progress, cancellationToken);
}
