using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpecMind.Models;
using SpecMind.Services;
using System.Collections.ObjectModel;

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
    private bool _isSettingsVisible = false;

    [ObservableProperty]
    private string _selectedSettingsCategory = "themes";

    [ObservableProperty]
    private ObservableCollection<AppTheme> _availableThemes = new();

    public MainWindowViewModel()
    {
        LoadHardwareData();
        LoadThemes();
    }

    private async void LoadHardwareData()
    {
        IHardwareScannerService scanner = new HardwareScannerService();
        HardwareInfo = await scanner.GetHardwareInfoAsync();
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
        IsSettingsVisible = false;
    }

    [RelayCommand]
    private void ShowDashboard()
    {
        IsDashboardVisible = true;
        IsDetailedVisible = false;
        IsSettingsVisible = false;
    }

    [RelayCommand]
    private void ShowSettings()
    {
        IsDashboardVisible = false;
        IsDetailedVisible = false;
        IsSettingsVisible = true;
        SelectedSettingsCategory = "themes";
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
}