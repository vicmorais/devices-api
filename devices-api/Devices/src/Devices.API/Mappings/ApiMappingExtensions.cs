using Devices.API.Requests;
using Devices.API.Responses;
using Devices.Application.DTOs;

namespace Devices.API.Mappings;

public static class ApiMappingExtensions
{
    public static CreateDeviceDto ToDto(this CreateDeviceRequest request)
    {
        return new CreateDeviceDto
        {
            Name = request.Name,
            Brand = request.Brand,
            State = request.State
        };
    }

    public static UpdateDeviceDto ToDto(this UpdateDeviceRequest request)
    {
        return new UpdateDeviceDto
        {
            Name = request.Name,
            Brand = request.Brand,
            State = request.State
        };
    }

    public static PatchDeviceDto ToDto(this PatchDeviceRequest request)
    {
        return new PatchDeviceDto
        {
            Name = request.Name,
            Brand = request.Brand,
            State = request.State
        };
    }

    public static DeviceResponse ToResponse(this DeviceDto dto)
    {
        return new DeviceResponse
        {
            Id = dto.Id,
            Name = dto.Name,
            Brand = dto.Brand,
            State = dto.State,
            CreationTime = dto.CreationTime,
            LastUpdatedTime = dto.LastUpdatedTime
        };
    }

    public static PagedResponse<DeviceResponse> ToResponse(this PagedResult<DeviceDto> pagedResult)
    {
        return new PagedResponse<DeviceResponse>
        {
            Items = pagedResult.Items.Select(d => d.ToResponse()),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize,
            TotalPages = pagedResult.TotalPages,
            HasNextPage = pagedResult.HasNextPage,
            HasPreviousPage = pagedResult.HasPreviousPage
        };
    }
}
