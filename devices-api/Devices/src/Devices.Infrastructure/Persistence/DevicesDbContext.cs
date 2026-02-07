using Devices.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Devices.Infrastructure.Persistence;

public class DevicesDbContext : DbContext
{
    public DevicesDbContext(DbContextOptions<DevicesDbContext> options) : base(options) { }

    public DbSet<Device> Devices => Set<Device>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DevicesDbContext).Assembly);
    }
}
