using System.Collections.Generic;

namespace SpecMind.Models;

public class HardwareInfo
{
    public CpuInfo Cpu { get; set; } = new();
    public GpuInfo Gpu { get; set; } = new();
    public RamInfo Ram { get; set; } = new();
    public MotherboardInfo Motherboard { get; set; } = new();
    public List<StorageInfo> Storages { get; set; } = new();
    public SensorData Sensors { get; set; } = new();
    public string DeviceType { get; set; } = "Desktop";
}

public class CpuInfo
{
    public string Name { get; set; } = "";
    public int Cores { get; set; }
    public int Threads { get; set; }
    public string BaseClock { get; set; } = "";
    public string MaxClock { get; set; } = "";
    public string CacheL1 { get; set; } = "";
    public string CacheL2 { get; set; } = "";
    public string CacheL3 { get; set; } = "";
    public string Architecture { get; set; } = "";
    public string Socket { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string SerialNumber { get; set; } = "";
}

public class GpuInfo
{
    public string Name { get; set; } = "";
    public string Vram { get; set; } = "";
    public string DriverVersion { get; set; } = "";
    public string DriverDate { get; set; } = "";
    public int CudaCores { get; set; }
    public string Tdp { get; set; } = "";
    public string PcieVersion { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string DeviceId { get; set; } = "";
    public string SerialNumber { get; set; } = "";
}

public class RamInfo
{
    public string ModuleName { get; set; } = "";
    public string TotalCapacity { get; set; } = "";
    public string Type { get; set; } = "";
    public string Speed { get; set; } = "";
    public string FormFactor { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string PartNumber { get; set; } = "";
    public int SlotsUsed { get; set; }
    public int TotalSlots { get; set; }
}

public class MotherboardInfo
{
    public string Manufacturer { get; set; } = "";
    public string Model { get; set; } = "";
    public string BiosVersion { get; set; } = "";
    public string BiosDate { get; set; } = "";
    public string Chipset { get; set; } = "";
    public string SerialNumber { get; set; } = "";
}

public class StorageInfo
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Capacity { get; set; } = "";
    public string HealthStatus { get; set; } = "";
    public string Interface { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public int Temperature { get; set; }
    public int PowerOnHours { get; set; }
    public string FirmwareVersion { get; set; } = "";
}

public class FanInfo
{
    public string Name { get; set; } = "";
    public int SpeedRpm { get; set; }
}

public class SensorData
{
    public double CpuTemperature { get; set; }
    public double GpuTemperature { get; set; }
    public double MotherboardTemperature { get; set; }
    public double CpuUsage { get; set; }
    public double GpuUsage { get; set; }
    public int CpuFanSpeed { get; set; }
    public int GpuFanSpeed { get; set; }
    public double RamUsage { get; set; }
    public List<FanInfo> SystemFans { get; set; } = new();
}