using SpecMind.Models;
using SpecMind.Modules.AI.Models;
using System;
using System.Threading.Tasks;

namespace SpecMind.Modules.AI.Services;

public class SnapshotBuilder
{
    public HardwareSnapshot Build(HardwareInfo hardwareInfo)
    {
        return new HardwareSnapshot
        {
            Hardware = hardwareInfo,
            CreatedAt = DateTime.Now
        };
    }

    public Task<HardwareSnapshot> BuildAsync()
    {
        return Task.FromResult(new HardwareSnapshot
        {
            Hardware = new HardwareInfo(),
            CreatedAt = DateTime.Now
        });
    }
}