using Avalonia;
using Avalonia.Headless;
using SpecMind.Models;
using SpecMind.Services;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(SpecMind.Tests.TestAppBuilder))]
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace SpecMind.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

internal sealed class TemporaryDirectory : IDisposable
{
    public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "SpecMindTests", Guid.NewGuid().ToString("N"));
    public TemporaryDirectory() => Directory.CreateDirectory(Path);
    public string File(string name) => System.IO.Path.Combine(Path, name);
    public void Dispose() => Directory.Delete(Path, recursive: true);
}

internal sealed class StubScanner : IHardwareScannerService, IDisposable
{
    public HardwareInfo Hardware { get; set; } = AITests.Hardware();
    public Exception? Error { get; set; }
    public int Calls { get; private set; }
    public int Disposals { get; private set; }
    public Task<HardwareInfo> GetHardwareInfoAsync()
    {
        Calls++;
        return Error is null ? Task.FromResult(Hardware) : Task.FromException<HardwareInfo>(Error);
    }
    public void Dispose() => Disposals++;
}
