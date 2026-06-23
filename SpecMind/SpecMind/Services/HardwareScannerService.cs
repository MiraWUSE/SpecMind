using LibreHardwareMonitor.Hardware;
using SpecMind.Models;
using System;
using System.Collections.Generic;
using System.Management;
using System.Threading.Tasks;

namespace SpecMind.Services;

public class HardwareScannerService : IHardwareScannerService, IDisposable
{
    private Computer _computer;
    private bool _disposed;

    public async Task<HardwareInfo> GetHardwareInfoAsync()
    {
        await Task.Delay(100);

        InitializeHardwareMonitor();

        var hardwareInfo = new HardwareInfo
        {
            DeviceType = GetDeviceType(),
            Cpu = GetCpuInfo(),
            Gpu = GetGpuInfo(),
            Ram = GetRamInfo(),
            Storages = GetStorageInfo(),
            Motherboard = GetMotherboardInfo(),
            Sensors = GetSensorData()
        };

        return hardwareInfo;
    }

    private void InitializeHardwareMonitor()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsControllerEnabled = true,
            IsNetworkEnabled = true,
            IsStorageEnabled = true
        };

        _computer.Open();
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
                float firstTemp = 0;
                float maxTemp = 0;
                int tempCount = 0;

                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        var value = sensor.Value ?? 0;

                        // Собираем все температуры CPU
                        if (value > 0 && value < 150)
                        {
                            tempCount++;
                            if (firstTemp == 0)
                                firstTemp = value;

                            if (value > maxTemp)
                                maxTemp = value;

                            // Проверяем конкретные имена
                            var name = sensor.Name.ToLower();
                            if (name.Contains("package") || name.Contains("cpu") || name.Contains("core"))
                            {
                                sensors.CpuTemperature = value;
                            }
                        }
                    }

                    if (sensor.SensorType == SensorType.Load && sensor.Name.Contains("CPU Total"))
                        sensors.CpuUsage = sensor.Value ?? 0;
                }

                // Если ни один сенсор не подошел по имени, берем максимальную температуру
                if (sensors.CpuTemperature == 0 && maxTemp > 0)
                {
                    sensors.CpuTemperature = maxTemp;
                }

                // Если всё ещё 0, берем первую найденную температуру
                if (sensors.CpuTemperature == 0 && firstTemp > 0)
                {
                    sensors.CpuTemperature = firstTemp;
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
                    if (sensor.SensorType == SensorType.Temperature && sensor.Name.Contains("Motherboard"))
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

                    // Более полная проверка типов
                    if (chassisType == 8 || chassisType == 9 || chassisType == 10 || chassisType == 14)
                        return "Laptop";
                    else if (chassisType == 3)
                        return "Desktop";
                    else if (chassisType == 13)
                        return "All-in-One";
                    else if (chassisType == 4 || chassisType == 5 || chassisType == 6 || chassisType == 7)
                        return "Desktop"; // Low profile, Mini tower, Tower, Portable
                    else if (chassisType == 11 || chassisType == 12)
                        return "Laptop"; // Handheld, Sub Notebook
                    else if (chassisType == 15 || chassisType == 16)
                        return "Desktop"; // Space-saving, Lunch box
                    else if (chassisType == 17 || chassisType == 18)
                        return "Desktop"; // Main system chassis, Expansion chassis
                    else if (chassisType == 19 || chassisType == 20)
                        return "Desktop"; // SubChassis, Bus Expansion chassis
                    else if (chassisType == 21 || chassisType == 22)
                        return "Desktop"; // Peripheral chassis, Storage chassis
                    else if (chassisType == 23 || chassisType == 24)
                        return "Desktop"; // Rack mount chassis, Sealed-case PC
                }
            }
        }
        catch { }

        // Fallback: проверяем через Win32_ComputerSystem
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT PCSystemType FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                var pcSystemType = Convert.ToInt32(obj["PCSystemType"] ?? 0);
                if (pcSystemType == 2)
                    return "Laptop";
                else if (pcSystemType == 1)
                    return "Desktop";
            }
        }
        catch { }

        return "Desktop";
    }

    private CpuInfo GetCpuInfo()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Name, NumberOfCores, NumberOfLogicalProcessors, MaxClockSpeed FROM Win32_Processor");
            foreach (ManagementObject obj in searcher.Get())
            {
                return new CpuInfo
                {
                    Name = obj["Name"]?.ToString() ?? "Unknown",
                    Cores = Convert.ToInt32(obj["NumberOfCores"] ?? 0),
                    Threads = Convert.ToInt32(obj["NumberOfLogicalProcessors"] ?? 0),
                    BaseClock = $"{obj["MaxClockSpeed"]} MHz"
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
            using var searcher = new ManagementObjectSearcher("SELECT Name, DriverVersion FROM Win32_VideoController");
            foreach (ManagementObject obj in searcher.Get())
            {
                var gpuName = obj["Name"]?.ToString() ?? "Unknown";
                var driverVersion = obj["DriverVersion"]?.ToString() ?? "Unknown";

                // Получаем VRAM через LibreHardwareMonitor (более надежно)
                string vram = GetGpuVramFromLHM();

                return new GpuInfo
                {
                    Name = gpuName,
                    Vram = vram,
                    DriverVersion = driverVersion
                };
            }
        }
        catch { }
        return new GpuInfo();
    }

    private string GetGpuVramFromLHM()
    {
        if (_computer == null) return "Unknown";

        foreach (var hardware in _computer.Hardware)
        {
            if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAmd)
            {
                hardware.Update();

                // Ищем сенсор памяти GPU
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.SmallData &&
                        (sensor.Name.Contains("GPU Memory Used") || sensor.Name.Contains("D3D Dedicated Memory Used")))
                    {
                        // Это использованная память, не общий объем
                        continue;
                    }

                    // Пробуем найти сенсор с общим объемом памяти
                    if (sensor.SensorType == SensorType.SmallData && sensor.Name.Contains("Memory Total"))
                    {
                        var mb = sensor.Value ?? 0;
                        var gb = mb / 1024;
                        return $"{gb} GB";
                    }
                }

                // Если не нашли через сенсоры, определяем по названию модели
                var name = hardware.Name?.ToLower() ?? "";

                // База данных популярных видеокарт (VRAM в GB)
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
            using var searcher = new ManagementObjectSearcher("SELECT Capacity, Speed, SMBIOSMemoryType, MemoryType, ConfiguredClockSpeed, Manufacturer, PartNumber FROM Win32_PhysicalMemory");
            ulong totalCapacity = 0;
            string type = "Unknown";
            int maxSpeed = 0;
            string manufacturer = "";
            string partNumber = "";

            foreach (ManagementObject obj in searcher.Get())
            {
                totalCapacity += Convert.ToUInt64(obj["Capacity"] ?? 0);

                // Получаем максимальную скорость
                var currentSpeed = Convert.ToUInt32(obj["Speed"] ?? 0);
                if (currentSpeed > maxSpeed)
                    maxSpeed = (int)currentSpeed;

                var configuredSpeed = Convert.ToUInt32(obj["ConfiguredClockSpeed"] ?? 0);
                if (configuredSpeed > maxSpeed)
                    maxSpeed = (int)configuredSpeed;

                // Получаем производителя и модель
                var mfr = obj["Manufacturer"]?.ToString() ?? "";
                var pn = obj["PartNumber"]?.ToString() ?? "";

                if (!string.IsNullOrEmpty(mfr) && !mfr.Contains("Unknown") && !mfr.Contains("0000"))
                    manufacturer = mfr.Trim();

                if (!string.IsNullOrEmpty(pn) && !pn.Contains("Unknown") && !pn.Contains("0000"))
                    partNumber = pn.Trim();
            }

            // Определяем тип памяти по скорости
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

            // Формируем название модуля RAM
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

            return new RamInfo
            {
                ModuleName = ramName,
                TotalCapacity = $"{totalGB} GB",
                Type = type,
                Speed = maxSpeed > 0 ? $"{maxSpeed} MHz" : "0 MHz"
            };
        }
        catch { }
        return new RamInfo();
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
                var caption = obj["Caption"]?.ToString() ?? "";
                var mediaType = obj["MediaType"]?.ToString() ?? "Unknown";
                var interfaceType = obj["InterfaceType"]?.ToString() ?? "Unknown";

                string storageType = "Unknown";

                // Определяем тип накопителя по множеству признаков
                var modelLower = modelName.ToLower();
                var interfaceLower = interfaceType.ToLower();

                // NVMe накопители (приоритетная проверка)
                if (interfaceLower.Contains("nvme") || interfaceLower.Contains("pcie") ||
                    modelLower.Contains("nvme") || modelLower.Contains("kc3000") ||
                    modelLower.Contains("skc3000") || modelLower.Contains("980") ||
                    modelLower.Contains("970") || modelLower.Contains("960") ||
                    modelLower.Contains("sn850") || modelLower.Contains("sn750") ||
                    modelLower.Contains("sn550") || modelLower.Contains("wd black") ||
                    modelLower.Contains("samsung") && modelLower.Contains("9"))
                {
                    storageType = "NVMe";
                }
                // SSD накопители
                else if (mediaType.Contains("ssd", StringComparison.OrdinalIgnoreCase) ||
                         modelLower.Contains("ssd") ||
                         interfaceLower.Contains("ssd") ||
                         modelLower.Contains("samsung") ||
                         modelLower.Contains("crucial") ||
                         modelLower.Contains("kingston") && !modelLower.Contains("hdd"))
                {
                    storageType = "SSD";
                }
                // HDD накопители
                else if (mediaType.Contains("fixed", StringComparison.OrdinalIgnoreCase) ||
                         interfaceLower.Contains("ide") || interfaceLower.Contains("sata") && !interfaceLower.Contains("pcie"))
                {
                    storageType = "HDD";
                }

                storages.Add(new StorageInfo
                {
                    Name = modelName,
                    Type = storageType,
                    Capacity = $"{sizeGB} GB",
                    HealthStatus = "Good"
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
            // Пробуем Win32_BaseBoard
            using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Product, Version, SMBIOSBIOSVersion FROM Win32_BaseBoard");
            foreach (ManagementObject obj in searcher.Get())
            {
                var manufacturer = obj["Manufacturer"]?.ToString() ?? "";
                var product = obj["Product"]?.ToString() ?? "";
                var biosVersion = obj["SMBIOSBIOSVersion"]?.ToString() ?? "";

                // Если данные есть, возвращаем их
                if (!string.IsNullOrEmpty(manufacturer) && !manufacturer.Contains("To be filled") && !manufacturer.Contains("Default"))
                {
                    return new MotherboardInfo
                    {
                        Manufacturer = manufacturer.Trim(),
                        Model = product.Trim(),
                        BiosVersion = biosVersion.Trim()
                    };
                }
            }
        }
        catch { }

        // Fallback: пробуем Win32_ComputerSystem
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Model FROM Win32_ComputerSystem");
            foreach (ManagementObject obj in searcher.Get())
            {
                return new MotherboardInfo
                {
                    Manufacturer = obj["Manufacturer"]?.ToString() ?? "Unknown",
                    Model = obj["Model"]?.ToString() ?? "Unknown",
                    BiosVersion = "Unknown"
                };
            }
        }
        catch { }

        return new MotherboardInfo();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _computer?.Close();
            _disposed = true;
        }
    }
}