using LLama;
using LLama.Common;
using SpecMind.Modules.AI.Configuration;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace SpecMind.Modules.AI.Runtime;

public sealed class LLamaRuntime : IAsyncDisposable
{
    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly CancellationTokenSource _lifetime = new();
    private readonly AIConfiguration _configuration;
    private LLamaWeights _weights;
    private LLamaContext _context;
    private Task _disposeTask;
    private readonly object _disposeSync = new();
    private bool _disposed;

    public RuntimeState State { get; private set; } = RuntimeState.NotLoaded;
    public string ModelPath { get; private set; }
    public LLamaExecutor Executor { get; private set; }
    public bool IsLoaded => _weights != null;
    public LLamaContext Context => _context ?? throw new InvalidOperationException("Контекст модели отсутствует.");

    public LLamaRuntime(AIConfiguration configuration = null)
        => _configuration = configuration ?? new AIConfiguration();

    private ModelParams Parameters(string path) => new(path)
    {
        ContextSize = (uint)Math.Clamp(_configuration.ContextSize, 2048, 32768),
        GpuLayerCount = 0, // This application ships the CPU backend.
        Threads = Math.Clamp(_configuration.Threads, 1, System.Environment.ProcessorCount),
        BatchThreads = Math.Clamp(_configuration.Threads, 1, System.Environment.ProcessorCount)
    };

    private async Task LoadCoreAsync(string modelPath, IProgress<string> progress, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (IsLoaded)
        {
            if (!string.Equals(ModelPath, Path.GetFullPath(modelPath), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Перед сменой модели её необходимо выгрузить.");
            return;
        }
        State = RuntimeState.Loading;
        progress?.Report("Загрузка локальной модели…");
        try
        {
            if (!File.Exists(modelPath))
                throw new FileNotFoundException($"Модель не найдена. Поместите GGUF в: {modelPath}", modelPath);
            _weights = await LLamaWeights.LoadFromFileAsync(Parameters(modelPath), token).ConfigureAwait(false);
            ModelPath = Path.GetFullPath(modelPath);
            State = RuntimeState.Ready;
        }
        catch
        {
            ReleaseResources();
            State = RuntimeState.Error;
            throw;
        }
    }

    public async Task LoadAsync(string modelPath, CancellationToken cancellationToken = default)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token, cancellationToken);
        await _gate.WaitAsync(linked.Token).ConfigureAwait(false);
        try { await LoadCoreAsync(modelPath, null, linked.Token).ConfigureAwait(false); }
        finally { _gate.Release(); }
    }

    public async Task<string> InferAsync(string modelPath, string prompt, IProgress<string> progress = null,
        CancellationToken cancellationToken = default)
    {
        var answer = new StringBuilder();
        await foreach (var token in InferStreamAsync(modelPath, prompt, progress, cancellationToken).ConfigureAwait(false))
            answer.Append(token);
        return answer.ToString().Trim();
    }

    public async IAsyncEnumerable<string> InferStreamAsync(string modelPath, string prompt, IProgress<string> progress = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(_lifetime.Token, cancellationToken);
        var token = linked.Token;
        await _gate.WaitAsync(token).ConfigureAwait(false);
        try
        {
            await LoadCoreAsync(modelPath, progress, token).ConfigureAwait(false);
            // History is supplied in the prompt; keep weights but start with a clean KV context.
            try
            {
                _context?.Dispose();
                _context = null;
                _context = await Task.Run(() => _weights.CreateContext(Parameters(modelPath)), token).ConfigureAwait(false);
                Executor = new LLamaExecutor(_context, _configuration);
                State = RuntimeState.Generating;
                progress?.Report("Модель формирует ответ…");
            }
            catch { State = RuntimeState.Error; throw; }

            await using var tokens = Executor.InferStreamAsync(prompt, token).GetAsyncEnumerator(token);
            while (true)
            {
                bool hasNext;
                try
                {
                    // Native prompt evaluation/decoding must never run on the UI thread.
                    hasNext = await Task.Run(async () => await tokens.MoveNextAsync().ConfigureAwait(false), token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) { throw; }
                catch { State = RuntimeState.Error; throw; }
                if (!hasNext) break;
                yield return tokens.Current;
            }
        }
        finally
        {
            Executor = null;
            _context?.Dispose();
            _context = null;
            if (State == RuntimeState.Generating) State = RuntimeState.Ready;
            _gate.Release();
        }
    }

    public async Task UnloadAsync()
    {
        await _gate.WaitAsync().ConfigureAwait(false);
        try
        {
            State = RuntimeState.Unloading;
            ReleaseResources();
            State = RuntimeState.NotLoaded;
        }
        finally { _gate.Release(); }
    }

    private void ReleaseResources()
    {
        Executor = null;
        _context?.Dispose();
        _context = null;
        _weights?.Dispose();
        _weights = null;
        ModelPath = null;
    }

    public ValueTask DisposeAsync()
    {
        lock (_disposeSync)
        {
            _disposeTask ??= DisposeCoreAsync();
            return new ValueTask(_disposeTask);
        }
    }

    private async Task DisposeCoreAsync()
    {
        if (_disposed) return;
        _disposed = true;
        _lifetime.Cancel();
        await UnloadAsync().ConfigureAwait(false);
        // Leave the canceled token source available to safely reject late callers.
    }
}
