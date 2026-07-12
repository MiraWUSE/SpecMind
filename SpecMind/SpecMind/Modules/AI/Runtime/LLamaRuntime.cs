using LLama;
using LLama.Common;

namespace SpecMind.Modules.AI.Runtime;

public sealed class LLamaRuntime : IDisposable
{
    private LLamaWeights? _weights;
    private LLamaContext? _context;

    public RuntimeState State { get; private set; } = RuntimeState.NotLoaded;

    public string? ModelPath { get; private set; }

    public LLamaExecutor? Executor { get; private set; }

    public bool IsLoaded => _context != null;

    public LLamaContext Context =>
        _context ?? throw new InvalidOperationException("Контекст модели отсутствует.");

    public async Task LoadAsync(string modelPath)
    {
        if (IsLoaded)
            return;

        State = RuntimeState.Loading;

        await Task.Run(() =>
        {
            var parameters = new ModelParams(modelPath)
            {
                ContextSize = 4096,
                GpuLayerCount = 0
            };

            _weights = LLamaWeights.LoadFromFile(parameters);

            _context = _weights.CreateContext(parameters);

            Executor = new LLamaExecutor(this);

            Executor.Initialize();
        });

        ModelPath = modelPath;

        State = RuntimeState.Ready;
    }

    public async Task UnloadAsync()
    {
        if (!IsLoaded)
            return;

        State = RuntimeState.Unloading;

        await Task.Run(() =>
        {
            Executor = null;

            _context?.Dispose();
            _weights?.Dispose();

            _context = null;
            _weights = null;
        });

        ModelPath = null;

        State = RuntimeState.NotLoaded;
    }

    public void Dispose()
    {
        Executor = null;

        _context?.Dispose();
        _weights?.Dispose();

        _context = null;
        _weights = null;
    }
}