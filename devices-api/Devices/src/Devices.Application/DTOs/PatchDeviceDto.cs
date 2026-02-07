using Devices.Domain.Enums;

namespace Devices.Application.DTOs;

public class PatchDeviceDto
{
    public string? Name { get; set; }
    public string? Brand { get; set; }
    public DeviceState? State { get; set; }
}
