using Devices.Application.Enums;

namespace Devices.Infrastructure.Entities;

public class Device : BaseEntity
{
    public string Name { get; set; } = String.Empty;

    public string Brand { get; set; } = String.Empty;

    public DeviceState State { get; set; } = DeviceState.Available;
}
