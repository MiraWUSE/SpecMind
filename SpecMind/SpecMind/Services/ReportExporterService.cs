using SpecMind.Models;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpecMind.Services;

public class ReportExporterService
{
    // ============ TXT ============
    public static async Task<string> ExportToTxtAsync(HardwareInfo info, string filePath)
    {
        var sb = new StringBuilder();

        sb.AppendLine("╔══════════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║                    SPEC MIND - SYSTEM REPORT                     ║");
        sb.AppendLine("╚══════════════════════════════════════════════════════════════════╝");
        sb.AppendLine();
        sb.AppendLine($"  Дата отчёта: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
        sb.AppendLine($"  Тип устройства: {info.DeviceType}");
        sb.AppendLine();

        // CPU
        sb.AppendLine("┌──────────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│  ПРОЦЕССОР (CPU)                                                  │");
        sb.AppendLine("└──────────────────────────────────────────────────────────────────┘");
        sb.AppendLine($"  Название:        {info.Cpu.Name}");
        sb.AppendLine($"  Производитель:   {info.Cpu.Manufacturer}");
        sb.AppendLine($"  Ядер:            {info.Cpu.Cores}");
        sb.AppendLine($"  Потоков:         {info.Cpu.Threads}");
        sb.AppendLine($"  Базовая частота: {info.Cpu.BaseClock}");
        sb.AppendLine($"  Макс. частота:   {info.Cpu.MaxClock}");
        sb.AppendLine($"  Архитектура:     {info.Cpu.Architecture}");
        sb.AppendLine($"  Сокет:           {info.Cpu.Socket}");
        sb.AppendLine($"  Кэш L1/L2/L3:    {info.Cpu.CacheL1} / {info.Cpu.CacheL2} / {info.Cpu.CacheL3}");
        sb.AppendLine($"  Температура:     {info.Sensors.CpuTemperature:F1}°C");
        sb.AppendLine($"  Загрузка:        {info.Sensors.CpuUsage:F1}%");
        sb.AppendLine();

        // GPU
        sb.AppendLine("┌──────────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│  ВИДЕОКАРТА (GPU)                                                 │");
        sb.AppendLine("└──────────────────────────────────────────────────────────────────┘");
        sb.AppendLine($"  Название:        {info.Gpu.Name}");
        sb.AppendLine($"  Производитель:   {info.Gpu.Manufacturer}");
        sb.AppendLine($"  VRAM:            {info.Gpu.Vram}");
        sb.AppendLine($"  CUDA Cores:      {info.Gpu.CudaCores}");
        sb.AppendLine($"  TDP:             {info.Gpu.Tdp}");
        sb.AppendLine($"  PCIe версия:     {info.Gpu.PcieVersion}");
        sb.AppendLine($"  Device ID:       {info.Gpu.DeviceId}");
        sb.AppendLine($"  Драйвер:         {info.Gpu.DriverVersion}");
        sb.AppendLine($"  Дата драйвера:   {info.Gpu.DriverDate}");
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
        sb.AppendLine($"  Form Factor:     {info.Ram.FormFactor}");
        sb.AppendLine($"  Производитель:   {info.Ram.Manufacturer}");
        sb.AppendLine($"  Part Number:     {info.Ram.PartNumber}");
        sb.AppendLine($"  Слоты:           {info.Ram.SlotsUsed} / {info.Ram.TotalSlots}");
        sb.AppendLine();

        // Storage
        sb.AppendLine("┌──────────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│  НАКОПИТЕЛИ (STORAGE)                                             │");
        sb.AppendLine("└──────────────────────────────────────────────────────────────────┘");
        foreach (var storage in info.Storages)
        {
            sb.AppendLine($"  ─ {storage.Name}");
            sb.AppendLine($"  │  Тип:         {storage.Type}");
            sb.AppendLine($"  │  Объём:       {storage.Capacity}");
            sb.AppendLine($"  │  Интерфейс:   {storage.Interface}");
            sb.AppendLine($"  │  S/N:         {storage.SerialNumber}");
            sb.AppendLine($"  │  Статус:      {storage.HealthStatus}");
            sb.AppendLine($"  └─────────────────────────────────────────");
            sb.AppendLine();
        }

        // Motherboard
        sb.AppendLine("┌──────────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│  МАТЕРИНСКАЯ ПЛАТА                                                │");
        sb.AppendLine("└──────────────────────────────────────────────────────────────────┘");
        sb.AppendLine($"  Производитель:   {info.Motherboard.Manufacturer}");
        sb.AppendLine($"  Модель:          {info.Motherboard.Model}");
        sb.AppendLine($"  Чипсет:          {info.Motherboard.Chipset}");
        sb.AppendLine($"  BIOS Version:    {info.Motherboard.BiosVersion}");
        sb.AppendLine($"  BIOS Date:       {info.Motherboard.BiosDate}");
        sb.AppendLine($"  Serial Number:   {info.Motherboard.SerialNumber}");
        sb.AppendLine();

        // Monitors
        sb.AppendLine("──────────────────────────────────────────────────────────────────┐");
        sb.AppendLine("│  МОНИТОРЫ (MONITORS)                                              │");
        sb.AppendLine("└──────────────────────────────────────────────────────────────────");
        foreach (var monitor in info.Monitors)
        {
            sb.AppendLine($"   {monitor.Name}");
            sb.AppendLine($"  │  Производитель:  {monitor.Manufacturer}");
            sb.AppendLine($"  │  Модель:         {monitor.Model}");
            sb.AppendLine($"  │  Разрешение:     {monitor.Resolution}");
            sb.AppendLine($"  │  Частота:        {monitor.RefreshRate}");
            sb.AppendLine($"  │  Подключение:    {monitor.ConnectionType}");
            sb.AppendLine($"  │  S/N:            {monitor.SerialNumber}");
            sb.AppendLine($"  └─────────────────────────────────────────");
            sb.AppendLine();
        }

        // Футер
        sb.AppendLine("════════════════════════════════════════════════════════════════════");
        sb.AppendLine("  Отчёт создан SpecMind - https://github.com/MiraWuse/SpecMind");
        sb.AppendLine("════════════════════════════════════════════════════════════════════");

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
        return filePath;
    }

    // ============ JSON ============
    public static async Task<string> ExportToJsonAsync(HardwareInfo info, string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        var report = new
        {
            ReportDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"),
            DeviceType = info.DeviceType,
            Cpu = info.Cpu,
            Gpu = info.Gpu,
            Ram = info.Ram,
            Storages = info.Storages,
            Motherboard = info.Motherboard,
            Sensors = new
            {
                CpuTemperature = info.Sensors.CpuTemperature,
                GpuTemperature = info.Sensors.GpuTemperature,
                CpuUsage = info.Sensors.CpuUsage,
                GpuUsage = info.Sensors.GpuUsage,
                GpuFanSpeed = info.Sensors.GpuFanSpeed,
                MotherboardTemperature = info.Sensors.MotherboardTemperature,
                CpuFanSpeed = info.Sensors.CpuFanSpeed,
                SystemFans = info.Sensors.SystemFans
            }
        };

        var json = JsonSerializer.Serialize(report, options);
        await File.WriteAllTextAsync(filePath, json, Encoding.UTF8);
        return filePath;
    }

    // ============ CSV ============
    public static async Task<string> ExportToCsvAsync(HardwareInfo info, string filePath)
    {
        var sb = new StringBuilder();

        // Заголовки
        sb.AppendLine("Категория;Параметр;Значение");

        // Общие
        sb.AppendLine($"Общие;Дата отчёта;{DateTime.Now:dd.MM.yyyy HH:mm:ss}");
        sb.AppendLine($"Общие;Тип устройства;{info.DeviceType}");

        // CPU
        sb.AppendLine($"CPU;Название;{info.Cpu.Name}");
        sb.AppendLine($"CPU;Производитель;{info.Cpu.Manufacturer}");
        sb.AppendLine($"CPU;Ядер;{info.Cpu.Cores}");
        sb.AppendLine($"CPU;Потоков;{info.Cpu.Threads}");
        sb.AppendLine($"CPU;Базовая частота;{info.Cpu.BaseClock}");
        sb.AppendLine($"CPU;Макс. частота;{info.Cpu.MaxClock}");
        sb.AppendLine($"CPU;Архитектура;{info.Cpu.Architecture}");
        sb.AppendLine($"CPU;Сокет;{info.Cpu.Socket}");
        sb.AppendLine($"CPU;Кэш L1;{info.Cpu.CacheL1}");
        sb.AppendLine($"CPU;Кэш L2;{info.Cpu.CacheL2}");
        sb.AppendLine($"CPU;Кэш L3;{info.Cpu.CacheL3}");
        sb.AppendLine($"CPU;Температура;{info.Sensors.CpuTemperature:F1}°C");
        sb.AppendLine($"CPU;Загрузка;{info.Sensors.CpuUsage:F1}%");

        // GPU
        sb.AppendLine($"GPU;Название;{info.Gpu.Name}");
        sb.AppendLine($"GPU;Производитель;{info.Gpu.Manufacturer}");
        sb.AppendLine($"GPU;VRAM;{info.Gpu.Vram}");
        sb.AppendLine($"GPU;CUDA Cores;{info.Gpu.CudaCores}");
        sb.AppendLine($"GPU;TDP;{info.Gpu.Tdp}");
        sb.AppendLine($"GPU;PCIe версия;{info.Gpu.PcieVersion}");
        sb.AppendLine($"GPU;Device ID;{info.Gpu.DeviceId}");
        sb.AppendLine($"GPU;Драйвер;{info.Gpu.DriverVersion}");
        sb.AppendLine($"GPU;Дата драйвера;{info.Gpu.DriverDate}");
        sb.AppendLine($"GPU;Температура;{info.Sensors.GpuTemperature:F1}°C");
        sb.AppendLine($"GPU;Загрузка;{info.Sensors.GpuUsage:F1}%");
        sb.AppendLine($"GPU;Вентилятор;{info.Sensors.GpuFanSpeed} RPM");

        // RAM
        sb.AppendLine($"RAM;Название;{info.Ram.ModuleName}");
        sb.AppendLine($"RAM;Объём;{info.Ram.TotalCapacity}");
        sb.AppendLine($"RAM;Тип;{info.Ram.Type}");
        sb.AppendLine($"RAM;Частота;{info.Ram.Speed}");
        sb.AppendLine($"RAM;Form Factor;{info.Ram.FormFactor}");
        sb.AppendLine($"RAM;Производитель;{info.Ram.Manufacturer}");
        sb.AppendLine($"RAM;Part Number;{info.Ram.PartNumber}");
        sb.AppendLine($"RAM;Слоты;{info.Ram.SlotsUsed} / {info.Ram.TotalSlots}");

        // Storage
        for (int i = 0; i < info.Storages.Count; i++)
        {
            var s = info.Storages[i];
            sb.AppendLine($"Storage_{i + 1};Название;{s.Name}");
            sb.AppendLine($"Storage_{i + 1};Тип;{s.Type}");
            sb.AppendLine($"Storage_{i + 1};Объём;{s.Capacity}");
            sb.AppendLine($"Storage_{i + 1};Интерфейс;{s.Interface}");
            sb.AppendLine($"Storage_{i + 1};S/N;{s.SerialNumber}");
            sb.AppendLine($"Storage_{i + 1};Статус;{s.HealthStatus}");
        }

        // Motherboard
        sb.AppendLine($"Motherboard;Производитель;{info.Motherboard.Manufacturer}");
        sb.AppendLine($"Motherboard;Модель;{info.Motherboard.Model}");
        sb.AppendLine($"Motherboard;Чипсет;{info.Motherboard.Chipset}");
        sb.AppendLine($"Motherboard;BIOS Version;{info.Motherboard.BiosVersion}");
        sb.AppendLine($"Motherboard;BIOS Date;{info.Motherboard.BiosDate}");
        sb.AppendLine($"Motherboard;Serial Number;{info.Motherboard.SerialNumber}");

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
        return filePath;
    }

    // ============ HTML ============
    public static async Task<string> ExportToHtmlAsync(HardwareInfo info, string filePath)
    {
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"ru\">");
        sb.AppendLine("<head>");
        sb.AppendLine("  <meta charset=\"UTF-8\">");
        sb.AppendLine("  <title>SpecMind Report</title>");
        sb.AppendLine("  <style>");
        sb.AppendLine("    body { font-family: 'Segoe UI', Arial, sans-serif; background: #1a1a2e; color: #e0e0e0; padding: 20px; margin: 0; }");
        sb.AppendLine("    .container { max-width: 900px; margin: 0 auto; }");
        sb.AppendLine("    h1 { color: #7aa9e0; border-bottom: 2px solid #7aa9e0; padding-bottom: 10px; }");
        sb.AppendLine("    h2 { color: #7aa9e0; margin-top: 30px; }");
        sb.AppendLine("    .card { background: #16213e; border-radius: 12px; padding: 20px; margin: 15px 0; }");
        sb.AppendLine("    .row { display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px solid #0f3460; }");
        sb.AppendLine("    .row:last-child { border-bottom: none; }");
        sb.AppendLine("    .label { color: #a0a0b0; }");
        sb.AppendLine("    .value { color: #ffffff; font-weight: bold; }");
        sb.AppendLine("    .good { color: #9ece6a; }");
        sb.AppendLine("    .warning { color: #e0af68; }");
        sb.AppendLine("    .critical { color: #f7768e; }");
        sb.AppendLine("    .footer { text-align: center; margin-top: 40px; color: #606070; font-size: 12px; }");
        sb.AppendLine("  </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("<div class=\"container\">");
        sb.AppendLine($"  <h1>SpecMind System Report</h1>");
        sb.AppendLine($"  <p><strong>Дата:</strong> {DateTime.Now:dd.MM.yyyy HH:mm:ss} | <strong>Тип:</strong> {info.DeviceType}</p>");

        // CPU
        sb.AppendLine("  <div class=\"card\">");
        sb.AppendLine("    <h2>Процессор</h2>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Название</span><span class=\"value\">{info.Cpu.Name}</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Производитель</span><span class=\"value\">{info.Cpu.Manufacturer}</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Ядер / Потоков</span><span class=\"value\">{info.Cpu.Cores} / {info.Cpu.Threads}</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Частоты</span><span class=\"value\">{info.Cpu.BaseClock} / {info.Cpu.MaxClock}</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Архитектура / Сокет</span><span class=\"value\">{info.Cpu.Architecture} / {info.Cpu.Socket}</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Кэш L1/L2/L3</span><span class=\"value\">{info.Cpu.CacheL1} / {info.Cpu.CacheL2} / {info.Cpu.CacheL3}</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Температура</span><span class=\"value\">{info.Sensors.CpuTemperature:F1}°C</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Загрузка</span><span class=\"value\">{info.Sensors.CpuUsage:F1}%</span></div>");
        sb.AppendLine("  </div>");

        // GPU
        sb.AppendLine("  <div class=\"card\">");
        sb.AppendLine("    <h2>Видеокарта</h2>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Название</span><span class=\"value\">{info.Gpu.Name}</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Производитель</span><span class=\"value\">{info.Gpu.Manufacturer}</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">VRAM</span><span class=\"value\">{info.Gpu.Vram}</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">CUDA Cores</span><span class=\"value\">{info.Gpu.CudaCores}</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">TDP</span><span class=\"value\">{info.Gpu.Tdp}</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">PCIe версия</span><span class=\"value\">{info.Gpu.PcieVersion}</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Драйвер</span><span class=\"value\">{info.Gpu.DriverVersion} ({info.Gpu.DriverDate})</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Температура</span><span class=\"value\">{info.Sensors.GpuTemperature:F1}°C</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Загрузка</span><span class=\"value\">{info.Sensors.GpuUsage:F1}%</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Вентилятор</span><span class=\"value\">{info.Sensors.GpuFanSpeed} RPM</span></div>");
        sb.AppendLine("  </div>");

        // RAM
        sb.AppendLine("  <div class=\"card\">");
        sb.AppendLine("    <h2>Оперативная память</h2>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Модуль</span><span class=\"value\">{info.Ram.ModuleName}</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Объём</span><span class=\"value\">{info.Ram.TotalCapacity}</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Тип / Частота</span><span class=\"value\">{info.Ram.Type} / {info.Ram.Speed}</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Form Factor</span><span class=\"value\">{info.Ram.FormFactor}</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Производитель</span><span class=\"value\">{info.Ram.Manufacturer}</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Слоты</span><span class=\"value\">{info.Ram.SlotsUsed} / {info.Ram.TotalSlots}</span></div>");
        sb.AppendLine("  </div>");

        // Storage
        sb.AppendLine("  <div class=\"card\">");
        sb.AppendLine("    <h2>Накопители</h2>");
        foreach (var s in info.Storages)
        {
            sb.AppendLine($"    <div class=\"row\"><span class=\"label\">{s.Name}</span><span class=\"value\">{s.Type} | {s.Capacity} | {s.Interface}</span></div>");
        }
        sb.AppendLine("  </div>");

        // Motherboard
        sb.AppendLine("  <div class=\"card\">");
        sb.AppendLine("    <h2>Материнская плата</h2>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Производитель</span><span class=\"value\">{info.Motherboard.Manufacturer}</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Модель</span><span class=\"value\">{info.Motherboard.Model}</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">Чипсет</span><span class=\"value\">{info.Motherboard.Chipset}</span></div>");
        sb.AppendLine($"    <div class=\"row\"><span class=\"label\">BIOS</span><span class=\"value\">{info.Motherboard.BiosVersion} ({info.Motherboard.BiosDate})</span></div>");
        sb.AppendLine("  </div>");

        sb.AppendLine("  <div class=\"footer\">Отчёт создан SpecMind - https://github.com/MiraWuse/SpecMind</div>");
        sb.AppendLine("</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
        return filePath;
    }
}