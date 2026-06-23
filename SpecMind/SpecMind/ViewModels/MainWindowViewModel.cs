using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpecMind.Models;
using SpecMind.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace SpecMind.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private HardwareInfo _hardwareInfo = new();

    [ObservableProperty]
    private bool _isDashboardVisible = true;

    [ObservableProperty]
    private bool _isDetailedVisible = false;

    [ObservableProperty]
    private bool _isMonitoringVisible = false;

    [ObservableProperty]
    private bool _isSettingsVisible = false;

    [ObservableProperty]
    private string _selectedSettingsCategory = "themes";

    [ObservableProperty]
    private ObservableCollection<AppTheme> _availableThemes = new();

    // Данные для графиков
    [ObservableProperty]
    private ObservableCollection<double> _cpuUsageData = new();

    [ObservableProperty]
    private ObservableCollection<double> _gpuUsageData = new();

    [ObservableProperty]
    private ObservableCollection<double> _cpuTempData = new();

    [ObservableProperty]
    private ObservableCollection<double> _gpuTempData = new();

    private readonly IHardwareScannerService _scanner;
    private DispatcherTimer? _monitoringTimer;
    private const int MaxDataPoints = 60;

    public MainWindowViewModel()
    {
        _scanner = new HardwareScannerService();
        LoadHardwareData();
        LoadThemes();

        _monitoringTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _monitoringTimer.Tick += async (s, e) => await UpdateMonitoringData();
    }

    public void StartMonitoring()
    {
        _monitoringTimer?.Start();
    }

    public void StopMonitoring()
    {
        _monitoringTimer?.Stop();
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
            System.Diagnostics.Debug.WriteLine($"Monitoring error: {ex.Message}");
        }
    }

    private async void LoadHardwareData()
    {
        HardwareInfo = await _scanner.GetHardwareInfoAsync();
    }

    private void LoadThemes()
    {
        AvailableThemes = new ObservableCollection<AppTheme>(ThemeService.GetAvailableThemes());
    }

    [RelayCommand]
    private void ShowDetailed()
    {
        IsDashboardVisible = false;
        IsDetailedVisible = true;
        IsMonitoringVisible = false;
        IsSettingsVisible = false;
        StopMonitoring();
    }

    [RelayCommand]
    private void ShowDashboard()
    {
        IsDashboardVisible = true;
        IsDetailedVisible = false;
        IsMonitoringVisible = false;
        IsSettingsVisible = false;
        StopMonitoring();
    }

    [RelayCommand]
    private void ShowMonitoring()
    {
        IsDashboardVisible = false;
        IsDetailedVisible = false;
        IsMonitoringVisible = true;
        IsSettingsVisible = false;
        StartMonitoring();
    }

    [RelayCommand]
    private void ShowSettings()
    {
        IsDashboardVisible = false;
        IsDetailedVisible = false;
        IsMonitoringVisible = false;
        IsSettingsVisible = true;
        SelectedSettingsCategory = "themes";
        StopMonitoring();
    }

    [RelayCommand]
    private void SelectCategory(string category)
    {
        SelectedSettingsCategory = category;
    }

    [RelayCommand]
    private void ApplyTheme(AppTheme theme)
    {
        if (theme != null)
        {
            ThemeService.ApplyTheme(theme);
        }
    }

    [RelayCommand]
    private void ApplyRandomTheme()
    {
        var randomTheme = ThemeService.GenerateRandomTheme();
        ThemeService.ApplyTheme(randomTheme);
    }

    [RelayCommand]
    private async Task ExportReport()
    {
        try
        {
            var filePath = await ReportExporterService.ExportToDesktopAsync(HardwareInfo);

            // Можно показать уведомление (опционально)
            System.Diagnostics.Debug.WriteLine($"Отчёт сохранён: {filePath}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка экспорта: {ex.Message}");
        }
    }

}