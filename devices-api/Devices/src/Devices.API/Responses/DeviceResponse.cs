using Devices.Domain.Enums;

namespace Devices.API.Responses;

public class DeviceResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public DeviceState State { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastUpdatedTime { get; set; }
}
