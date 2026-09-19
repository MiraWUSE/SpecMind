using SpecMind.Models;
using SpecMind.Modules.AI.Models;
using System.Text.Json;

namespace SpecMind.Modules.AI.Services;

public class SnapshotBuilder
{
    public HardwareSnapshot Build(HardwareInfo hardwareInfo)
    {
        ArgumentNullException.ThrowIfNull(hardwareInfo);
        // Freeze nested fields before background inference; never fabricate hardware.
        var copy = JsonSerializer.Deserialize<HardwareInfo>(JsonSerializer.Serialize(hardwareInfo));
        return new HardwareSnapshot { Hardware = copy, CreatedAt = DateTime.Now };
    }
}
