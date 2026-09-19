using SpecMind.Modules.AI;
using SpecMind.Modules.AI.Configuration;
using SpecMind.Modules.AI.Models;
using SpecMind.Modules.AI.Providers;
using SpecMind.Modules.AI.Runtime;
using Xunit;

namespace SpecMind.Tests;

public class LocalModelTests
{
    [LocalModelFact]
    public async Task NativeGenerationCanBeCancelledAndRetried()
    {
        var path = Environment.GetEnvironmentVariable("SPECMIND_MODEL_PATH")
            ?? Path.Combine(AppContext.BaseDirectory, "Models", AIModule.ModelFileName);
        await using var runtime = new LLamaRuntime(new AIConfiguration
        { ContextSize = 4096, MaxTokens = 256, Threads = 8, Temperature = 0.1f });
        var service = AITests.Service(new QwenProvider(runtime, path));
        using var cancel = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        var progress = new InlineProgress(status =>
        {
            if (status.Contains("формирует")) cancel.CancelAfter(TimeSpan.FromMilliseconds(200));
        });
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.SendMessageAsync(
            AITests.Hardware(), [], "Подробно объясни все характеристики моего компьютера.", progress, cancel.Token));
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        var answer = await service.SendMessageAsync(AITests.Hardware(), [],
            "Назови только модель моей видеокарты.", cancellationToken: timeout.Token);
        Assert.Contains("3080", answer);
        await runtime.DisposeAsync();
        Assert.False(runtime.IsLoaded);
        Assert.Equal(RuntimeState.NotLoaded, runtime.State);
    }

    private sealed class InlineProgress(Action<string> report) : IProgress<string>
    {
        public void Report(string value) => report(value);
    }

    [LocalModelFact]
    public async Task QwenAnswersTwoHardwareQuestionsWithOneLoadedModel()
    {
        var path = Environment.GetEnvironmentVariable("SPECMIND_MODEL_PATH")
            ?? Path.Combine(AppContext.BaseDirectory, "Models", AIModule.ModelFileName);
        Assert.True(File.Exists(path), "Set SPECMIND_MODEL_PATH to a local Qwen GGUF.");
        await using var runtime = new LLamaRuntime(new AIConfiguration
        { ContextSize = 4096, MaxTokens = 96, Threads = 8, Temperature = 0.1f });
        var service = AITests.Service(new QwenProvider(runtime, path));
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        var first = await service.SendMessageAsync(AITests.Hardware(), [],
            "Назови только модель моего процессора. Без пояснений.", cancellationToken: timeout.Token);
        Assert.Contains("13400", first);
        Assert.True(runtime.IsLoaded);
        var weightsField = typeof(LLamaRuntime).GetField("_weights", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var weights = weightsField.GetValue(runtime);
        var second = await service.SendMessageAsync(AITests.Hardware(),
            [new ChatMessage { IsUser = true, Message = "Назови модель моего процессора." }, new ChatMessage { Message = first }],
            "Теперь назови только модель моей видеокарты.", cancellationToken: timeout.Token);
        Assert.Contains("3080", second);
        Assert.Same(weights, weightsField.GetValue(runtime));
        Assert.Equal(RuntimeState.Ready, runtime.State);
    }
}

public sealed class LocalModelFactAttribute : FactAttribute
{
    public LocalModelFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("SPECMIND_RUN_MODEL_TESTS") != "1")
            Skip = "Opt in with SPECMIND_RUN_MODEL_TESTS=1; requires a local GGUF and CPU inference.";
    }
}
