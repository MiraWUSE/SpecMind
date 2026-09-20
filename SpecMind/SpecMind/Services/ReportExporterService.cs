using SpecMind.Models;
using System;
using System.IO;
using System.Net;
using System.Globalization;
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
        sb.AppendLine($"  Температура:     {TemperatureReading.Format(info.Sensors.CpuTemperature)}");
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
        sb.AppendLine($"  Температура:     {TemperatureReading.Format(info.Sensors.GpuTemperature)}");
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
            Monitors = info.Monitors,
            Motherboard = info.Motherboard,
            Sensors = new
            {
                CpuTemperature = info.Sensors.CpuTemperature,
                GpuTemperature = info.Sensors.GpuTemperature,
                CpuUsage = info.Sensors.CpuUsage,
                RamUsage = info.Sensors.RamUsage,
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
        sb.AppendLine($"Общие;Дата отчёта;{Csv($"{DateTime.Now:dd.MM.yyyy HH:mm:ss}")}");
        sb.AppendLine($"Общие;Тип устройства;{Csv($"{info.DeviceType}")}");

        // CPU
        sb.AppendLine($"CPU;Название;{Csv($"{info.Cpu.Name}")}");
        sb.AppendLine($"CPU;Производитель;{Csv($"{info.Cpu.Manufacturer}")}");
        sb.AppendLine($"CPU;Ядер;{Csv($"{info.Cpu.Cores}")}");
        sb.AppendLine($"CPU;Потоков;{Csv($"{info.Cpu.Threads}")}");
        sb.AppendLine($"CPU;Базовая частота;{Csv($"{info.Cpu.BaseClock}")}");
        sb.AppendLine($"CPU;Макс. частота;{Csv($"{info.Cpu.MaxClock}")}");
        sb.AppendLine($"CPU;Архитектура;{Csv($"{info.Cpu.Architecture}")}");
        sb.AppendLine($"CPU;Сокет;{Csv($"{info.Cpu.Socket}")}");
        sb.AppendLine($"CPU;Кэш L1;{Csv($"{info.Cpu.CacheL1}")}");
        sb.AppendLine($"CPU;Кэш L2;{Csv($"{info.Cpu.CacheL2}")}");
        sb.AppendLine($"CPU;Кэш L3;{Csv($"{info.Cpu.CacheL3}")}");
        sb.AppendLine($"CPU;Температура;{Csv($"{TemperatureReading.Format(info.Sensors.CpuTemperature)}")}");
        sb.AppendLine($"CPU;Загрузка;{Csv($"{info.Sensors.CpuUsage:F1}%")}");

        // GPU
        sb.AppendLine($"GPU;Название;{Csv($"{info.Gpu.Name}")}");
        sb.AppendLine($"GPU;Производитель;{Csv($"{info.Gpu.Manufacturer}")}");
        sb.AppendLine($"GPU;VRAM;{Csv($"{info.Gpu.Vram}")}");
        sb.AppendLine($"GPU;CUDA Cores;{Csv($"{info.Gpu.CudaCores}")}");
        sb.AppendLine($"GPU;TDP;{Csv($"{info.Gpu.Tdp}")}");
        sb.AppendLine($"GPU;PCIe версия;{Csv($"{info.Gpu.PcieVersion}")}");
        sb.AppendLine($"GPU;Device ID;{Csv($"{info.Gpu.DeviceId}")}");
        sb.AppendLine($"GPU;Драйвер;{Csv($"{info.Gpu.DriverVersion}")}");
        sb.AppendLine($"GPU;Дата драйвера;{Csv($"{info.Gpu.DriverDate}")}");
        sb.AppendLine($"GPU;Температура;{Csv($"{TemperatureReading.Format(info.Sensors.GpuTemperature)}")}");
        sb.AppendLine($"GPU;Загрузка;{Csv($"{info.Sensors.GpuUsage:F1}%")}");
        sb.AppendLine($"GPU;Вентилятор;{Csv($"{info.Sensors.GpuFanSpeed} RPM")}");

        // RAM
        sb.AppendLine($"RAM;Название;{Csv($"{info.Ram.ModuleName}")}");
        sb.AppendLine($"RAM;Объём;{Csv($"{info.Ram.TotalCapacity}")}");
        sb.AppendLine($"RAM;Тип;{Csv($"{info.Ram.Type}")}");
        sb.AppendLine($"RAM;Частота;{Csv($"{info.Ram.Speed}")}");
        sb.AppendLine($"RAM;Form Factor;{Csv($"{info.Ram.FormFactor}")}");
        sb.AppendLine($"RAM;Производитель;{Csv($"{info.Ram.Manufacturer}")}");
        sb.AppendLine($"RAM;Part Number;{Csv($"{info.Ram.PartNumber}")}");
        sb.AppendLine($"RAM;Слоты;{Csv($"{info.Ram.SlotsUsed} / {info.Ram.TotalSlots}")}");

        // Storage
        for (int i = 0; i < info.Storages.Count; i++)
        {
            var s = info.Storages[i];
            sb.AppendLine($"Storage_{i + 1};Название;{Csv($"{s.Name}")}");
            sb.AppendLine($"Storage_{i + 1};Тип;{Csv($"{s.Type}")}");
            sb.AppendLine($"Storage_{i + 1};Объём;{Csv($"{s.Capacity}")}");
            sb.AppendLine($"Storage_{i + 1};Интерфейс;{Csv($"{s.Interface}")}");
            sb.AppendLine($"Storage_{i + 1};S/N;{Csv($"{s.SerialNumber}")}");
            sb.AppendLine($"Storage_{i + 1};Статус;{Csv($"{s.HealthStatus}")}");
        }

        // Motherboard
        sb.AppendLine($"Motherboard;Производитель;{Csv($"{info.Motherboard.Manufacturer}")}");
        sb.AppendLine($"Motherboard;Модель;{Csv($"{info.Motherboard.Model}")}");
        sb.AppendLine($"Motherboard;Чипсет;{Csv($"{info.Motherboard.Chipset}")}");
        sb.AppendLine($"Motherboard;BIOS Version;{Csv($"{info.Motherboard.BiosVersion}")}");
        sb.AppendLine($"Motherboard;BIOS Date;{Csv($"{info.Motherboard.BiosDate}")}");
        sb.AppendLine($"Motherboard;Serial Number;{Csv($"{info.Motherboard.SerialNumber}")}");

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
        AppendHtmlLine(sb, $"  <h1>SpecMind System Report</h1>");
        AppendHtmlLine(sb, $"  <p><strong>Дата:</strong> {DateTime.Now:dd.MM.yyyy HH:mm:ss} | <strong>Тип:</strong> {info.DeviceType}</p>");

        // CPU
        sb.AppendLine("  <div class=\"card\">");
        sb.AppendLine("    <h2>Процессор</h2>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Название</span><span class=\"value\">{info.Cpu.Name}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Производитель</span><span class=\"value\">{info.Cpu.Manufacturer}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Ядер / Потоков</span><span class=\"value\">{info.Cpu.Cores} / {info.Cpu.Threads}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Частоты</span><span class=\"value\">{info.Cpu.BaseClock} / {info.Cpu.MaxClock}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Архитектура / Сокет</span><span class=\"value\">{info.Cpu.Architecture} / {info.Cpu.Socket}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Кэш L1/L2/L3</span><span class=\"value\">{info.Cpu.CacheL1} / {info.Cpu.CacheL2} / {info.Cpu.CacheL3}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Температура</span><span class=\"value\">{TemperatureReading.Format(info.Sensors.CpuTemperature)}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Загрузка</span><span class=\"value\">{info.Sensors.CpuUsage:F1}%</span></div>");
        sb.AppendLine("  </div>");

        // GPU
        sb.AppendLine("  <div class=\"card\">");
        sb.AppendLine("    <h2>Видеокарта</h2>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Название</span><span class=\"value\">{info.Gpu.Name}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Производитель</span><span class=\"value\">{info.Gpu.Manufacturer}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">VRAM</span><span class=\"value\">{info.Gpu.Vram}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">CUDA Cores</span><span class=\"value\">{info.Gpu.CudaCores}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">TDP</span><span class=\"value\">{info.Gpu.Tdp}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">PCIe версия</span><span class=\"value\">{info.Gpu.PcieVersion}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Драйвер</span><span class=\"value\">{info.Gpu.DriverVersion} ({info.Gpu.DriverDate})</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Температура</span><span class=\"value\">{TemperatureReading.Format(info.Sensors.GpuTemperature)}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Загрузка</span><span class=\"value\">{info.Sensors.GpuUsage:F1}%</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Вентилятор</span><span class=\"value\">{info.Sensors.GpuFanSpeed} RPM</span></div>");
        sb.AppendLine("  </div>");

        // RAM
        sb.AppendLine("  <div class=\"card\">");
        sb.AppendLine("    <h2>Оперативная память</h2>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Модуль</span><span class=\"value\">{info.Ram.ModuleName}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Объём</span><span class=\"value\">{info.Ram.TotalCapacity}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Тип / Частота</span><span class=\"value\">{info.Ram.Type} / {info.Ram.Speed}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Form Factor</span><span class=\"value\">{info.Ram.FormFactor}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Производитель</span><span class=\"value\">{info.Ram.Manufacturer}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Слоты</span><span class=\"value\">{info.Ram.SlotsUsed} / {info.Ram.TotalSlots}</span></div>");
        sb.AppendLine("  </div>");

        // Storage
        sb.AppendLine("  <div class=\"card\">");
        sb.AppendLine("    <h2>Накопители</h2>");
        foreach (var s in info.Storages)
        {
            AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">{s.Name}</span><span class=\"value\">{s.Type} | {s.Capacity} | {s.Interface}</span></div>");
        }
        sb.AppendLine("  </div>");

        // Motherboard
        sb.AppendLine("  <div class=\"card\">");
        sb.AppendLine("    <h2>Материнская плата</h2>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Производитель</span><span class=\"value\">{info.Motherboard.Manufacturer}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Модель</span><span class=\"value\">{info.Motherboard.Model}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">Чипсет</span><span class=\"value\">{info.Motherboard.Chipset}</span></div>");
        AppendHtmlLine(sb, $"    <div class=\"row\"><span class=\"label\">BIOS</span><span class=\"value\">{info.Motherboard.BiosVersion} ({info.Motherboard.BiosDate})</span></div>");
        sb.AppendLine("  </div>");

        sb.AppendLine("  <div class=\"footer\">Отчёт создан SpecMind - https://github.com/MiraWuse/SpecMind</div>");
        sb.AppendLine("</div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
        return filePath;
    }
    private static string Csv(string value)
        => value.IndexOfAny([';', '"', '\r', '\n']) >= 0
            ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;

    private static void AppendHtmlLine(StringBuilder builder, FormattableString line)
        => builder.AppendLine(line.ToString(HtmlFormatProvider.Instance));

    private sealed class HtmlFormatProvider : IFormatProvider, ICustomFormatter
    {
        public static readonly HtmlFormatProvider Instance = new();
        public object GetFormat(Type type) => type == typeof(ICustomFormatter) ? this : null;
        public string Format(string format, object value, IFormatProvider provider)
            => WebUtility.HtmlEncode(value is IFormattable formattable
                ? formattable.ToString(format, CultureInfo.CurrentCulture) : value?.ToString() ?? "");
    }
}