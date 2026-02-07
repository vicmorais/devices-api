using Devices.Application.DTOs;

namespace Devices.Application.Interfaces;

public interface IDeviceService
{
    Task<DeviceDto> CreateAsync(CreateDeviceDto dto,CancellationToken cancellationToken = default);
    Task<DeviceDto> UpdateAsync(Guid id,UpdateDeviceDto dto,CancellationToken cancellationToken = default);
    Task<DeviceDto> PatchAsync(Guid id,PatchDeviceDto dto,CancellationToken cancellationToken = default);
    Task<DeviceDto?> GetByIdAsync(Guid id,CancellationToken cancellationToken = default);
    Task<PagedResult<DeviceDto>> GetAllAsync(string? brand,string? state,int page,int pageSize,CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id,CancellationToken cancellationToken = default);
}
