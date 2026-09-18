using Avalonia.Controls;
using Avalonia.Controls.Templates;
using System;
using System.Collections.Generic;

namespace SpecMind;

public class ViewLocator : IDataTemplate
{
    private static readonly Dictionary<Type, Type> ViewMap = new()
    {
        { typeof(SpecMind.ViewModels.Pages.DashboardViewModel), typeof(SpecMind.Views.Pages.DashboardView) },
        { typeof(SpecMind.ViewModels.Pages.DetailedViewModel), typeof(SpecMind.Views.Pages.DetailedView) },
        { typeof(SpecMind.ViewModels.Pages.MonitoringViewModel), typeof(SpecMind.Views.Pages.MonitoringView) },
        { typeof(SpecMind.ViewModels.Pages.SettingsViewModel), typeof(SpecMind.Views.Pages.SettingsView) },
        { typeof(SpecMind.ViewModels.Pages.ExportViewModel), typeof(SpecMind.Views.Pages.ExportView) },
        { typeof(SpecMind.Modules.AI.ViewModels.AIViewModel), typeof(SpecMind.Views.Pages.AIView) }
    };

    public Control? Build(object? data)
    {
        if (data is null)
            return null;

        var dataType = data.GetType();

        if (ViewMap.TryGetValue(dataType, out var viewType))
        {
            var view = Activator.CreateInstance(viewType) as Control;
            if (view != null)
            {
                view.DataContext = data;
                return view;
            }
        }

        return new TextBlock
        {
            Text = $"View не найден для: {dataType.Name}"
        };
    }

    public bool Match(object? data)
    {
        return data is SpecMind.ViewModels.ViewModelBase;
    }
}
