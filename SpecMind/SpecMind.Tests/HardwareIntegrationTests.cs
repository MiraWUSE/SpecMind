using SpecMind.Services;
using Xunit;

namespace SpecMind.Tests;

public class HardwareIntegrationTests
{
    [HardwareFact]
    public async Task RealScannerReadsHardwareTwiceAndCloses()
    {
        using var scanner = new HardwareScannerService();
        for (var i = 0; i < 2; i++)
        {
            var hardware = await scanner.GetHardwareInfoAsync();
            Assert.False(string.IsNullOrWhiteSpace(hardware.Cpu.Name));
            Assert.False(string.IsNullOrWhiteSpace(hardware.Ram.TotalCapacity));
            Assert.InRange(hardware.Sensors.CpuUsage, 0, 100);
            if (hardware.Sensors.CpuTemperature.HasValue)
                Assert.True(TemperatureReading.IsValid(hardware.Sensors.CpuTemperature));
        }
        scanner.Dispose();
        await Assert.ThrowsAsync<ObjectDisposedException>(() => scanner.GetHardwareInfoAsync());
    }
}

public sealed class HardwareFactAttribute : FactAttribute
{
    public HardwareFactAttribute()
    {
        if (!OperatingSystem.IsWindows() || Environment.GetEnvironmentVariable("SPECMIND_RUN_HARDWARE_TESTS") != "1")
            Skip = "Opt in on Windows with SPECMIND_RUN_HARDWARE_TESTS=1; requires access to physical hardware.";
    }
}
