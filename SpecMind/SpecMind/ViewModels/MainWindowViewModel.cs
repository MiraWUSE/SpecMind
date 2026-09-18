using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpecMind.Models;
using SpecMind.Modules.AI.ViewModels; // <-- ЭТОТ USING ОБЯЗАТЕЛЕН
using SpecMind.Services;
using SpecMind.ViewModels.Pages;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SpecMind.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IHardwareScannerService _scanner;
    private DispatcherTimer? _monitoringTimer;
    private const int MaxDataPoints = 60;

    [ObservableProperty]
    private HardwareInfo hardwareInfo = new();

    [ObservableProperty]
    private ObservableCollection<double> cpuUsageData = new();

    [ObservableProperty]
    private ObservableCollection<double> gpuUsageData = new();

    [ObservableProperty]
    private ObservableCollection<double> cpuTempData = new();

    [ObservableProperty]
    private ObservableCollection<double> gpuTempData = new();

    [ObservableProperty]
    private ViewModelBase currentPage = null!;

    public MainWindowViewModel()
    {
        _scanner = new HardwareScannerService();
        CurrentPage = new DashboardViewModel(this);
        LoadHardwareData();

        _monitoringTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _monitoringTimer.Tick += async (_, _) => await UpdateMonitoringData();
        _monitoringTimer.Start();
    }

    #region Navigation

    [RelayCommand]
    private void ShowDashboard() => CurrentPage = new DashboardViewModel(this);

    [RelayCommand]
    private void ShowDetailed() => CurrentPage = new DetailedViewModel(this);

    [RelayCommand]
    private void ShowMonitoring() => CurrentPage = new MonitoringViewModel(this);

    [RelayCommand]
    private void ShowSettings() => CurrentPage = new SettingsViewModel(this);

    [RelayCommand]
    private void ShowExport() => CurrentPage = new ExportViewModel(this);

    [RelayCommand]
    private void ShowAI() => CurrentPage = new AIViewModel(this); // <-- ТЕПЕРЬ ЭТО РАБОТАЕТ

    #endregion

    private async void LoadHardwareData()
    {
        HardwareInfo = await _scanner.GetHardwareInfoAsync();
    }

    private async Task UpdateMonitoringData()
    {
        try
        {
            var info = await _scanner.GetHardwareInfoAsync();
            HardwareInfo = info;

            CpuUsageData.Add(info.Sensors.CpuUsage);
            if (CpuUsageData.Count > MaxDataPoints) CpuUsageData.RemoveAt(0);

            GpuUsageData.Add(info.Sensors.GpuUsage);
            if (GpuUsageData.Count > MaxDataPoints) GpuUsageData.RemoveAt(0);

            CpuTempData.Add(info.Sensors.CpuTemperature);
            if (CpuTempData.Count > MaxDataPoints) CpuTempData.RemoveAt(0);

            GpuTempData.Add(info.Sensors.GpuTemperature);
            if (GpuTempData.Count > MaxDataPoints) GpuTempData.RemoveAt(0);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }
}