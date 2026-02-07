
using Devices.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Devices.Infrastructure.Configurations;

public class DeviceConfiguration:IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.Brand)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.State)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(d => d.CreationTime)
            .IsRequired();

        builder.Property(d => d.LastUpdateTime)
            .IsRequired();

    }
}
