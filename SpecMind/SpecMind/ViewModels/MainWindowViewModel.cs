using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpecMind.Models;
using SpecMind.Modules.AI.ViewModels;
using SpecMind.Modules.AI;
using SpecMind.Modules.AI.Services;
using SpecMind.Services;
using SpecMind.ViewModels.Pages;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SpecMind.ViewModels;

public partial class MainWindowViewModel : ViewModelBase, IAsyncDisposable
{
    private const int MaxDataPoints = 60;
    private readonly IHardwareScannerService _scanner;
    private readonly DashboardViewModel _dashboard;
    private readonly DetailedViewModel _detailed;
    private readonly MonitoringViewModel _monitoring;
    private readonly SettingsViewModel _settings;
    private readonly ExportViewModel _export;
    private readonly AIViewModel _ai;
    private readonly AIModule _aiModule;
    private DispatcherTimer _monitoringTimer;
    private Task _refreshTask = Task.CompletedTask;
    private Task _disposeTask;
    private bool _disposed;

    [ObservableProperty]
    private HardwareInfo hardwareInfo = new();

    public ObservableCollection<double> CpuUsageData { get; } = new();
    public ObservableCollection<double> GpuUsageData { get; } = new();
    public ObservableCollection<double> CpuTempData { get; } = new();
    public ObservableCollection<double> GpuTempData { get; } = new();

    [ObservableProperty]
    private ViewModelBase currentPage;

    public MainWindowViewModel() : this(new HardwareScannerService()) { }

    // The owner starts monitoring after composing the application and disposes it on shutdown.
    public MainWindowViewModel(IHardwareScannerService scanner, AIService aiService = null)
    {
        _scanner = scanner ?? throw new ArgumentNullException(nameof(scanner));
        _dashboard = new DashboardViewModel(this);
        _detailed = new DetailedViewModel(this);
        _monitoring = new MonitoringViewModel(this);
        _settings = new SettingsViewModel(this);
        _export = new ExportViewModel(this);
        if (aiService == null)
            _aiModule = new AIModule();
        _ai = new AIViewModel(aiService ?? _aiModule.AIService, () => HardwareInfo);
        CurrentPage = _dashboard;
    }

    public void StartMonitoring()
    {
        if (_disposed || _monitoringTimer != null)
            return;

        _monitoringTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _monitoringTimer.Tick += OnMonitoringTick;
        _monitoringTimer.Start();
        _ = RefreshHardwareAsync();
    }

    private async void OnMonitoringTick(object sender, EventArgs e)
        => await RefreshHardwareAsync();

    // Called on the UI thread; overlapping ticks share the same in-flight scan.
    public Task RefreshHardwareAsync()
    {
        if (_disposed)
            return Task.CompletedTask;
        if (!_refreshTask.IsCompleted)
            return _refreshTask;

        return _refreshTask = RefreshHardwareCoreAsync();
    }

    private async Task RefreshHardwareCoreAsync()
    {
        try
        {
            var info = await _scanner.GetHardwareInfoAsync();
            if (_disposed)
                return;

            HardwareInfo = info;
            AddSample(CpuUsageData, info.Sensors.CpuUsage);
            AddSample(GpuUsageData, info.Sensors.GpuUsage);
            AddSample(CpuTempData, info.Sensors.CpuTemperature);
            AddSample(GpuTempData, info.Sensors.GpuTemperature);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    private static void AddSample(ObservableCollection<double> data, double value)
    {
        data.Add(value);
        if (data.Count > MaxDataPoints)
            data.RemoveAt(0);
    }

    [RelayCommand]
    private void ShowDashboard() => CurrentPage = _dashboard;
    [RelayCommand]
    private void ShowDetailed() => CurrentPage = _detailed;
    [RelayCommand]
    private void ShowMonitoring() => CurrentPage = _monitoring;
    [RelayCommand]
    private void ShowSettings() => CurrentPage = _settings;
    [RelayCommand]
    private void ShowExport() => CurrentPage = _export;
    [RelayCommand]
    private void ShowAI() => CurrentPage = _ai;

    public ValueTask DisposeAsync()
    {
        _disposeTask ??= DisposeCoreAsync();
        return new ValueTask(_disposeTask);
    }

    private async Task DisposeCoreAsync()
    {
        _disposed = true;
        if (_monitoringTimer != null)
        {
            _monitoringTimer.Stop();
            _monitoringTimer.Tick -= OnMonitoringTick;
        }

        _dashboard.Dispose();
        _detailed.Dispose();
        _monitoring.Dispose();
        await _ai.DisposeAsync();
        if (_aiModule != null)
            await _aiModule.DisposeAsync();

        // Do not close native resources while a worker is still reading sensors.
        await _refreshTask;
        if (_scanner is IDisposable disposable)
            disposable.Dispose();
    }
}
