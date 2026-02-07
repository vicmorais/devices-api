using Devices.Application.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Devices.Application.DTOs;

public class DeviceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public DeviceState State { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime LastUpdatedTime { get; set; }
}
