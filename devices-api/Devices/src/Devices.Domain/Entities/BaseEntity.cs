namespace Devices.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; init; } = Guid.CreateVersion7();

    public DateTime CreationTime { get; init; } = DateTime.UtcNow;

    public DateTime LastUpdateTime { get; set; } = DateTime.UtcNow;
}
