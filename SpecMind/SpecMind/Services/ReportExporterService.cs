using SpecMind.Models;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SpecMind.Services;

public class ReportExporterService
{
    public static async Task<string> ExportToTxtAsync(HardwareInfo info, string filePath)
    {
        var sb = new StringBuilder();

        // Заголовок
        sb.AppendLine("╔══════════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║                    SPEC MIND - SYSTEM REPORT                     ║");
        sb.AppendLine("╚══════════════════════════════════════════════════════════════════╝");
        sb.AppendLine();
        sb.AppendLine($"  Дата отчёта: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
        sb.AppendLine($"  Тип устройства: {info.DeviceType}");
        sb.AppendLine();

        // CPU
        sb.AppendLine("──────────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│  ПРОЦЕССОР (CPU)                                                  │");
        sb.AppendLine("└──────────────────────────────────────────────────────────────────┘");
        sb.AppendLine($"  Название:        {info.Cpu.Name}");
        sb.AppendLine($"  Ядер:            {info.Cpu.Cores}");
        sb.AppendLine($"  Потоков:         {info.Cpu.Threads}");
        sb.AppendLine($"  Базовая частота: {info.Cpu.BaseClock}");
        sb.AppendLine($"  Температура:     {info.Sensors.CpuTemperature:F1}°C");
        sb.AppendLine($"  Загрузка:        {info.Sensors.CpuUsage:F1}%");
        sb.AppendLine();

        // GPU
        sb.AppendLine("┌──────────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│  ВИДЕОКАРТА (GPU)                                                 │");
        sb.AppendLine("└──────────────────────────────────────────────────────────────────┘");
        sb.AppendLine($"  Название:        {info.Gpu.Name}");
        sb.AppendLine($"  VRAM:            {info.Gpu.Vram}");
        sb.AppendLine($"  Драйвер:         {info.Gpu.DriverVersion}");
        sb.AppendLine($"  Температура:     {info.Sensors.GpuTemperature:F1}°C");
        sb.AppendLine($"  Загрузка:        {info.Sensors.GpuUsage:F1}%");
        sb.AppendLine($"  Вентилятор:      {info.Sensors.GpuFanSpeed} RPM");
        sb.AppendLine();

        // RAM
        sb.AppendLine("┌──────────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│  ОПЕРАТИВНАЯ ПАМЯТЬ (RAM)                                         │");
        sb.AppendLine("└──────────────────────────────────────────────────────────────────┘");
        sb.AppendLine($"  Название:        {info.Ram.ModuleName}");
        sb.AppendLine($"  Объём:           {info.Ram.TotalCapacity}");
        sb.AppendLine($"  Тип:             {info.Ram.Type}");
        sb.AppendLine($"  Частота:         {info.Ram.Speed}");
        sb.AppendLine();

        // Storage
        sb.AppendLine("┌──────────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│  НАКОПИТЕЛИ (STORAGE)                                             │");
        sb.AppendLine("──────────────────────────────────────────────────────────────────┘");
        foreach (var storage in info.Storages)
        {
            sb.AppendLine($"  ─ {storage.Name}");
            sb.AppendLine($"  │  Тип:      {storage.Type}");
            sb.AppendLine($"  │  Объём:    {storage.Capacity}");
            sb.AppendLine($"  │  Статус:   {storage.HealthStatus}");
            sb.AppendLine($"  └─────────────────────────────────────────");
            sb.AppendLine();
        }

        // Motherboard
        sb.AppendLine("┌──────────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│  МАТЕРИНСКАЯ ПЛАТА                                                │");
        sb.AppendLine("└──────────────────────────────────────────────────────────────────┘");
        sb.AppendLine($"  Производитель:   {info.Motherboard.Manufacturer}");
        sb.AppendLine($"  Модель:          {info.Motherboard.Model}");
        sb.AppendLine($"  BIOS:            {info.Motherboard.BiosVersion}");
        sb.AppendLine();

        // Футер
        sb.AppendLine("════════════════════════════════════════════════════════════════════");
        sb.AppendLine("  Отчёт создан SpecMind - https://github.com/MiraWuse/SpecMind");
        sb.AppendLine("════════════════════════════════════════════════════════════════════");

        var content = sb.ToString();

        await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);

        return filePath;
    }

    public static async Task<string> ExportToDesktopAsync(HardwareInfo info)
    {
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var fileName = $"SpecMind_Report_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
        var filePath = Path.Combine(desktopPath, fileName);

        return await ExportToTxtAsync(info, filePath);
    }
}