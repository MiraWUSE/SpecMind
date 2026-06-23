using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SpecMind.Models;
using SpecMind.Services;
using SpecMind.Views;

namespace SpecMind.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private HardwareInfo _hardwareInfo = new();

    public MainWindowViewModel()
    {
        LoadHardwareData();
    }

    private async void LoadHardwareData()
    {
        IHardwareScannerService scanner = new HardwareScannerService();
        HardwareInfo = await scanner.GetHardwareInfoAsync();
    }

    [RelayCommand]
    private void OpenDetailedWindow()
    {
        var viewModel = new DetailedWindowViewModel(HardwareInfo);
        var window = new DetailedWindow
        {
            DataContext = viewModel
        };
        window.Show();
    }
}