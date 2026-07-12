using LLama;
using LLama.Common;

namespace SpecMind.Modules.AI.Runtime;

public class LLamaExecutor
{
    private readonly LLamaRuntime _runtime;

    private InteractiveExecutor? _executor;

    public LLamaExecutor(LLamaRuntime runtime)
    {
        _runtime = runtime;
    }

    public void Initialize()
    {
        _executor = new InteractiveExecutor(_runtime.Context);
    }

    public async Task<string> InferAsync(string prompt)
    {
        if (_executor == null)
            throw new InvalidOperationException("Executor отсутствует.");

        var parameters = new InferenceParams
        {
            MaxTokens = 512
        };

        string result = "";

        await foreach (var token in _executor.InferAsync(prompt, parameters))
        {
            result += token;
        }

        return result.Trim();
    }

    public async IAsyncEnumerable<string> InferStreamAsync(string prompt)
    {
        if (_executor == null)
            throw new InvalidOperationException("Executor отсутствует.");

        var parameters = new InferenceParams
        {
            MaxTokens = 512
        };

        await foreach (var token in _executor.InferAsync(prompt, parameters))
        {
            yield return token;
        }
    }
}