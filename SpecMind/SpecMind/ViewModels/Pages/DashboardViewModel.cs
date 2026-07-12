using SpecMind.Models;

namespace SpecMind.ViewModels.Pages;

public class DashboardViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _main;

    public DashboardViewModel(MainWindowViewModel main)
    {
        _main = main;
    }

    public HardwareInfo HardwareInfo => _main.HardwareInfo;
}