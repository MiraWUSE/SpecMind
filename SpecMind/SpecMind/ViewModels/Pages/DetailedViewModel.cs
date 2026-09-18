using CommunityToolkit.Mvvm.ComponentModel;
using SpecMind.Models;
using SpecMind.Services;

namespace SpecMind.ViewModels.Pages;

public partial class DetailedViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _main;

    public System.Windows.Input.ICommand ShowDashboardCommand => _main.ShowDashboardCommand;

    public DetailedViewModel(MainWindowViewModel main)
    {
        _main = main;
    }

    public HardwareInfo HardwareInfo => _main.HardwareInfo;
}