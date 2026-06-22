using System.Collections.Generic;

namespace SpecMind.Models;

public class SensorData
{
    // CPU
    public float CpuTemperature { get; set; }
    public int CpuFanSpeed { get; set; }
    public float CpuUsage { get; set; }

    // GPU
    public float GpuTemperature { get; set; }
    public int GpuFanSpeed { get; set; }
    public float GpuUsage { get; set; }

    // Система
    public float MotherboardTemperature { get; set; }
    public List<FanInfo> SystemFans { get; set; } = new();
}

public class FanInfo
{
    public string Name { get; set; } = "Unknown";
    public int SpeedRpm { get; set; }
}