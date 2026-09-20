using System.Runtime.CompilerServices;
using SpecMind.Modules.AI.Models;
using SpecMind.Modules.AI.Providers;
using SpecMind.Modules.AI.Runtime;
using SpecMind.Modules.AI.Services;
using SpecMind.Modules.AI.ViewModels;
using SpecMind.ViewModels;
using Xunit;

namespace SpecMind.Tests;

public class ChatBehaviorTests
{
    [Fact]
    public void HistoryIsAnIndependentSnapshotAndClearRemovesAllMessages()
    {
        var memory = new ChatMemory();
        var user = memory.AddUser("Question");
        memory.AddAssistant("Answer");
        memory.AddAssistantThinking();
        memory.AddAssistant(" ");
        var history = memory.GetHistory();
        user.Message = "Changed";
        memory.Clear();
        Assert.Empty(memory.Messages);
        Assert.Equal(2, history.Length);
        Assert.Equal("Question", history[0].Message);
        Assert.True(history[0].IsUser);
        Assert.False(history[1].IsUser);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task BlankQuestionsDoNotStartGeneration(string question)
    {
        var provider = new ControlledProvider();
        await using var vm = new AIViewModel(AITests.Service(provider), AITests.Hardware) { UserMessage = question };
        Assert.False(vm.SendCommand.CanExecute(null));
        await vm.SendAsync();
        Assert.Equal(0, provider.Calls);
        Assert.Single(vm.Memory.Messages);
    }

    [Fact]
    public async Task ConcurrentSendIsRejectedAndDraftIsPreserved()
    {
        var provider = new ControlledProvider();
        await using var vm = new AIViewModel(AITests.Service(provider), AITests.Hardware) { UserMessage = "First" };
        var first = vm.SendAsync();
        vm.UserMessage = "Next draft";
        Assert.False(vm.SendCommand.CanExecute(null));
        await vm.SendAsync();
        Assert.Equal(1, provider.Calls);
        provider.Completion.SetResult("First answer");
        await first;
        Assert.Equal("Next draft", vm.UserMessage);
        Assert.True(vm.SendCommand.CanExecute(null));
    }

    [Fact]
    public async Task CancelCommandStopsGenerationAndAllowsRetry()
    {
        var provider = new ControlledProvider();
        await using var vm = new AIViewModel(AITests.Service(provider), AITests.Hardware) { UserMessage = "Cancel me" };
        var task = vm.SendCommand.ExecuteAsync(null);
        Assert.True(vm.SendCancelCommand.CanExecute(null));
        vm.SendCancelCommand.Execute(null);
        await task;
        Assert.False(vm.IsBusy);
        Assert.Empty(vm.Memory.GetHistory());
        Assert.Equal("Генерация остановлена.", vm.Memory.Messages[^1].Message);
        vm.UserMessage = "Retry";
        provider.Completion.SetResult("Answer");
        await vm.SendCommand.ExecuteAsync(null);
        Assert.Equal("Answer", vm.Memory.Messages[^1].Message);
    }

    [Fact]
    public async Task EmptyAnswerIsAnErrorAndRestoresQuestion()
    {
        var provider = new ControlledProvider();
        provider.Completion.SetResult("  ");
        await using var vm = new AIViewModel(AITests.Service(provider), AITests.Hardware) { UserMessage = "Question" };
        await vm.SendAsync();
        Assert.Equal("Question", vm.UserMessage);
        Assert.True(vm.Memory.Messages[^1].IsError);
        Assert.Empty(vm.Memory.GetHistory());
    }

    [Fact]
    public async Task StreamingServiceForwardsHardwareAndCancellation()
    {
        var provider = new ControlledProvider();
        var service = AITests.Service(provider);
        using var cancel = new CancellationTokenSource();
        var result = new List<string>();
        await foreach (var token in service.StreamMessageAsync(AITests.Hardware(), [], "Question", cancellationToken: cancel.Token))
            result.Add(token);
        Assert.Equal(new[] { "one", "two" }, result);
        Assert.Contains("RTX 3080", provider.Prompt);
        cancel.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var token in service.StreamMessageAsync(AITests.Hardware(), [], "Question", cancellationToken: cancel.Token)) { }
        });
    }

    [Fact]
    public void ZeroHistoryLimitExcludesPreviousConversation()
    {
        var prompt = new PromptBuilder(0).BuildPrompt(new SnapshotBuilder().Build(AITests.Hardware()),
            [new ChatMessage { IsUser = true, Message = "OLD QUESTION" }], "NEW QUESTION");
        Assert.DoesNotContain("OLD QUESTION", prompt);
        Assert.Contains("NEW QUESTION", prompt);
    }

    [Fact]
    public async Task ScannerFailurePreservesLastSnapshotAndRecovers()
    {
        var scanner = new StubScanner();
        await using var main = new MainWindowViewModel(scanner);
        await main.RefreshHardwareAsync();
        var snapshot = main.HardwareInfo;
        scanner.Error = new IOException("Device unavailable");
        await main.RefreshHardwareAsync();
        Assert.Same(snapshot, main.HardwareInfo);
        Assert.Single(main.CpuUsageData);
        scanner.Error = null;
        scanner.Hardware = AITests.Hardware();
        scanner.Hardware.Cpu.Name = "Recovered";
        await main.RefreshHardwareAsync();
        Assert.Equal("Recovered", main.HardwareInfo.Cpu.Name);
        Assert.Equal(2, main.CpuUsageData.Count);
    }

    private sealed class ControlledProvider : IChatProvider
    {
        public int Calls { get; private set; }
        public string Prompt { get; private set; } = "";
        public TaskCompletionSource<string> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task<string> SendMessageAsync(string prompt, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
        {
            Calls++;
            Prompt = prompt;
            return Completion.Task.WaitAsync(cancellationToken);
        }
        public async IAsyncEnumerable<string> StreamMessageAsync(string prompt, IProgress<string>? progress = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Prompt = prompt;
            cancellationToken.ThrowIfCancellationRequested();
            yield return "one";
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            yield return "two";
        }
    }
}
