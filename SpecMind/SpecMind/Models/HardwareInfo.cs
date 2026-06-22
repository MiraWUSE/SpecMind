using System.Collections.Generic;

namespace SpecMind.Models;

public class HardwareInfo
{
    public string DeviceType { get; set; } = "Unknown";
    public CpuInfo Cpu { get; set; } = new();
    public GpuInfo Gpu { get; set; } = new();
    public RamInfo Ram { get; set; } = new();
    public List<StorageInfo> Storages { get; set; } = new();
    public MotherboardInfo Motherboard { get; set; } = new();

    // Для AI-анализа (заполнится позже)
    public int HealthScore { get; set; } = 0;
    public List<string> Bottlenecks { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();

    // Данные с датчиков (температуры, вентиляторы)
    public SensorData Sensors { get; set; } = new();
}