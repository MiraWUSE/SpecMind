using LLama;
using LLama.Common;
using LLama.Sampling;
using SpecMind.Modules.AI.Configuration;
using SpecMind.Modules.AI.Services;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace SpecMind.Modules.AI.Runtime;

public class LLamaExecutor
{
    private readonly LLamaContext _context;
    private readonly AIConfiguration _configuration;

    public LLamaExecutor(LLamaContext context, AIConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<string> InferAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var answer = new StringBuilder();
        await foreach (var token in InferStreamAsync(prompt, cancellationToken).ConfigureAwait(false))
            answer.Append(token);
        return answer.ToString().Trim();
    }

    public async IAsyncEnumerable<string> InferStreamAsync(string prompt,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var maxTokens = Math.Clamp(_configuration.MaxTokens, 32, (int)_context.ContextSize / 2);
        prompt = PromptBuilder.FitContext(prompt, text => _context.Tokenize(text, false, true).Length,
            (int)_context.ContextSize - maxTokens - 16);
        var parameters = new InferenceParams
        {
            MaxTokens = maxTokens,
            AntiPrompts = new[] { "<|im_end|>", "<|im_start|>", "<|endoftext|>" },
            SamplingPipeline = new DefaultSamplingPipeline
            {
                Temperature = Math.Clamp(_configuration.Temperature, 0f, 2f),
                TopP = Math.Clamp(_configuration.TopP, 0.01f, 1f)
            }
        };
        var executor = new InteractiveExecutor(_context);
        await foreach (var token in executor.InferAsync(prompt, parameters, cancellationToken).ConfigureAwait(false))
            yield return token;
    }
}
