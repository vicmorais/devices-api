using Devices.Domain.Enums;

namespace Devices.Domain.Entities;

public class Device : BaseEntity
{
    public string Name { get; set; } = String.Empty;

    public string Brand { get; set; } = String.Empty;

    public DeviceState State { get; set; } = DeviceState.Available;
}
