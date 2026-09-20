using SpecMind.Models;
using SpecMind.Modules.AI.ViewModels;
using SpecMind.Services;
using SpecMind.ViewModels;
using SpecMind.ViewModels.Pages;
using Xunit;

namespace SpecMind.Tests;

public class MonitoringTests
{
    [Fact]
    public async Task TemperatureHistoryPreservesMissingSamplesAndRecovery()
    {
        var snapshot = Snapshot(20);
        snapshot.Sensors.CpuTemperature = null;
        await using var main = new MainWindowViewModel(new FakeScanner(() => Task.FromResult(snapshot)));
        await main.RefreshHardwareAsync();
        snapshot = Snapshot(45);
        await main.RefreshHardwareAsync();
        snapshot.Sensors.CpuTemperature = null;
        await main.RefreshHardwareAsync();
        Assert.Equal(new double?[] { null, 45, null }, main.CpuTempData);
        Assert.Equal(3, main.GpuTempData.Count);
        Assert.Null(main.HardwareInfo.Sensors.CpuTemperature);
    }

    [Fact]
    public async Task OpenPagesAreNotifiedWhenHardwareIsReplaced()
    {
        var snapshot = Snapshot(24);
        await using var main = new MainWindowViewModel(new FakeScanner(() => Task.FromResult(snapshot)));
        var dashboard = Assert.IsType<DashboardViewModel>(main.CurrentPage);
        main.ShowDetailedCommand.Execute(null);
        var detailed = Assert.IsType<DetailedViewModel>(main.CurrentPage);
        main.ShowMonitoringCommand.Execute(null);
        var monitoring = Assert.IsType<MonitoringViewModel>(main.CurrentPage);
        var notifications = new List<object?>();
        foreach (var page in new PagesViewModelBase[] { dashboard, detailed, monitoring })
            page.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(page.HardwareInfo)) notifications.Add(sender);
            };

        await main.RefreshHardwareAsync();

        Assert.Equal(3, notifications.Count);
        Assert.Contains(dashboard, notifications);
        Assert.Contains(detailed, notifications);
        Assert.Contains(monitoring, notifications);
        Assert.Same(snapshot, dashboard.HardwareInfo);
        Assert.Same(snapshot, detailed.HardwareInfo);
        Assert.Same(snapshot, monitoring.HardwareInfo);
        Assert.Equal("Test CPU", dashboard.HardwareInfo.Cpu.Name);
    }

    [Fact]
    public async Task MonitoringUsesOneBoundedHistoryAcrossNavigation()
    {
        var sample = 0;
        await using var main = new MainWindowViewModel(new FakeScanner(() => Task.FromResult(Snapshot(++sample))));
        main.ShowMonitoringCommand.Execute(null);
        var monitoring = Assert.IsType<MonitoringViewModel>(main.CurrentPage);
        main.ShowDashboardCommand.Execute(null);
        for (var i = 0; i < 65; i++) await main.RefreshHardwareAsync();
        main.ShowMonitoringCommand.Execute(null);

        Assert.Same(monitoring, main.CurrentPage);
        Assert.Same(main.CpuUsageData, monitoring.CpuUsageData);
        Assert.Same(main.GpuUsageData, monitoring.GpuUsageData);
        Assert.Same(main.CpuTempData, monitoring.CpuTempData);
        Assert.Same(main.GpuTempData, monitoring.GpuTempData);
        foreach (var data in new[] { monitoring.CpuTempData, monitoring.GpuTempData })
        {
            Assert.Equal(60, data.Count);
            Assert.Equal(6d, data[0]);
            Assert.Equal(65d, data[^1]);
        }
        foreach (var data in new[] { monitoring.CpuUsageData, monitoring.GpuUsageData })
        {
            Assert.Equal(60, data.Count);
            Assert.Equal(6d, data[0]);
            Assert.Equal(65d, data[^1]);
        }
    }

    [Fact]
    public async Task NavigationReusesEveryPageAndPreservesState()
    {
        var scanner = new FakeScanner(() => Task.FromResult(Snapshot(1)));
        await using var main = new MainWindowViewModel(scanner);
        var commands = new[] { main.ShowDashboardCommand, main.ShowDetailedCommand,
            main.ShowMonitoringCommand, main.ShowSettingsCommand, main.ShowExportCommand, main.ShowAICommand };
        var pages = commands.Select(command =>
        {
            command.Execute(null);
            return main.CurrentPage;
        }).ToArray();
        var settings = Assert.IsType<SettingsViewModel>(pages[3]);
        settings.SelectCategoryCommand.Execute("general");
        var ai = Assert.IsType<AIViewModel>(pages[5]);
        ai.UserMessage = "Unsent draft";
        ai.Memory.AddUser("Previous message");
        for (var pass = 0; pass < 5; pass++)
            for (var i = 0; i < commands.Length; i++)
            {
                commands[i].Execute(null);
                Assert.Same(pages[i], main.CurrentPage);
            }
        Assert.Equal("general", settings.SelectedSettingsCategory);
        Assert.Equal("Unsent draft", ai.UserMessage);
        Assert.Equal("Previous message", ai.Memory.Messages[^1].Message);
        Assert.Equal(0, scanner.Calls); // Navigation does not start scans or new timers.
    }

    [Fact]
    public async Task OverlappingRefreshesShareOneScan()
    {
        var pending = new TaskCompletionSource<HardwareInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
        var scanner = new FakeScanner(() => pending.Task);
        await using var main = new MainWindowViewModel(scanner);
        var first = main.RefreshHardwareAsync();
        var second = main.RefreshHardwareAsync();
        Assert.Same(first, second);
        Assert.Equal(1, scanner.Calls);
        pending.SetResult(Snapshot(8));
        await Task.WhenAll(first, second);
        Assert.Single(main.CpuUsageData);
    }

    [Fact]
    public async Task FailedScanCanBeRetriedWithoutAddingEmptySamples()
    {
        var scanner = new FakeScanner(() => Task.FromException<HardwareInfo>(new IOException("Scan failed")));
        await using var main = new MainWindowViewModel(scanner);
        await main.RefreshHardwareAsync();
        Assert.Empty(main.CpuUsageData);
        scanner.Scan = () => Task.FromResult(Snapshot(10));
        await main.RefreshHardwareAsync();
        Assert.Equal(2, scanner.Calls);
        Assert.Equal(10d, Assert.Single(main.CpuUsageData));
    }

    [Fact]
    public async Task DisposalWaitsForScanAndPreventsLateUpdatesAndFurtherPolling()
    {
        var pending = new TaskCompletionSource<HardwareInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
        var scanner = new FakeScanner(() => pending.Task);
        var main = new MainWindowViewModel(scanner);
        var dashboard = Assert.IsType<DashboardViewModel>(main.CurrentPage);
        var changes = 0;
        dashboard.PropertyChanged += (_, _) => changes++;
        var refresh = main.RefreshHardwareAsync();
        var dispose = main.DisposeAsync().AsTask();
        Assert.False(dispose.IsCompleted);
        Assert.Equal(0, scanner.Disposals);
        pending.SetResult(Snapshot(5));
        await Task.WhenAll(refresh, dispose);
        Assert.Empty(main.CpuUsageData);
        Assert.Equal(0, changes);
        main.HardwareInfo = Snapshot(99);
        Assert.Equal(0, changes); // Page unsubscribed from its owner.
        await main.RefreshHardwareAsync();
        await main.DisposeAsync();
        Assert.Equal(1, scanner.Calls);
        Assert.Equal(1, scanner.Disposals);
    }

    [Fact]
    public async Task DisposedScannerRejectsReadsWithoutOpeningHardware()
    {
        var scanner = new HardwareScannerService();
        scanner.Dispose();
        scanner.Dispose();
        await Assert.ThrowsAsync<ObjectDisposedException>(() => scanner.GetHardwareInfoAsync());
    }

    private static HardwareInfo Snapshot(double value) => new()
    {
        Cpu = new CpuInfo { Name = "Test CPU" },
        Sensors = new SensorData { CpuUsage = value, GpuUsage = value,
            CpuTemperature = value, GpuTemperature = value }
    };

    private sealed class FakeScanner(Func<Task<HardwareInfo>> scan) : IHardwareScannerService, IDisposable
    {
        public Func<Task<HardwareInfo>> Scan { get; set; } = scan;
        public int Calls { get; private set; }
        public int Disposals { get; private set; }
        public Task<HardwareInfo> GetHardwareInfoAsync()
        {
            Calls++;
            return Scan();
        }
        public void Dispose() => Disposals++;
    }
}
