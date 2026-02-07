using Devices.Domain.Enums;

namespace Devices.Application.DTOs;

public class UpdateDeviceDto
{
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public DeviceState State { get; set; }
}
