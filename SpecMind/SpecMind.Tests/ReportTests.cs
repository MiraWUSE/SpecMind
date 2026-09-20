using System.Text.Json;
using SpecMind.Models;
using SpecMind.Services;
using Xunit;

namespace SpecMind.Tests;

public class ReportTests
{
    private static HardwareInfo Hardware()
    {
        var h = AITests.Hardware();
        h.Motherboard.Model = "Test Board";
        h.Storages.Add(new StorageInfo { Name = "Test SSD", Capacity = "1 TB", Type = "SSD", Interface = "NVMe" });
        h.Monitors.Add(new MonitorInfo { Name = "Test Monitor", Resolution = "1920x1080" });
        h.Sensors.RamUsage = 35;
        return h;
    }

    [Theory]
    [InlineData("txt")]
    [InlineData("json")]
    [InlineData("csv")]
    [InlineData("html")]
    public async Task EveryFormatWritesHardwareAndOverwritesExistingFile(string format)
    {
        using var temp = new TemporaryDirectory();
        var file = temp.File("report." + format);
        await File.WriteAllTextAsync(file, "OLD CONTENT");
        var hardware = Hardware();
        Assert.Equal(file, await Export(format)(hardware, file));
        var text = await File.ReadAllTextAsync(file);
        foreach (var value in new[] { hardware.Cpu.Name, hardware.Gpu.Name, hardware.Ram.TotalCapacity, "Test SSD", "Test Board" })
            Assert.Contains(value, text);
        Assert.DoesNotContain("OLD CONTENT", text);
    }

    [Fact]
    public async Task JsonPreservesMonitorListAndRamLoad()
    {
        using var temp = new TemporaryDirectory();
        var file = temp.File("report.json");
        await ReportExporterService.ExportToJsonAsync(Hardware(), file);
        var roundTrip = JsonSerializer.Deserialize<HardwareInfo>(await File.ReadAllTextAsync(file))!;
        Assert.Equal("Test Monitor", Assert.Single(roundTrip.Monitors).Name);
        Assert.Equal(35, roundTrip.Sensors.RamUsage);
    }

    [Fact]
    public async Task HtmlEscapesHardwareText()
    {
        using var temp = new TemporaryDirectory();
        var hardware = Hardware();
        hardware.Cpu.Name = "<script>alert('cpu')</script> & Core";
        hardware.Storages[0].Name = "<b>Disk</b>";
        var file = temp.File("report.html");
        await ReportExporterService.ExportToHtmlAsync(hardware, file);
        var text = await File.ReadAllTextAsync(file);
        Assert.DoesNotContain("<script>", text);
        Assert.Contains("&lt;script&gt;", text);
        Assert.Contains("&lt;b&gt;Disk&lt;/b&gt;", text);
        Assert.Contains("<html", text);
    }

    [Fact]
    public async Task CsvQuotesSemicolonsNewlinesAndQuotes()
    {
        using var temp = new TemporaryDirectory();
        var hardware = Hardware();
        hardware.Cpu.Name = "CPU; \"Special\"\nEdition";
        var file = temp.File("report.csv");
        await ReportExporterService.ExportToCsvAsync(hardware, file);
        Assert.Contains("CPU;Название;\"CPU; \"\"Special\"\"\nEdition\"", await File.ReadAllTextAsync(file));
    }

    [Theory]
    [InlineData("txt")]
    [InlineData("json")]
    [InlineData("csv")]
    [InlineData("html")]
    public async Task WriteFailureIsReportedToCaller(string format)
    {
        using var temp = new TemporaryDirectory();
        await Assert.ThrowsAnyAsync<IOException>(() => Export(format)(Hardware(), temp.File("missing/report")));
    }

    private static Func<HardwareInfo, string, Task<string>> Export(string format) => format switch
    {
        "txt" => ReportExporterService.ExportToTxtAsync,
        "json" => ReportExporterService.ExportToJsonAsync,
        "csv" => ReportExporterService.ExportToCsvAsync,
        "html" => ReportExporterService.ExportToHtmlAsync,
        _ => throw new ArgumentOutOfRangeException(nameof(format))
    };
}
