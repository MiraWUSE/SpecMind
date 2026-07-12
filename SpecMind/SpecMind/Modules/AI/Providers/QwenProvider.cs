using SpecMind.Modules.AI.Runtime;

namespace SpecMind.Modules.AI.Providers;

public class QwenProvider : IChatProvider
{
    private readonly LLamaRuntime _runtime;

    private readonly string _modelPath;

    public QwenProvider(
        LLamaRuntime runtime,
        string modelPath)
    {
        _runtime = runtime;
        _modelPath = modelPath;
    }

    public async Task<string> SendMessageAsync(string prompt)
    {
        if (!_runtime.IsLoaded)
            await _runtime.LoadAsync(_modelPath);

        return await _runtime.Executor!.InferAsync(prompt);
    }

    public async IAsyncEnumerable<string> StreamMessageAsync(string prompt)
    {
        if (!_runtime.IsLoaded)
            await _runtime.LoadAsync(_modelPath);

        await foreach (var token in _runtime.Executor!.InferStreamAsync(prompt))
            yield return token;
    }
}