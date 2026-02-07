using Devices.Domain.Enums;

namespace Devices.API.Requests;

public class PatchDeviceRequest
{
    public string? Name { get; set; }
    public string? Brand { get; set; }
    public DeviceState? State { get; set; }
}
