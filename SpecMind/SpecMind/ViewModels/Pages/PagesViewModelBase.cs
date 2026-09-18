using CommunityToolkit.Mvvm.ComponentModel;
using SpecMind.ViewModels;

namespace SpecMind.ViewModels.Pages;

public abstract partial class PagesViewModelBase : ViewModelBase
{
    protected readonly MainWindowViewModel Main;

    protected PagesViewModelBase(MainWindowViewModel main)
    {
        Main = main;
    }
}