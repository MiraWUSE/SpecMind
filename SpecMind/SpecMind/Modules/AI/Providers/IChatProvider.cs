using System.Threading;

namespace SpecMind.Modules.AI.Providers;

public interface IChatProvider
{
    Task<string> SendMessageAsync(string prompt, IProgress<string> progress = null, CancellationToken cancellationToken = default);
    IAsyncEnumerable<string> StreamMessageAsync(string prompt, IProgress<string> progress = null, CancellationToken cancellationToken = default);
}
