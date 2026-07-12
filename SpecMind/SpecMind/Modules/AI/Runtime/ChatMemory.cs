using System.Collections.ObjectModel;
using SpecMind.Modules.AI.Models;

namespace SpecMind.Modules.AI.Runtime;

public class ChatMemory
{
    public ObservableCollection<ChatMessage> Messages { get; }
        = new();

    public void AddUser(string text)
    {
        Messages.Add(new ChatMessage
        {
            IsUser = true,
            Message = text
        });
    }

    public ChatMessage AddAssistantThinking()
    {
        var msg = new ChatMessage
        {
            IsUser = false,
            IsThinking = true,
            Message = ""
        };

        Messages.Add(msg);

        return msg;
    }

    public void AddAssistant(string text)
    {
        Messages.Add(new ChatMessage
        {
            IsUser = false,
            Message = text,
            IsThinking = false
        });
    }

    public void Clear()
    {
        Messages.Clear();
    }
}