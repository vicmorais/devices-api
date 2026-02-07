using System;
using System.Collections.Generic;
using System.Text;

namespace Devices.Infrastructure.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public DateTime CreationTime { get; init; } = DateTime.UtcNow;

    public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;
}
