using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace SpecMind;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var viewModel = new ViewModels.MainWindowViewModel();
            var window = new Views.MainWindow
            {
                DataContext = viewModel,
            };
            desktop.MainWindow = window;
            // Keep the dispatcher alive until the in-flight scan and native cleanup finish.
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            window.Closed += async (_, _) =>
            {
                var exitCode = 0;
                try
                {
                    await viewModel.DisposeAsync();
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    exitCode = 1;
                }
                finally
                {
                    desktop.Shutdown(exitCode);
                }
            };
            viewModel.StartMonitoring();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
