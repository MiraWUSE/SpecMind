using CommunityToolkit.Mvvm.Input;
using SpecMind.ViewModels;
using SpecMind.ViewModels.Pages;
using Xunit;

namespace SpecMind.Tests;

public class ExportCommandTests
{
    [Theory]
    [InlineData("txt")]
    [InlineData("json")]
    [InlineData("csv")]
    [InlineData("html")]
    public async Task CommandsUseSelectedPathAndCurrentSnapshot(string format)
    {
        using var temp = new TemporaryDirectory();
        await using var main = new MainWindowViewModel(new StubScanner());
        await main.RefreshHardwareAsync();
        var file = temp.File("report." + format);
        var vm = new ExportViewModel(main, (extension, description) =>
        {
            Assert.Equal(format, extension);
            Assert.False(string.IsNullOrWhiteSpace(description));
            return Task.FromResult(file);
        });
        await Command(vm, format).ExecuteAsync(null);
        Assert.Contains(main.HardwareInfo.Cpu.Name, await File.ReadAllTextAsync(file));
        Assert.Contains(file, vm.StatusText);
    }

    [Fact]
    public async Task CancelledDialogDoesNotWriteAReport()
    {
        using var temp = new TemporaryDirectory();
        await using var main = new MainWindowViewModel(new StubScanner());
        var vm = new ExportViewModel(main, (_, _) => Task.FromResult<string>(null!));
        await vm.ExportTxtCommand.ExecuteAsync(null);
        Assert.Empty(vm.StatusText);
        Assert.Empty(Directory.GetFiles(temp.Path));
    }

    [Fact]
    public async Task SaveFailureIsVisibleAndRetrySucceeds()
    {
        using var temp = new TemporaryDirectory();
        await using var main = new MainWindowViewModel(new StubScanner());
        var path = temp.File("missing/report.txt");
        var vm = new ExportViewModel(main, (_, _) => Task.FromResult(path));
        await vm.ExportTxtCommand.ExecuteAsync(null);
        Assert.Contains("Не удалось сохранить", vm.StatusText);
        path = temp.File("report.txt");
        await vm.ExportTxtCommand.ExecuteAsync(null);
        Assert.True(File.Exists(path));
        Assert.DoesNotContain("Не удалось", vm.StatusText);
    }

    private static IAsyncRelayCommand Command(ExportViewModel vm, string format) => format switch
    {
        "txt" => vm.ExportTxtCommand, "json" => vm.ExportJsonCommand,
        "csv" => vm.ExportCsvCommand, "html" => vm.ExportHtmlCommand,
        _ => throw new ArgumentOutOfRangeException(nameof(format))
    };
}
