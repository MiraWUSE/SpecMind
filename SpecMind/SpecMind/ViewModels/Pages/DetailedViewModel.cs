using CommunityToolkit.Mvvm.ComponentModel;
using SpecMind.Models;
using SpecMind.Services;

namespace SpecMind.ViewModels.Pages;

public partial class DetailedViewModel : ViewModelBase
{
    private readonly IHardwareScannerService _scanner =
        new HardwareScannerService();

    [ObservableProperty]
    private HardwareInfo hardwareInfo = new();

    public DetailedViewModel()
    {
        LoadHardwareData();
    }

    private async void LoadHardwareData()
    {
        HardwareInfo = await _scanner.GetHardwareInfoAsync();
    }
}