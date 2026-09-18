using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Threading;
using SpecMind.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SpecMind.ViewModels.Pages;

public partial class MonitoringViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _main;

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

    public MonitoringViewModel(MainWindowViewModel main)
    {
        _main = main;

        _monitoringTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        _monitoringTimer.Tick += (_, _) => UpdateMonitoringData(); // ← Убран async/await здесь, так как метод теперь синхронный

        _monitoringTimer.Start();
    }

    public HardwareInfo HardwareInfo => _main.HardwareInfo;

    private void UpdateMonitoringData() // ← Метод стал синхронным
    {
        try
        {
            // Читаем свойство напрямую, без await, так как оно уже обновляется в MainWindowViewModel
            var info = _main.HardwareInfo;

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
}