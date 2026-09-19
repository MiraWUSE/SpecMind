using SpecMind.Models;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace SpecMind.ViewModels.Pages;

public abstract class PagesViewModelBase : ViewModelBase, IDisposable
{
    protected readonly MainWindowViewModel Main;

    protected PagesViewModelBase(MainWindowViewModel main)
    {
        Main = main;
        Main.PropertyChanged += OnMainPropertyChanged;
    }

    public HardwareInfo HardwareInfo => Main.HardwareInfo;
    public ICommand ShowDashboardCommand => Main.ShowDashboardCommand;

    private void OnMainPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == nameof(Main.HardwareInfo))
            OnPropertyChanged(nameof(HardwareInfo));
    }

    public void Dispose() => Main.PropertyChanged -= OnMainPropertyChanged;
}
