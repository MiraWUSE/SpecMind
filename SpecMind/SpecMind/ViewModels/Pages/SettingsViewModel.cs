using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpecMind.Models;
using SpecMind.Services;
using System.Collections.ObjectModel;

namespace SpecMind.ViewModels.Pages;

public partial class SettingsViewModel : ViewModelBase
{
    [ObservableProperty]
    private string selectedSettingsCategory = "themes";

    [ObservableProperty]
    private ObservableCollection<AppTheme> availableThemes = new();

    public SettingsViewModel()
    {
        LoadThemes();
    }

    private void LoadThemes()
    {
        AvailableThemes =
            new ObservableCollection<AppTheme>(
                ThemeService.GetAvailableThemes());
    }

    [RelayCommand]
    private void SelectCategory(string category)
    {
        SelectedSettingsCategory = category;
    }

    [RelayCommand]
    private void ApplyTheme(AppTheme theme)
    {
        if (theme == null)
            return;

        ThemeService.ApplyTheme(theme);
    }

    [RelayCommand]
    private void ApplyRandomTheme()
    {
        var randomTheme = ThemeService.GenerateRandomTheme();

        ThemeService.ApplyTheme(randomTheme);
    }
}