using System.Collections.ObjectModel;

namespace SpecMind.ViewModels.Pages;

public class MonitoringViewModel : PagesViewModelBase
{
    public MonitoringViewModel(MainWindowViewModel main) : base(main) { }

    public ObservableCollection<double> CpuUsageData => Main.CpuUsageData;
    public ObservableCollection<double> GpuUsageData => Main.GpuUsageData;
    public ObservableCollection<double?> CpuTempData => Main.CpuTempData;
    public ObservableCollection<double?> GpuTempData => Main.GpuTempData;
}
