using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpecMind.Models;
using SpecMind.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;

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
    private bool _isExportVisible = false;

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

        // Запускаем сбор данных сразу при старте приложения
        _monitoringTimer.Start();
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

    // ========== НАВИГАЦИЯ ==========

    [RelayCommand]
    private void ShowDetailed()
    {
        IsDashboardVisible = false;
        IsDetailedVisible = true;
        IsMonitoringVisible = false;
        IsSettingsVisible = false;
        IsExportVisible = false;
        StopMonitoring();
    }

    [RelayCommand]
    private void ShowDashboard()
    {
        IsDashboardVisible = true;
        IsDetailedVisible = false;
        IsMonitoringVisible = false;
        IsSettingsVisible = false;
        IsExportVisible = false;
        // Не останавливаем мониторинг - данные уже собираются в фоне
    }

    [RelayCommand]
    private void ShowMonitoring()
    {
        IsDashboardVisible = false;
        IsDetailedVisible = false;
        IsMonitoringVisible = true;
        IsSettingsVisible = false;
        IsExportVisible = false;
        // Таймер уже запущен, данные есть
    }

    [RelayCommand]
    private void ShowSettings()
    {
        IsDashboardVisible = false;
        IsDetailedVisible = false;
        IsMonitoringVisible = false;
        IsSettingsVisible = true;
        IsExportVisible = false;
        SelectedSettingsCategory = "themes";
    }

    [RelayCommand]
    private void ShowExport()
    {
        IsDashboardVisible = false;
        IsDetailedVisible = false;
        IsMonitoringVisible = false;
        IsSettingsVisible = false;
        IsExportVisible = true;
    }

    [RelayCommand]
    private void SelectCategory(string category)
    {
        SelectedSettingsCategory = category;
    }

    // ========== ТЕМЫ ==========

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

    // ========== ЭКСПОРТ ОТЧЁТОВ ==========

    [RelayCommand]
    private async Task ExportTxt()
    {
        await SaveReportAsync("txt", "Текстовый файл", ReportExporterService.ExportToTxtAsync);
    }

    [RelayCommand]
    private async Task ExportJson()
    {
        await SaveReportAsync("json", "JSON файл", ReportExporterService.ExportToJsonAsync);
    }

    [RelayCommand]
    private async Task ExportCsv()
    {
        await SaveReportAsync("csv", "CSV таблица", ReportExporterService.ExportToCsvAsync);
    }

    [RelayCommand]
    private async Task ExportHtml()
    {
        await SaveReportAsync("html", "HTML страница", ReportExporterService.ExportToHtmlAsync);
    }

    private async Task SaveReportAsync(string extension, string description, Func<HardwareInfo, string, Task<string>> exporter)
    {
        try
        {
            var topLevel = Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
            var window = topLevel?.MainWindow;

            if (window == null) return;

            var storageProvider = window.StorageProvider;
            var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = $"Сохранить отчёт ({extension.ToUpper()})",
                DefaultExtension = extension,
                FileTypeChoices = new[]
                {
                    new FilePickerFileType(description)
                    {
                        Patterns = new[] { $"*.{extension}" }
                    }
                },
                SuggestedFileName = $"SpecMind_Report_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}"
            });

            if (file != null)
            {
                var filePath = file.Path.LocalPath;
                await exporter(HardwareInfo, filePath);

                System.Diagnostics.Debug.WriteLine($"Отчёт сохранён: {filePath}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка экспорта: {ex.Message}");
        }
    }
}