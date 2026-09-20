using System.Globalization;
using System.Text.Json;
using SpecMind.Models;
using SpecMind.Services;
using SpecMind.ViewModels;
using Xunit;

namespace SpecMind.Tests;

public class TemperatureTests
{
    [Fact]
    public void PackageWinsRegardlessOfSensorOrder()
    {
        (string, double?)[] readings = [("CPU Core #1", 81), ("CPU Package", 65), ("CPU Core #2", 76)];
        Assert.Equal(65, TemperatureReading.SelectCpu(readings));
        Assert.Equal(65, TemperatureReading.SelectCpu(readings.Reverse()));
    }

    [Fact]
    public void AmdControlTemperatureWinsOverCoreReadings()
        => Assert.Equal(72, TemperatureReading.SelectCpu([
            ("CPU Core", 60), ("Core (Tdie)", 67), ("Core (Tctl/Tdie)", 72)]));

    [Fact]
    public void MissingPackageFallsBackToHottestCoreWithoutDistanceToLimit()
        => Assert.Equal(63, TemperatureReading.SelectCpu([
            ("CPU Package", null), ("CPU Core #1", 51), ("CPU Core #2", 63),
            ("CPU Core #1 Distance to TjMax", 90), ("Unrelated", 100)]));

    [Fact]
    public void MissingInvalidAndUnrelatedSensorsRemainUnavailable()
        => Assert.Null(TemperatureReading.SelectCpu([
            ("CPU Package", double.NaN), ("Core #1", 0), ("Core #2", -1),
            ("Core #3", 150), ("Core #4", double.PositiveInfinity), ("Unknown", 50)]));

    [Theory]
    [InlineData(null)]
    [InlineData(0d)]
    [InlineData(double.NaN)]
    public void MissingTemperatureIsNotShownAsNormal(double? reading)
    {
        Assert.Equal("Нет данных", TemperatureReading.Format(reading));
        Assert.Equal("Нет данных", StatusConverter.Instance.Convert(reading, typeof(string), "cpu_temp", CultureInfo.InvariantCulture));
        Assert.Equal("Нет данных", TemperatureConverter.Instance.Convert(reading, typeof(string), null, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ValidTemperatureAndZeroLoadKeepTheirMeaning()
    {
        Assert.Equal("42.5°C", TemperatureReading.Format(42.5, provider: CultureInfo.InvariantCulture));
        Assert.Equal("Норма", StatusConverter.Instance.Convert(0d, typeof(string), "usage", CultureInfo.InvariantCulture));
        Assert.Equal("Критично", StatusConverter.Instance.Convert(90d, typeof(string), "cpu_temp", CultureInfo.InvariantCulture));
    }

    [Fact]
    public async Task ReportsPreserveMissingTemperatureAndAvailableReadings()
    {
        var hardware = new HardwareInfo { Sensors = new SensorData { GpuTemperature = 42 } };
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        try
        {
            foreach (var export in new Func<HardwareInfo, string, Task<string>>[] {
                ReportExporterService.ExportToTxtAsync, ReportExporterService.ExportToCsvAsync,
                ReportExporterService.ExportToHtmlAsync })
            {
                await export(hardware, path);
                var report = await File.ReadAllTextAsync(path);
                Assert.Contains("Нет данных", report);
                Assert.Contains(TemperatureReading.Format(42), System.Net.WebUtility.HtmlDecode(report));
            }
            await ReportExporterService.ExportToJsonAsync(hardware, path);
            using var json = JsonDocument.Parse(await File.ReadAllTextAsync(path));
            Assert.Contains("\"CpuTemperature\": null", json.RootElement.GetRawText());
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
}
