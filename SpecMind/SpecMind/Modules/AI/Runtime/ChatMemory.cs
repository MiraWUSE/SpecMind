using System.Collections.ObjectModel;
using SpecMind.Modules.AI.Models;

namespace SpecMind.Modules.AI.Runtime;

public class ChatMemory
{
    public ObservableCollection<ChatMessage> Messages { get; } = new();

    public ChatMessage AddUser(string text)
    {
        var message = new ChatMessage { IsUser = true, Message = text };
        Messages.Add(message);
        return message;
    }

    public ChatMessage AddAssistantThinking()
    {
        var message = new ChatMessage { IsThinking = true };
        Messages.Add(message);
        return message;
    }

    public void AddAssistant(string text) => Messages.Add(new ChatMessage { Message = text });
    public void Clear() => Messages.Clear();

    public ChatMessage[] GetHistory() => Messages
        .Where(m => !m.IsThinking && !m.IsError && !m.IsSystemMessage && !string.IsNullOrWhiteSpace(m.Message))
        .Select(m => new ChatMessage { IsUser = m.IsUser, Message = m.Message }).ToArray();
}
