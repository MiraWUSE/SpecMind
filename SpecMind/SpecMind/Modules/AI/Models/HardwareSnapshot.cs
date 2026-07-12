using SpecMind.Models;
using System;

namespace SpecMind.Modules.AI.Models;

public class HardwareSnapshot
{
    public HardwareInfo Hardware { get; init; }

    public DateTime CreatedAt { get; init; } = DateTime.Now;
}