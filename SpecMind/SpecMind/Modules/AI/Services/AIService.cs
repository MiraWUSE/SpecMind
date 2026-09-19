using SpecMind.Models;
using SpecMind.Modules.AI.Models;
using SpecMind.Modules.AI.Providers;
using System.Threading;

namespace SpecMind.Modules.AI.Services;

public class AIService
{
    private readonly PromptBuilder _promptBuilder;
    private readonly SnapshotBuilder _snapshotBuilder;
    private readonly IChatProvider _provider;

    public AIService(PromptBuilder promptBuilder, SnapshotBuilder snapshotBuilder, IChatProvider provider)
    {
        _promptBuilder = promptBuilder;
        _snapshotBuilder = snapshotBuilder;
        _provider = provider;
    }

    private string BuildPrompt(HardwareInfo hardware, IEnumerable<ChatMessage> history, string question)
    {
        ArgumentNullException.ThrowIfNull(hardware);
        if (string.IsNullOrWhiteSpace(hardware.Cpu.Name) && string.IsNullOrWhiteSpace(hardware.Gpu.Name))
            throw new InvalidOperationException("Характеристики ПК ещё не получены. Дождитесь сканирования и повторите вопрос.");
        return _promptBuilder.BuildPrompt(_snapshotBuilder.Build(hardware), history, question);
    }

    public Task<string> SendMessageAsync(HardwareInfo hardware, IEnumerable<ChatMessage> history, string userMessage,
        IProgress<string> progress = null, CancellationToken cancellationToken = default)
        => _provider.SendMessageAsync(BuildPrompt(hardware, history, userMessage), progress, cancellationToken);

    public IAsyncEnumerable<string> StreamMessageAsync(HardwareInfo hardware, IEnumerable<ChatMessage> history, string userMessage,
        IProgress<string> progress = null, CancellationToken cancellationToken = default)
        => _provider.StreamMessageAsync(BuildPrompt(hardware, history, userMessage), progress, cancellationToken);
}
