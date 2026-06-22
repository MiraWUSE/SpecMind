using SpecMind.Models;
using System.Threading.Tasks;

namespace SpecMind.Services;

public interface IHardwareScannerService
{
    Task<HardwareInfo> GetHardwareInfoAsync();
}