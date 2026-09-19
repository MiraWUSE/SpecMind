using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpecMind.Models;
using SpecMind.Modules.AI.Runtime;
using SpecMind.Modules.AI.Services;
using SpecMind.ViewModels;
using System.Threading;

namespace SpecMind.Modules.AI.ViewModels;

public partial class AIViewModel : ViewModelBase, IAsyncDisposable
{
    private readonly AIService _aiService;
    private readonly Func<HardwareInfo> _hardwareSource;
    private readonly CancellationTokenSource _lifetime = new();
    private Task _requestTask = Task.CompletedTask;
    private bool _disposed;
    public ChatMemory Memory { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private string userMessage = "";
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SendCommand))]
    private bool isBusy;
    [ObservableProperty]
    private string statusText = "Qwen2.5 • локальная модель загрузится при первом вопросе";

    public AIViewModel(AIService aiService, Func<HardwareInfo> hardwareSource)
    {
        _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
        _hardwareSource = hardwareSource ?? throw new ArgumentNullException(nameof(hardwareSource));
        Memory.Messages.Add(new()
        {
            IsSystemMessage = true,
            Message = "Здравствуйте! Я SpecMind AI. Задайте вопрос о вашем компьютере — его характеристики будут переданы локальной модели автоматически."
        });
    }

    private bool CanSend() => !_disposed && !IsBusy && !string.IsNullOrWhiteSpace(UserMessage);

    [RelayCommand(CanExecute = nameof(CanSend), IncludeCancelCommand = true)]
    public Task SendAsync(CancellationToken cancellationToken = default)
    {
        if (!CanSend()) return Task.CompletedTask;
        return _requestTask = SendCoreAsync(cancellationToken);
    }

    private async Task SendCoreAsync(CancellationToken cancellationToken)
    {
        IsBusy = true;
        StatusText = "Подготовка запроса…";
        var question = UserMessage.Trim();
        var history = Memory.GetHistory(); // Capture BEFORE appending this question.
        var user = Memory.AddUser(question);
        UserMessage = "";
        var answer = Memory.AddAssistantThinking();
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token, cancellationToken);
        var progress = new Progress<string>(status =>
        {
            if (IsBusy && !_disposed) StatusText = status;
        });
        try
        {
            var text = await _aiService.SendMessageAsync(_hardwareSource(), history, question, progress, linked.Token);
            linked.Token.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(text)) throw new InvalidOperationException("Модель вернула пустой ответ. Попробуйте переформулировать вопрос.");
            answer.Message = text;
            StatusText = "Готова • модель работает локально";
        }
        catch (OperationCanceledException)
        {
            user.IsError = true;
            answer.IsError = true;
            answer.Message = "Генерация остановлена.";
            StatusText = "Генерация остановлена";
        }
        catch (Exception ex)
        {
            user.IsError = true;
            answer.IsError = true;
            answer.Message = "Не удалось получить ответ: " + ex.Message;
            StatusText = "Ошибка • можно повторить запрос";
            if (string.IsNullOrEmpty(UserMessage)) UserMessage = question;
        }
        finally
        {
            answer.IsThinking = false;
            IsBusy = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;
            _lifetime.Cancel();
            SendCommand.NotifyCanExecuteChanged();
        }
        await _requestTask;
    }
}
