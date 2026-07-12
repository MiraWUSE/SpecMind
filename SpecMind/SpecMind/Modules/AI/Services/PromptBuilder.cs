using SpecMind.Modules.AI.Models;
using System.Collections.Generic;
using System.Text;

namespace SpecMind.Modules.AI.Services;

public class PromptBuilder
{
    public string BuildPrompt(
        HardwareSnapshot snapshot,
        IEnumerable<ChatMessage> history,
        string userMessage)
    {
        StringBuilder builder = new();

        builder.AppendLine("Ты встроенный AI-помощник приложения SpecMind.");
        builder.AppendLine("Отвечай только по теме компьютеров, ноутбуков и комплектующих.");
        builder.AppendLine();

        builder.AppendLine("===== Характеристики компьютера =====");

        if (snapshot.Hardware != null)
        {
            builder.AppendLine(snapshot.Hardware.ToString());
        }

        builder.AppendLine();

        builder.AppendLine("===== История диалога =====");

        foreach (var message in history)
        {
            string role = message.IsUser ? "Пользователь" : "AI";

            builder.AppendLine($"{role}: {message.Message}");
        }

        builder.AppendLine();

        builder.AppendLine("===== Новый вопрос =====");

        builder.AppendLine(userMessage);

        return builder.ToString();
    }
}