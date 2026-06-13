namespace SpecMind.Models;

public class GpuInfo
{
    public string Name { get; set; } = "Unknown";
    public string Vram { get; set; } = "0 GB";
    public string DriverVersion { get; set; } = "Unknown";
    public string CurrentTemperature { get; set; } = "0 °C";
}