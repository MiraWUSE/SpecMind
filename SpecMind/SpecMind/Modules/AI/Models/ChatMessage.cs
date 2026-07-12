namespace SpecMind.Modules.AI.Models;

public class ChatMessage
{
    public bool IsUser { get; set; }

    public string Message { get; set; } = "";

    public DateTime Time { get; set; } = DateTime.Now;

    public bool IsThinking { get; set; }

    public string Sender => IsUser ? "Вы" : "SpecMind AI";
}