using Avalonia.Controls;
using Avalonia.Controls.Templates;
using SpecMind.ViewModels;

namespace SpecMind;

public class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data == null)
            return null;

        var name = data.GetType().FullName!
            .Replace("ViewModel", "View")
            .Replace("ViewModels", "Views");

        var type = Type.GetType(name);

        if (type == null)
            return new TextBlock
            {
                Text = $"View not found:\n{name}"
            };

        return (Control)Activator.CreateInstance(type)!;
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}