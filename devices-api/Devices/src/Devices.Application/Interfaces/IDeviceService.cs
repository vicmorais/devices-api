using Devices.Application.DTOs;

namespace Devices.Application.Interfaces;

public interface IDeviceService
{
    Task<DeviceDto> CreateAsync(CreateDeviceDto dto);
    Task<DeviceDto> UpdateAsync(Guid id,UpdateDeviceDto dto);
    Task<DeviceDto> PatchAsync(Guid id,PatchDeviceDto dto);
    Task<DeviceDto?> GetByIdAsync(Guid id);
    Task<PagedResult<DeviceDto>> GetAllAsync(string? brand,string? state,int page,int pageSize);
    Task DeleteAsync(Guid id);
}
