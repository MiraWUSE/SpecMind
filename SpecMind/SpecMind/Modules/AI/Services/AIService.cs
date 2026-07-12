using SpecMind.Modules.AI.Models;
using SpecMind.Modules.AI.Providers;

namespace SpecMind.Modules.AI.Services;

public class AIService
{
    private readonly PromptBuilder _promptBuilder;

    private readonly SnapshotBuilder _snapshotBuilder;

    private readonly IChatProvider _provider;

    public AIService(
        PromptBuilder promptBuilder,
        SnapshotBuilder snapshotBuilder,
        IChatProvider provider)
    {
        _promptBuilder = promptBuilder;
        _snapshotBuilder = snapshotBuilder;
        _provider = provider;
    }

    /// <summary>
    /// Отправить сообщение AI.
    /// </summary>
    public async Task<string> SendMessageAsync(
        IEnumerable<ChatMessage> history,
        string userMessage)
    {
        HardwareSnapshot snapshot =
            await _snapshotBuilder.BuildAsync();

        string prompt = _promptBuilder.BuildPrompt(
            snapshot,
            history,
            userMessage);

        return await _provider.SendMessageAsync(prompt);
    }

    /// <summary>
    /// Потоковая генерация.
    /// </summary>
    public IAsyncEnumerable<string> StreamMessageAsync(
        IEnumerable<ChatMessage> history,
        string userMessage)
    {
        return StreamInternal(history, userMessage);
    }

    private async IAsyncEnumerable<string> StreamInternal(
        IEnumerable<ChatMessage> history,
        string userMessage)
    {
        HardwareSnapshot snapshot =
            await _snapshotBuilder.BuildAsync();

        string prompt = _promptBuilder.BuildPrompt(
            snapshot,
            history,
            userMessage);

        await foreach (string token in _provider.StreamMessageAsync(prompt))
        {
            yield return token;
        }
    }
}