namespace SpecMind.Models;

public class CpuInfo
{
    public string Name { get; set; } = "Unknown";
    public int Cores { get; set; }
    public int Threads { get; set; }
    public string BaseClock { get; set; } = "0 GHz";
    public string CurrentTemperature { get; set; } = "0 °C";
}