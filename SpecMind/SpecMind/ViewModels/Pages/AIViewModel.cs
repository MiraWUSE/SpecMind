using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpecMind.Modules.AI.Runtime;
using SpecMind.Modules.AI.Services;
using SpecMind.ViewModels;

namespace SpecMind.Modules.AI.ViewModels;

public partial class AIViewModel : ViewModelBase
{
    private readonly AIService _aiService;

    public ChatMemory Memory { get; } = new();

    [ObservableProperty]
    private string userMessage = "";

    [ObservableProperty]
    private bool isBusy;

    public AIViewModel(AIService aiService)
    {
        _aiService = aiService;

        Memory.Messages.Add(new()
        {
            IsUser = false,
            Message =
"""
Здравствуйте!

Я SpecMind AI.

Я могу:

• анализировать ваш компьютер;

• подобрать комплектующие;

• объяснить характеристики;

• подсказать апгрейд;

• рассказать о совместимости компонентов;

• помочь выбрать ноутбук.
"""
        });
    }

    [RelayCommand]
    public async Task SendAsync()
    {
        if (IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(UserMessage))
            return;

        IsBusy = true;

        string question = UserMessage;

        Memory.AddUser(question);

        UserMessage = "";

        var aiMessage = Memory.AddAssistantThinking();

        try
        {
            string answer =
                await _aiService.SendMessageAsync(
                    Memory.Messages,
                    question);

            aiMessage.IsThinking = false;
            aiMessage.Message = answer;
        }
        catch (Exception ex)
        {
            aiMessage.IsThinking = false;
            aiMessage.Message = ex.Message;
        }

        IsBusy = false;
    }

    [RelayCommand]
    private async Task TestAI()
    {
        try
        {
            string answer = await _aiService.SendMessageAsync(
                Memory.Messages,
                "Привет! Представься.");

            Memory.AddAssistant(answer);
        }
        catch (Exception ex)
        {
            Memory.AddAssistant("Ошибка:\n\n" + ex);
        }
    }
}