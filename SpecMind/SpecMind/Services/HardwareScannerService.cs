using LibreHardwareMonitor.Hardware;
using SpecMind.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace SpecMind.Services;

public class HardwareScannerService : IHardwareScannerService, IDisposable
{
    private Computer _computer;
    private bool _disposed;
    private readonly object _sync = new();

    public Task<HardwareInfo> GetHardwareInfoAsync() => Task.Run(() =>
    {
        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            InitializeHardwareMonitor();

            return new HardwareInfo
            {
                DeviceType = GetDeviceType(),
                Cpu = GetCpuInfo(),
                Gpu = GetGpuInfo(),
                Ram = GetRamInfo(),
                Storages = GetStorageInfo(),
                Motherboard = GetMotherboardInfo(),
                Monitors = GetMonitorInfo(),
                Sensors = GetSensorData()
            };
        }
    });

    private void InitializeHardwareMonitor()
    {
        if (_computer != null)
            return;

        var computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsControllerEnabled = true,
            IsNetworkEnabled = true,
            IsStorageEnabled = true
        };

        try
        {
            computer.Open();
            _computer = computer;
        }
        catch
        {
            computer.Close();
            throw;
        }
    }

    private SensorData GetSensorData()
    {
        var sensors = new SensorData();

        if (_computer == null) return sensors;

        foreach (var hardware in _computer.Hardware)
        {
            hardware.Update();

            // CPU сенсоры
            if (hardware.HardwareType == HardwareType.Cpu)
            {
                List<float> temperatures = new List<float>();

                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        var value = sensor.Value ?? 0;

                        // Собираем все валидные температуры CPU
                        if (value > 0 && value < 150)
                        {
                            temperatures.Add(value);

                            // Ищем Package или Core температуры
                            var name = sensor.Name.ToLower();
                            if (name.Contains("package") || name.Contains("cpu package") || name.Contains("tctl"))
                            {
                                sensors.CpuTemperature = value;
                            }
                        }
                    }

                    if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("CPU Total"))
                        sensors.CpuUsage = sensor.Value ?? 0;
                }

                // Если не нашли Package, берем среднюю или максимальную температуру
                if (sensors.CpuTemperature == 0 && temperatures.Count > 0)
                {
                    sensors.CpuTemperature = temperatures.Count > 1
                        ? temperatures[temperatures.Count - 2] // Предпоследняя (обычно Package)
                        : temperatures[temperatures.Count - 1]; // Последняя
                }

                // Вентиляторы CPU
                foreach (var subHardware in hardware.SubHardware)
                {
                    subHardware.Update();
                    foreach (var sensor in subHardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Fan && sensor.Name.ToLower().Contains("cpu"))
                            sensors.CpuFanSpeed = (int)(sensor.Value ?? 0);
                    }
                }
            }

            // GPU сенсоры
            if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
            {
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("GPU Core"))
                        sensors.GpuTemperature = sensor.Value ?? 0;

                    if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("GPU Core"))
                        sensors.GpuUsage = sensor.Value ?? 0;

                    if (sensor.SensorType == SensorType.Fan && sensor.Name.Contains("GPU"))
                        sensors.GpuFanSpeed = (int)(sensor.Value ?? 0);
                }
            }

            // Материнская плата
            if (hardware.HardwareType == HardwareType.Motherboard)
            {
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature &&
                        (sensor.Name.Contains("Motherboard") || sensor.Name.Contains("System")))
                        sensors.MotherboardTemperature = sensor.Value ?? 0;

                    if (sensor.SensorType == SensorType.Fan)
                    {
                        sensors.SystemFans.Add(new FanInfo
                        {
                            Name = sensor.Name,
                            SpeedRpm = (int)(sensor.Value ?? 0)
                        });
                    }
                }
            }
        }

        return sensors;
    }

    private string GetDeviceType()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT ChassisTypes FROM Win32_SystemEnclosure");
            foreach (ManagementObject obj in searcher.Get())
            {
                var chassisTypes = obj["ChassisTypes"] as ushort[];
                if (chassisTypes != null && chassisTypes.Length > 0)
                {
                    var chassisType = chassisTypes[0];

                    if (chassisType == 8 || chassisType == 9 || chassisType == 10 || chassisType == 14)
                        return "Laptop";
                    else if (chassisType == 3)
                        return "Desktop";
                    else if (chassisType == 13)
                        return "All-in-One";
                }
            }
        }
        catch { }

        return "Desktop";
    }

    private CpuInfo GetCpuInfo()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed, Architecture, SocketDesignation, Manufacturer, L2CacheSize, L3CacheSize FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                var name = obj["Name"]?.ToString() ?? "Unknown";
                var architecture = Convert.ToInt32(obj["Architecture"] ?? 0);
                var socket = obj["SocketDesignation"]?.ToString() ?? "";
                var manufacturer = obj["Manufacturer"]?.ToString() ?? "";

                string archText = architecture switch
                {
                    0 => "x86",
                    1 => "MIPS",
                    2 => "Alpha",
                    3 => "PowerPC",
                    5 => "ARM",
                    6 => "IA64",
                    9 => "x64",
                    _ => "Unknown"
                };

                var baseClock = Convert.ToUInt32(obj["MaxClockSpeed"] ?? 0);
                var maxClock = baseClock > 0 ? (uint)(baseClock * 1.5) : 0;

                // Получаем кэш
                var l2Cache = obj["L2CacheSize"] != null ? Convert.ToUInt32(obj["L2CacheSize"]) : 0;
                var l3Cache = obj["L3CacheSize"] != null ? Convert.ToUInt32(obj["L3CacheSize"]) : 0;

                // L1 кэш обычно равен количеству ядер * 64KB или 32KB
                var cores = Convert.ToInt32(obj["NumberOfCores"] ?? 1);
                var l1Cache = cores * 64; // Примерно

                string l1Text = l1Cache > 0 ? $"{l1Cache} KB" : "Unknown";
                string l2Text = l2Cache > 0 ? $"{l2Cache} KB" : "Unknown";
                string l3Text = l3Cache > 0 ? $"{l3Cache} KB" : "Unknown";

                return new CpuInfo
                {
                    Name = name,
                    Cores = cores,
                    Threads = Convert.ToInt32(obj["NumberOfLogicalProcessors"] ?? 0),
                    BaseClock = $"{baseClock} MHz",
                    MaxClock = maxClock > 0 ? $"{maxClock} MHz" : "Unknown",
                    CacheL1 = l1Text,
                    CacheL2 = l2Text,
                    CacheL3 = l3Text,
                    Architecture = archText,
                    Socket = socket,
                    Manufacturer = manufacturer.Contains("Intel") ? "Intel" :
                                 manufacturer.Contains("AMD") ? "AMD" : "Unknown",
                    SerialNumber = "Unknown"
                };
            }
        }
        catch { }
        return new CpuInfo();
    }

    private GpuInfo GetGpuInfo()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, DriverVersion, DriverDate, PNPDeviceID FROM Win32_VideoController");
            foreach (ManagementObject obj in searcher.Get())
            {
                var gpuName = obj["Name"]?.ToString() ?? "Unknown";
                var driverVersion = obj["DriverVersion"]?.ToString() ?? "Unknown";
                var driverDate = obj["DriverDate"]?.ToString() ?? "Unknown";
                var pnpDeviceId = obj["PNPDeviceID"]?.ToString() ?? "";

                string deviceId = "Unknown";
                if (pnpDeviceId.Contains("VEN_") && pnpDeviceId.Contains("&DEV_"))
                {
                    var venStart = pnpDeviceId.IndexOf("VEN_") + 4;
                    var devStart = pnpDeviceId.IndexOf("&DEV_") + 5;
                    var ven = pnpDeviceId.Substring(venStart, 4);
                    var dev = pnpDeviceId.Substring(devStart, 4);
                    deviceId = $"VEN_{ven}&DEV_{dev}";
                }

                string manufacturer = "Unknown";
                if (gpuName.Contains("NVIDIA") || pnpDeviceId.Contains("VEN_10DE"))
                    manufacturer = "NVIDIA";
                else if (gpuName.Contains("AMD") || gpuName.Contains("Radeon") || pnpDeviceId.Contains("VEN_1002"))
                    manufacturer = "AMD";
                else if (gpuName.Contains("Intel"))
                    manufacturer = "Intel";

                string vram = GetGpuVramFromLHM();
                var gpuDatabase = GetGpuDatabase();
                var gpuKey = gpuName.ToLower();

                int cudaCores = 0;
                string tdp = "Unknown";
                string pcieVersion = "Unknown";

                foreach (var kvp in gpuDatabase)
                {
                    if (gpuKey.Contains(kvp.Key))
                    {
                        cudaCores = kvp.Value.CudaCores;
                        tdp = kvp.Value.Tdp;
                        break;
                    }
                }

                if (pnpDeviceId.Contains("PCI\\VEN_"))
                {
                    pcieVersion = "PCIe 3.0";
                    if (gpuName.Contains("RTX 40") || gpuName.Contains("RX 7000"))
                        pcieVersion = "PCIe 4.0";
                    else if (gpuName.Contains("RTX 30") || gpuName.Contains("RX 6000"))
                        pcieVersion = "PCIe 4.0";
                }

                if (driverDate.Length >= 8)
                {
                    driverDate = $"{driverDate.Substring(6, 2)}.{driverDate.Substring(4, 2)}.{driverDate.Substring(0, 4)}";
                }

                return new GpuInfo
                {
                    Name = gpuName,
                    Vram = vram,
                    DriverVersion = driverVersion,
                    DriverDate = driverDate,
                    CudaCores = cudaCores,
                    Tdp = tdp,
                    PcieVersion = pcieVersion,
                    Manufacturer = manufacturer,
                    DeviceId = deviceId,
                    SerialNumber = "Unknown"
                };
            }
        }
        catch { }
        return new GpuInfo();
    }

    private Dictionary<string, GpuSpecs> GetGpuDatabase()
    {
        return new Dictionary<string, GpuSpecs>
        {
            { "rtx 4090", new GpuSpecs { CudaCores = 16384, Tdp = "450W" } },
            { "rtx 4080", new GpuSpecs { CudaCores = 9728, Tdp = "320W" } },
            { "rtx 4070 ti", new GpuSpecs { CudaCores = 7680, Tdp = "285W" } },
            { "rtx 4070", new GpuSpecs { CudaCores = 5888, Tdp = "200W" } },
            { "rtx 4060 ti", new GpuSpecs { CudaCores = 4352, Tdp = "160W" } },
            { "rtx 4060", new GpuSpecs { CudaCores = 3072, Tdp = "115W" } },
            { "rtx 3090", new GpuSpecs { CudaCores = 10496, Tdp = "350W" } },
            { "rtx 3080 ti", new GpuSpecs { CudaCores = 10240, Tdp = "350W" } },
            { "rtx 3080", new GpuSpecs { CudaCores = 8704, Tdp = "320W" } },
            { "rtx 3070 ti", new GpuSpecs { CudaCores = 6144, Tdp = "290W" } },
            { "rtx 3070", new GpuSpecs { CudaCores = 5888, Tdp = "220W" } },
            { "rtx 3060 ti", new GpuSpecs { CudaCores = 4864, Tdp = "200W" } },
            { "rtx 3060", new GpuSpecs { CudaCores = 3584, Tdp = "170W" } },
            { "rtx 3050", new GpuSpecs { CudaCores = 2560, Tdp = "130W" } },
            { "rtx 2080 ti", new GpuSpecs { CudaCores = 4352, Tdp = "250W" } },
            { "rtx 2080", new GpuSpecs { CudaCores = 2944, Tdp = "215W" } },
            { "rtx 2070", new GpuSpecs { CudaCores = 2304, Tdp = "175W" } },
            { "rtx 2060", new GpuSpecs { CudaCores = 1920, Tdp = "160W" } },
            { "gtx 1080 ti", new GpuSpecs { CudaCores = 3584, Tdp = "250W" } },
            { "gtx 1080", new GpuSpecs { CudaCores = 2560, Tdp = "180W" } },
            { "gtx 1070", new GpuSpecs { CudaCores = 1920, Tdp = "150W" } },
            { "gtx 1060", new GpuSpecs { CudaCores = 1280, Tdp = "120W" } },
            { "gtx 1050 ti", new GpuSpecs { CudaCores = 768, Tdp = "75W" } },
            { "rx 7900 xtx", new GpuSpecs { CudaCores = 6144, Tdp = "355W" } },
            { "rx 7900 xt", new GpuSpecs { CudaCores = 5376, Tdp = "315W" } },
            { "rx 7800 xt", new GpuSpecs { CudaCores = 3840, Tdp = "263W" } },
            { "rx 7700 xt", new GpuSpecs { CudaCores = 3456, Tdp = "245W" } },
            { "rx 7600", new GpuSpecs { CudaCores = 2048, Tdp = "165W" } },
            { "rx 6950 xt", new GpuSpecs { CudaCores = 5120, Tdp = "335W" } },
            { "rx 6900 xt", new GpuSpecs { CudaCores = 5120, Tdp = "300W" } },
            { "rx 6800 xt", new GpuSpecs { CudaCores = 4608, Tdp = "300W" } },
            { "rx 6800", new GpuSpecs { CudaCores = 3840, Tdp = "250W" } },
            { "rx 6700 xt", new GpuSpecs { CudaCores = 2560, Tdp = "230W" } },
            { "rx 6600 xt", new GpuSpecs { CudaCores = 2048, Tdp = "160W" } },
            { "rx 6600", new GpuSpecs { CudaCores = 1792, Tdp = "132W" } },
            { "rx 6500 xt", new GpuSpecs { CudaCores = 1024, Tdp = "107W" } }
        };
    }

    private string GetGpuVramFromLHM()
    {
        if (_computer == null) return "Unknown";

        foreach (var hardware in _computer.Hardware)
        {
            if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
            {
                hardware.Update();

                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.SmallData && sensor.Name.Contains("Memory Total"))
                    {
                        var mb = sensor.Value ?? 0;
                        var gb = mb / 1024;
                        return $"{gb} GB";
                    }
                }

                var name = hardware.Name?.ToLower() ?? "";

                var vramDatabase = new Dictionary<string, int>
                {
                    { "rtx 4090", 24 }, { "rtx 4080", 16 }, { "rtx 4070 ti", 12 },
                    { "rtx 4070", 12 }, { "rtx 4060 ti", 16 }, { "rtx 4060", 8 },
                    { "rtx 3090", 24 }, { "rtx 3080 ti", 12 }, { "rtx 3080", 10 },
                    { "rtx 3070 ti", 8 }, { "rtx 3070", 8 }, { "rtx 3060 ti", 8 },
                    { "rtx 3060", 12 }, { "rtx 3050", 8 },
                    { "rtx 2080 ti", 11 }, { "rtx 2080", 8 }, { "rtx 2070", 8 },
                    { "rtx 2060", 6 },
                    { "gtx 1080 ti", 11 }, { "gtx 1080", 8 }, { "gtx 1070", 8 },
                    { "gtx 1060", 6 }, { "gtx 1050 ti", 4 },
                    { "rx 7900 xtx", 24 }, { "rx 7900 xt", 20 }, { "rx 7800 xt", 16 },
                    { "rx 7700 xt", 12 }, { "rx 7600", 8 },
                    { "rx 6950 xt", 16 }, { "rx 6900 xt", 16 }, { "rx 6800 xt", 16 },
                    { "rx 6800", 16 }, { "rx 6700 xt", 12 }, { "rx 6600 xt", 8 },
                    { "rx 6600", 8 }, { "rx 6500 xt", 4 }
                };

                foreach (var kvp in vramDatabase)
                {
                    if (name.Contains(kvp.Key))
                    {
                        return $"{kvp.Value} GB";
                    }
                }
            }
        }

        return "Unknown";
    }

    private RamInfo GetRamInfo()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Capacity, Speed, SMBIOSMemoryType, MemoryType, ConfiguredClockSpeed, Manufacturer, PartNumber, FormFactor FROM Win32_PhysicalMemory");
            ulong totalCapacity = 0;
            string type = "Unknown";
            int maxSpeed = 0;
            string manufacturer = "";
            string partNumber = "";
            int formFactor = 0;
            int slotCount = 0;

            foreach (ManagementObject obj in searcher.Get())
            {
                totalCapacity += Convert.ToUInt64(obj["Capacity"] ?? 0);
                slotCount++;

                var currentSpeed = Convert.ToUInt32(obj["Speed"] ?? 0);
                if (currentSpeed > maxSpeed)
                    maxSpeed = (int)currentSpeed;

                var configuredSpeed = Convert.ToUInt32(obj["ConfiguredClockSpeed"] ?? 0);
                if (configuredSpeed > maxSpeed)
                    maxSpeed = (int)configuredSpeed;

                var mfr = obj["Manufacturer"]?.ToString() ?? "";
                var pn = obj["PartNumber"]?.ToString() ?? "";
                formFactor = Convert.ToInt32(obj["FormFactor"] ?? 0);

                if (!string.IsNullOrEmpty(mfr) && !mfr.Contains("Unknown") && !mfr.Contains("0000"))
                    manufacturer = mfr.Trim();

                if (!string.IsNullOrEmpty(pn) && !pn.Contains("Unknown") && !pn.Contains("0000"))
                    partNumber = pn.Trim();
            }

            if (maxSpeed > 0)
            {
                type = maxSpeed switch
                {
                    >= 4800 => "DDR5",
                    >= 1600 => "DDR4",
                    >= 800 => "DDR3",
                    _ => "Unknown"
                };
            }

            string formFactorText = formFactor switch
            {
                0 => "Unknown",
                8 => "DIMM",
                12 => "SODIMM",
                _ => "Unknown"
            };

            string ramName = "Неизвестно";
            if (!string.IsNullOrEmpty(manufacturer) && !string.IsNullOrEmpty(partNumber))
            {
                ramName = $"{manufacturer} {partNumber}";
            }
            else if (!string.IsNullOrEmpty(manufacturer))
            {
                ramName = manufacturer;
            }
            else if (!string.IsNullOrEmpty(partNumber))
            {
                ramName = partNumber;
            }

            var totalGB = totalCapacity / (1024 * 1024 * 1024);
            int totalSlots = GetTotalRamSlots();

            return new RamInfo
            {
                ModuleName = ramName,
                TotalCapacity = $"{totalGB} GB",
                Type = type,
                Speed = maxSpeed > 0 ? $"{maxSpeed} MHz" : "0 MHz",
                FormFactor = formFactorText,
                Manufacturer = manufacturer,
                PartNumber = partNumber,
                SlotsUsed = slotCount,
                TotalSlots = totalSlots
            };
        }
        catch { }
        return new RamInfo();
    }

    private int GetTotalRamSlots()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT MemoryDevices FROM Win32_PhysicalMemoryArray");
            foreach (ManagementObject obj in searcher.Get())
            {
                return Convert.ToInt32(obj["MemoryDevices"] ?? 0);
            }
        }
        catch { }
        return 0;
    }

    private List<StorageInfo> GetStorageInfo()
    {
        var storages = new List<StorageInfo>();
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Model, MediaType, Size, InterfaceType, Caption, SerialNumber FROM Win32_DiskDrive");
            foreach (ManagementObject obj in searcher.Get())
            {
                var sizeBytes = Convert.ToUInt64(obj["Size"] ?? 0);
                var sizeGB = sizeBytes / (1024 * 1024 * 1024);

                var modelName = obj["Model"]?.ToString() ?? "Unknown";
                var mediaType = obj["MediaType"]?.ToString() ?? "Unknown";
                var interfaceType = obj["InterfaceType"]?.ToString() ?? "Unknown";
                var serialNumber = obj["SerialNumber"]?.ToString() ?? "Unknown";

                // Убираем лишние пробелы и подчеркивания из serial number
                if (serialNumber.Contains("_"))
                {
                    serialNumber = serialNumber.Replace("_", "").Trim();
                }

                string storageType = "Unknown";
                string interfaceText = interfaceType;

                var modelLower = modelName.ToLower();
                var interfaceLower = interfaceType.ToLower();

                // Определяем тип накопителя
                if (interfaceLower.Contains("nvme") || interfaceLower.Contains("pcie") ||
                    modelLower.Contains("nvme") || modelLower.Contains("skc3000") ||
                    modelLower.Contains("980") || modelLower.Contains("970") ||
                    modelLower.Contains("sn850") || modelLower.Contains("sn750"))
                {
                    storageType = "NVMe";
                    interfaceText = "NVMe/PCIe";
                }
                else if (mediaType.Contains("ssd", StringComparison.OrdinalIgnoreCase) ||
                         modelLower.Contains("ssd") || modelLower.Contains("samsung") ||
                         modelLower.Contains("crucial") || modelLower.Contains("kingston"))
                {
                    storageType = "SSD";
                    interfaceText = interfaceLower.Contains("sata") ? "SATA" : "SATA/Unknown";
                }
                else
                {
                    storageType = "HDD";
                    interfaceText = interfaceLower.Contains("sata") ? "SATA" : "IDE/SATA";
                }

                storages.Add(new StorageInfo
                {
                    Name = modelName,
                    Type = storageType,
                    Capacity = $"{sizeGB} GB",
                    HealthStatus = "Good",
                    Interface = interfaceText,
                    SerialNumber = serialNumber,
                    Temperature = 0,
                    PowerOnHours = 0,
                    FirmwareVersion = "Unknown"
                });
            }
        }
        catch { }
        return storages;
    }

    private MotherboardInfo GetMotherboardInfo()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Product, Version FROM Win32_BaseBoard");
            foreach (ManagementObject obj in searcher.Get())
            {
                var manufacturer = obj["Manufacturer"]?.ToString() ?? "";
                var product = obj["Product"]?.ToString() ?? "";
                var version = obj["Version"]?.ToString() ?? "";

                if (!string.IsNullOrEmpty(manufacturer) && !manufacturer.Contains("To be filled") && !manufacturer.Contains("Default"))
                {
                    string biosDate = GetBiosDate();
                    string chipset = GetChipset();

                    return new MotherboardInfo
                    {
                        Manufacturer = manufacturer.Trim(),
                        Model = product.Trim(),
                        BiosVersion = version.Trim(),
                        BiosDate = biosDate,
                        Chipset = chipset,
                        SerialNumber = "Unknown"
                    };
                }
            }
        }
        catch { }

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Model FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                return new MotherboardInfo
                {
                    Manufacturer = obj["Manufacturer"]?.ToString() ?? "Unknown",
                    Model = obj["Model"]?.ToString() ?? "Unknown",
                    BiosVersion = "Unknown",
                    BiosDate = GetBiosDate(),
                    Chipset = GetChipset(),
                    SerialNumber = "Unknown"
                };
            }
        }
        catch { }

        return new MotherboardInfo();
    }

    private string GetBiosDate()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT ReleaseDate FROM Win32_BIOS");
            foreach (ManagementObject obj in searcher.Get())
            {
                var releaseDate = obj["ReleaseDate"]?.ToString() ?? "";
                if (releaseDate.Length >= 8)
                {
                    return $"{releaseDate.Substring(6, 2)}.{releaseDate.Substring(4, 2)}.{releaseDate.Substring(0, 4)}";
                }
            }
        }
        catch { }
        return "Unknown";
    }

    private string GetChipset()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_ServerFeature");
            // Чипсет сложно получить через WMI, пробуем через LibreHardwareMonitor
            if (_computer != null)
            {
                foreach (var hardware in _computer.Hardware)
                {
                    if (hardware.HardwareType == HardwareType.Motherboard)
                    {
                        // Ищем информацию о чипсете в названии
                        var name = hardware.Name?.ToLower() ?? "";
                        if (name.Contains("intel") || name.Contains("amd") || name.Contains("chipset"))
                        {
                            return hardware.Name;
                        }
                    }
                }
            }
        }
        catch { }
        return "Unknown";
    }


    private List<MonitorInfo> GetMonitorInfo()
    {
        var monitors = new List<MonitorInfo>();

        // Получаем информацию о мониторах через WMI (root\wmi)
        try
        {
            using var searcher = new ManagementObjectSearcher(@"root\wmi", "SELECT * FROM WmiMonitorID");
            foreach (ManagementObject obj in searcher.Get())
            {
                var monitor = new MonitorInfo
                {
                    Manufacturer = GetMonitorString(obj["ManufacturerName"]),
                    Model = GetMonitorString(obj["UserFriendlyName"]),
                    SerialNumber = GetMonitorString(obj["SerialNumberID"]),
                    IsActive = true
                };
                monitors.Add(monitor);
            }
        }
        catch { }

        // Получаем тип подключения
        try
        {
            using var searcher = new ManagementObjectSearcher(@"root\wmi", "SELECT * FROM WmiMonitorConnectionParams");
            int i = 0;
            foreach (ManagementObject obj in searcher.Get())
            {
                if (i < monitors.Count)
                {
                    var connectionType = Convert.ToInt32(obj["VideoOutputTechnology"]);
                    monitors[i].ConnectionType = connectionType switch
                    {
                        1 => "VGA",
                        5 => "DVI",
                        6 => "HDMI",
                        8 => "DisplayPort",
                        7 => "LVDS (встроенный)",
                        9 => "SDI",
                        10 => "USB-C / Thunderbolt",
                        _ => $"Type {connectionType}"
                    };
                }
                i++;
            }
        }
        catch { }

        // Получаем разрешение и частоту обновления
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT CurrentHorizontalResolution, CurrentVerticalResolution, CurrentRefreshRate FROM Win32_VideoController");
            int i = 0;
            foreach (ManagementObject obj in searcher.Get())
            {
                if (i < monitors.Count)
                {
                    var hRes = obj["CurrentHorizontalResolution"]?.ToString() ?? "?";
                    var vRes = obj["CurrentVerticalResolution"]?.ToString() ?? "?";
                    var refresh = obj["CurrentRefreshRate"]?.ToString() ?? "?";
                    monitors[i].Resolution = $"{hRes} x {vRes}";
                    monitors[i].RefreshRate = $"{refresh} Hz";
                }
                i++;
            }
        }
        catch { }

        // Если не удалось получить разрешение — ставим заглушку
        foreach (var m in monitors)
        {
            if (string.IsNullOrEmpty(m.Resolution)) m.Resolution = "Unknown";
            if (string.IsNullOrEmpty(m.RefreshRate)) m.RefreshRate = "Unknown";
            if (string.IsNullOrEmpty(m.ConnectionType)) m.ConnectionType = "Unknown";
            if (string.IsNullOrEmpty(m.Name)) m.Name = $"{m.Manufacturer} {m.Model}";
        }

        return monitors;
    }

    private string GetMonitorString(object value)
    {
        if (value is ushort[] chars)
        {
            var str = new string(chars.Select(c => (char)c).ToArray());
            return str.TrimEnd('\0').Trim();
        }
        return value?.ToString() ?? "Unknown";
    }

    public void Dispose()
    {
        lock (_sync)
        {
            if (_disposed)
                return;
            _disposed = true;
            _computer?.Close();
            _computer = null;
        }
    }
}

public class GpuSpecs
{
    public int CudaCores { get; set; }
    public string Tdp { get; set; } = "";
}
