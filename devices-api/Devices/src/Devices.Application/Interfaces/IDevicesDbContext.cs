using Devices.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Devices.Application.Interfaces;

public interface IDevicesDbContext
{
    DbSet<Device> Devices { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
