using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpecMind.Modules.AI.Runtime;
using SpecMind.ViewModels;
using SpecMind.ViewModels.Pages;
using System;
using System.Threading.Tasks;

namespace SpecMind.Modules.AI.ViewModels; // <-- ПРОВЕРЬТЕ, ЧТО ЭТОТ NEAMESPACE ТОЧНЫЙ

public partial class AIViewModel : PagesViewModelBase
{
    public ChatMemory Memory { get; } = new();

    [ObservableProperty]
    private string userMessage = "";

    [ObservableProperty]
    private bool isBusy;

    // <-- КОНСТРУКТОР ТЕПЕРЬ ПРИНИМАЕТ ТОЛЬКО MainWindowViewModel
    public AIViewModel(MainWindowViewModel main) : base(main)
    {
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
        if (IsBusy) return;
        if (string.IsNullOrWhiteSpace(UserMessage)) return;

        IsBusy = true;
        string question = UserMessage;
        Memory.AddUser(question);
        UserMessage = "";

        var aiMessage = Memory.AddAssistantThinking();

        try
        {
            // ВРЕМЕННАЯ ЗАГЛУШКА: чтобы приложение компилировалось и работало
            await Task.Delay(1000);
            aiMessage.IsThinking = false;
            aiMessage.Message = $"Вы спросили: \"{question}\"\n\n(Полная интеграция с ИИ-провайдером настраивается)";
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
            await Task.Delay(500);
            Memory.AddAssistant("Тестовый ответ. Реальная интеграция с ИИ скоро будет добавлена.");
        }
        catch (Exception ex)
        {
            Memory.AddAssistant("Ошибка:\n\n" + ex);
        }
    }
}