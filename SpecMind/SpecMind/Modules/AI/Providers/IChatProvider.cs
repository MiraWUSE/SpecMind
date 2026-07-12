namespace SpecMind.Modules.AI.Providers;

public interface IChatProvider
{
    /// Отправить сообщение модели
    Task<string> SendMessageAsync(string prompt);

    /// Потоковая генерация ответа
    IAsyncEnumerable<string> StreamMessageAsync(string prompt);
}