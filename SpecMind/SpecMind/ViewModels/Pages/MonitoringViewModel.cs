using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Threading;
using SpecMind.Models;
using SpecMind.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SpecMind.ViewModels.Pages;

public partial class MonitoringViewModel : ViewModelBase
{
    private readonly IHardwareScannerService _scanner = new HardwareScannerService();

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

    private DispatcherTimer? _monitoringTimer;

    private const int MaxDataPoints = 60;

    public MonitoringViewModel()
    {
        _monitoringTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        _monitoringTimer.Tick += async (_, _) =>
        {
            await UpdateMonitoringData();
        };

        _monitoringTimer.Start();
    }

    private async Task UpdateMonitoringData()
    {
        try
        {
            var info = await _scanner.GetHardwareInfoAsync();

            HardwareInfo = info;

            CpuUsageData.Add(info.Sensors.CpuUsage);
            if (CpuUsageData.Count > MaxDataPoints)
                CpuUsageData.RemoveAt(0);

            GpuUsageData.Add(info.Sensors.GpuUsage);
            if (GpuUsageData.Count > MaxDataPoints)
                GpuUsageData.RemoveAt(0);

            CpuTempData.Add(info.Sensors.CpuTemperature);
            if (CpuTempData.Count > MaxDataPoints)
                CpuTempData.RemoveAt(0);

            GpuTempData.Add(info.Sensors.GpuTemperature);
            if (GpuTempData.Count > MaxDataPoints)
                GpuTempData.RemoveAt(0);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    public void StartMonitoring()
    {
        _monitoringTimer?.Start();
    }

    public void StopMonitoring()
    {
        _monitoringTimer?.Stop();
    }
}