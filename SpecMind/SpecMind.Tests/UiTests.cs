using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using SpecMind.Services;
using SpecMind.ViewModels;
using SpecMind.ViewModels.Pages;
using SpecMind.Views;
using SpecMind.Views.Pages;
using Xunit;

namespace SpecMind.Tests;

public class UiTests
{
    [AvaloniaFact]
    public async Task NavigationButtonsLoadAllSixPages()
    {
        await using var vm = new MainWindowViewModel(new StubScanner());
        var window = new MainWindow { DataContext = vm };
        try
        {
            window.Show();
            var cases = new (string Label, Type View)[] {
                ("Подробно", typeof(DetailedView)), ("Мониторинг", typeof(MonitoringView)),
                ("AI Помощник", typeof(AIView)), ("Экспорт", typeof(ExportView)),
                ("Настройки", typeof(SettingsView)), ("Главная", typeof(DashboardView)) };
            foreach (var (label, viewType) in cases)
            {
                var button = window.GetVisualDescendants().OfType<Button>()
                    .Single(b => b.Content is string text && text.Contains(label));
                Assert.NotNull(button.Command);
                button.Focus();
                window.KeyPressQwerty(PhysicalKey.Enter, RawInputModifiers.None);
                window.KeyReleaseQwerty(PhysicalKey.Enter, RawInputModifiers.None);
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                Assert.Contains(window.GetVisualDescendants(), c => c.GetType() == viewType && ReferenceEquals(((Control)c).DataContext, vm.CurrentPage));
            }
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public async Task DashboardGaugesTrackRefreshAndMissingTemperature()
    {
        var scanner = new StubScanner();
        scanner.Hardware.Sensors.CpuTemperature = null;
        await using var vm = new MainWindowViewModel(scanner);
        var window = new MainWindow { DataContext = vm };
        try
        {
            window.Show();
            await vm.RefreshHardwareAsync();
            Dispatcher.UIThread.RunJobs();
            var gauges = window.GetVisualDescendants().OfType<CircularGauge>().ToArray();
            Assert.Equal(4, gauges.Length);
            Assert.Null(gauges.Single(g => g.Label == "CPU Температура").Value);
            Assert.Equal(12.5, gauges.Single(g => g.Label == "CPU Загрузка").Value);
            scanner.Hardware = AITests.Hardware();
            scanner.Hardware.Sensors.CpuTemperature = 55;
            await vm.RefreshHardwareAsync();
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(55, gauges.Single(g => g.Label == "CPU Температура").Value);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public async Task DetailedPageShowsUnavailableSensorAndHardwareName()
    {
        var scanner = new StubScanner();
        scanner.Hardware.Sensors.CpuTemperature = null;
        await using var vm = new MainWindowViewModel(scanner);
        vm.ShowDetailedCommand.Execute(null);
        var window = new MainWindow { DataContext = vm };
        try
        {
            window.Show();
            await vm.RefreshHardwareAsync();
            Dispatcher.UIThread.RunJobs();
            var texts = window.GetVisualDescendants().OfType<TextBlock>().Select(x => x.Text).ToArray();
            Assert.Contains("Нет данных", texts);
            Assert.Contains(scanner.Hardware.Cpu.Name, texts);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData("nightowl", "#1A1B26")]
    [InlineData("daylight", "#F5F7FA")]
    [InlineData("cyberpunk", "#0D0221")]
    public void ThemesUpdateExistingResources(string key, string expected)
    {
        try
        {
            ThemeService.ApplyTheme("nightowl");
            var brush = Application.Current!.Resources["SM.BackgroundPrimary"];
            ThemeService.ApplyTheme(key);
            Assert.Same(brush, Application.Current.Resources["SM.BackgroundPrimary"]);
            Assert.Equal(Color.Parse(expected), Assert.IsType<SolidColorBrush>(brush).Color);
        }
        finally { ThemeService.ApplyTheme("nightowl"); }
    }

    [AvaloniaFact]
    public async Task SettingsCommandsApplyThemeAndCategory()
    {
        await using var main = new MainWindowViewModel(new StubScanner());
        main.ShowSettingsCommand.Execute(null);
        var settings = Assert.IsType<SettingsViewModel>(main.CurrentPage);
        try
        {
            settings.SelectCategoryCommand.Execute("general");
            Assert.Equal("general", settings.SelectedSettingsCategory);
            var daylight = settings.AvailableThemes.Single(t => t.Name == "Daylight");
            settings.ApplyThemeCommand.Execute(daylight);
            Assert.Same(daylight, ThemeService.CurrentTheme);
            settings.ApplyRandomThemeCommand.Execute(null);
            Assert.True(ThemeService.CurrentTheme.IsRandom);
            Assert.True(ThemeService.CurrentTheme.IsDark);
            Assert.Equal(255, Color.Parse(ThemeService.CurrentTheme.BackgroundPrimary).A);
            settings.ShowDashboardCommand.Execute(null);
            Assert.IsType<DashboardViewModel>(main.CurrentPage);
        }
        finally { ThemeService.ApplyTheme("nightowl"); }
    }

    [AvaloniaFact]
    public void ChartCanDetachChangeDataAndReattach()
    {
        var values = new ObservableCollection<double> { 5, 20, 60 };
        var chart = new SimpleLineChart { Data = values, Width = 600, Height = 200 };
        var window = new Window { Content = chart };
        try
        {
            window.Show();
            window.UpdateLayout();
            values.Add(80);
            Assert.NotEmpty(chart.GetVisualDescendants().OfType<Canvas>().Single().Children);
            window.Content = null;
            values.Add(40);
            chart.Data = new ObservableCollection<double> { 1, 2 };
            window.Content = chart;
            window.UpdateLayout();
            chart.Data.Add(3);
            Assert.NotEmpty(chart.GetVisualDescendants().OfType<Canvas>().Single().Children);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void ViewLocatorHandlesUnknownAndNullModels()
    {
        var locator = new ViewLocator();
        Assert.Null(locator.Build(null));
        Assert.False(locator.Match(new object()));
        Assert.Contains("View не найден", Assert.IsType<TextBlock>(locator.Build(new object())).Text);
    }
}

