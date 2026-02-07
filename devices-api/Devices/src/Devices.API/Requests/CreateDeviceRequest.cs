using Devices.Domain.Enums;

namespace Devices.API.Requests;

public class CreateDeviceRequest
{
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public DeviceState State { get; set; } = DeviceState.Available;
}
