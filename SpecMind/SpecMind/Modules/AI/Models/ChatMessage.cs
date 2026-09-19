using CommunityToolkit.Mvvm.ComponentModel;

namespace SpecMind.Modules.AI.Models;

public partial class ChatMessage : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Sender))]
    private bool isUser;
    [ObservableProperty]
    private string message = "";
    [ObservableProperty]
    private bool isThinking;
    [ObservableProperty]
    private bool isError;
    public bool IsSystemMessage { get; set; }
    public DateTime Time { get; set; } = DateTime.Now;
    public string Sender => IsUser ? "Вы" : "SpecMind AI";
}
