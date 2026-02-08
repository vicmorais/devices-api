using Devices.Application.Behaviours;
using Devices.Application.DTOs;
using Devices.Application.Exceptions;
using Devices.Application.Interfaces;
using Devices.Application.Mappings;
using Devices.Domain.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Devices.Application.Services;

public class DeviceService: IDeviceService
{
    private readonly IDevicesDbContext _context;

    private readonly ILogger<DeviceService> _logger;

    private readonly IValidator<CreateDeviceDto> _createValidator;
    private readonly IValidator<UpdateDeviceDto> _updateValidator;
    private readonly IValidator<PatchDeviceDto> _patchValidator;

    public DeviceService(IDevicesDbContext devicesDbContext,
        ILogger<DeviceService> logger,
        IValidator<CreateDeviceDto> createValidator,
        IValidator<UpdateDeviceDto> updateValidator,
        IValidator<PatchDeviceDto> patchValidator)
    {
        _context = devicesDbContext;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _patchValidator = patchValidator;
    }


    public async Task<DeviceDto> CreateAsync(CreateDeviceDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Creating new device with Name: {Name}, Brand: {Brand}",
            dto.Name, dto.Brand);

        await ValidationBehaviour.ValidateAndThrowAsync(_createValidator, dto, cancellationToken);

        var device = dto.ToEntity();

        _context.Devices.Add(device);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Device created successfully with Id: {DeviceId}", device.Id);

        return device.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Deleting device {DeviceId}", id);

        var device = await _context.Devices.FindAsync([id], cancellationToken)
            ?? throw new DeviceNotFoundException(id);

        if(device.State == DeviceState.InUse)
        {
            _logger.LogWarning("Attempted to delete in-use device {DeviceId}", id);
            throw new DeviceInUseException("delete");
        }

        _context.Devices.Remove(device);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Device {DeviceId} deleted successfully", id);
    }

    public async Task<PagedResult<DeviceDto>> GetAllAsync(string? brand, string? state, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Fetching devices - Brand: {Brand}, State: {State}, Page: {Page}, PageSize: {PageSize}",
            brand ?? "all", state ?? "all", page, pageSize);

        var query = _context.Devices.AsNoTracking();

        if(!string.IsNullOrWhiteSpace(brand))
        {
            query = query.Where(d => d.Brand.ToLower() == brand.ToLower());
        }

        if(!string.IsNullOrWhiteSpace(state))
        {
            if(!Enum.TryParse<DeviceState>(state, ignoreCase: true, out var deviceState))
            {
                _logger.LogWarning("Invalid device state filter: {State}", state);
                throw new InvalidDeviceStateException(state);
            }

            query = query.Where(d => d.State == deviceState);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(d => d.CreationTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {TotalCount} devices, returning page {Page}",
            totalCount, page);

        return new PagedResult<DeviceDto>
        {
            Items = items.ToDtos(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<DeviceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Fetching device {DeviceId}", id);

        var device = await _context.Devices.FindAsync([id], cancellationToken);

        if(device is null)
        {
            _logger.LogWarning("Device {DeviceId} not found", id);
        }

        return device?.ToDto();
    }

    public async Task<DeviceDto> PatchAsync(Guid id, PatchDeviceDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Patching device {DeviceId}", id);

        await ValidationBehaviour.ValidateAndThrowAsync(_patchValidator, dto, cancellationToken);

        var device = await _context.Devices.FindAsync([id], cancellationToken)
            ?? throw new DeviceNotFoundException(id);

        if(device.State == DeviceState.InUse)
        {
            if(dto.Name is not null && dto.Name != device.Name)
            {
                _logger.LogWarning(
                    "Attempted to patch Name of in-use device {DeviceId}", id);
                throw new DeviceInUseException("update Name of");
            }

            if(dto.Brand is not null && dto.Brand != device.Brand)
            {
                _logger.LogWarning(
                    "Attempted to patch Brand of in-use device {DeviceId}", id);
                throw new DeviceInUseException("update Brand of");
            }
        }

        if(dto.Name is not null)
            device.Name = dto.Name;
        if(dto.Brand is not null)
            device.Brand = dto.Brand;
        if(dto.State.HasValue)
            device.State = dto.State.Value;

        device.LastUpdateTime = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Device {DeviceId} patched successfully", id);

        return device.ToDto();
    }

    public async Task<DeviceDto> UpdateAsync(Guid id, UpdateDeviceDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Updating device {DeviceId}", id);

        await ValidationBehaviour.ValidateAndThrowAsync(_updateValidator, dto, cancellationToken);

        var device = await _context.Devices.FindAsync([id], cancellationToken)
            ?? throw new DeviceNotFoundException(id);

        if(device.State == DeviceState.InUse)
        {
            if(device.Name != dto.Name || device.Brand != dto.Brand)
            {
                _logger.LogWarning(
                    "Attempted to update Name or Brand of in-use device {DeviceId}", id);
                throw new DeviceInUseException("update Name or Brand of");
            }
        }

        device.Name = dto.Name;
        device.Brand = dto.Brand;
        device.State = dto.State;
        device.LastUpdateTime = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Device {DeviceId} updated successfully", id);

        return device.ToDto();
    }
}