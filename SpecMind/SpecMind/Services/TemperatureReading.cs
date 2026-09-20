using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SpecMind.Services;

public static class TemperatureReading
{
    public static bool IsValid(double? value) => value is > 0 and < 150;

    public static string Format(double? value, string format = "F1", IFormatProvider provider = null)
        => IsValid(value) ? value.Value.ToString(format, provider ?? CultureInfo.CurrentCulture) + "°C" : "Нет данных";

    public static double? SelectCpu(IEnumerable<(string Name, double? Value)> readings)
    {
        // A distance-to-TjMax sensor is not an absolute temperature.
        var valid = readings.Where(r => IsValid(r.Value) &&
            !r.Name.Contains("distance", StringComparison.OrdinalIgnoreCase) &&
            !r.Name.Contains("tjmax", StringComparison.OrdinalIgnoreCase)).ToArray();
        foreach (var preferred in new[] { "package", "tctl", "tdie" })
        {
            var matches = valid.Where(r => r.Name.Contains(preferred, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (matches.Length > 0) return matches.Max(r => r.Value);
        }
        var cores = valid.Where(r => r.Name.Contains("core", StringComparison.OrdinalIgnoreCase)).ToArray();
        return cores.Length > 0 ? cores.Max(r => r.Value) : null;
    }
}
