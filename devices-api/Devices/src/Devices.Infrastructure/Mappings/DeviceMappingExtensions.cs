using Devices.Application.DTOs;
using Devices.Infrastructure.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Devices.Infrastructure.Mappings;
public static class DeviceMappingExtensions
{
    public static Device ToEntity(this CreateDeviceDto dto)
    {
        return new Device
        {
            Name=dto.Name,
            Brand=dto.Brand,
            State=dto.State
        };
    }

    public static DeviceDto ToDto(this Device entity)
    {
        return new DeviceDto
        {
            Id = entity.Id,
            State = entity.State,
            Brand = entity.Brand,
            Name = entity.Name, 
            CreationTime = entity.CreationTime,
            LastUpdatedTime = entity.LastUpdateTime
        };
    }

    public static IEnumerable<DeviceDto> ToDtos(this IEnumerable<Device> entities)
    {
        return entities.Select(e => e.ToDto());
    }
}
