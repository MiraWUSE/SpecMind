using CommunityToolkit.Mvvm.ComponentModel;
using SpecMind.Models;

namespace SpecMind.ViewModels;

public partial class DetailedWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private HardwareInfo _hardwareInfo;

    public DetailedWindowViewModel(HardwareInfo hardwareInfo)
    {
        _hardwareInfo = hardwareInfo;
    }
}