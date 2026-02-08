using Asp.Versioning;
using Devices.API.Mappings;
using Devices.API.Requests;
using Devices.API.Responses;
using Devices.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Devices.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class DevicesController: ControllerBase
{
    private readonly IDeviceService _deviceService;

    public DevicesController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    /// <summary>
    /// Creates a new device.
    /// </summary>
    /// <param name="request">The device creation payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The newly created device.</returns>
    /// <response code="201">Device created successfully.</response>
    /// <response code="422">Validation failed.</response>
    [HttpPost]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(
        [FromBody] CreateDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _deviceService.CreateAsync(request.ToDto(), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result.ToResponse());
    }

    /// <summary>
    /// Retrieves a device by its unique identifier.
    /// </summary>
    /// <param name="id">The device unique identifier (GUID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The requested device.</returns>
    /// <response code="200">Device found.</response>
    /// <response code="404">Device not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _deviceService.GetByIdAsync(id, cancellationToken);

        if(result is null)
            return NotFound();

        return Ok(result.ToResponse());
    }

    /// <summary>
    /// Retrieves a paginated list of devices with optional filters.
    /// </summary>
    /// <param name="brand">Optional filter by brand name (case-insensitive).</param>
    /// <param name="state">Optional filter by device state (Available, InUse, Inactive).</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Number of items per page (default: 10).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of devices.</returns>
    /// <response code="200">Devices retrieved successfully.</response>
    /// <response code="400">Invalid state filter value.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<DeviceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? brand,
        [FromQuery] string? state,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _deviceService.GetAllAsync(brand, state, page, pageSize, cancellationToken);
        return Ok(result.ToResponse());
    }

    /// <summary>
    /// Fully updates an existing device. All fields are required.
    /// Name and Brand cannot be changed if the device is currently in use.
    /// </summary>
    /// <param name="id">The device unique identifier (GUID).</param>
    /// <param name="request">The full device update payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated device.</returns>
    /// <response code="200">Device updated successfully.</response>
    /// <response code="404">Device not found.</response>
    /// <response code="409">Cannot update Name/Brand of an in-use device.</response>
    /// <response code="422">Validation failed.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _deviceService.UpdateAsync(id, request.ToDto(), cancellationToken);
        return Ok(result.ToResponse());
    }

    /// <summary>
    /// Partially updates an existing device. Only provided (non-null) fields are updated.
    /// Name and Brand cannot be changed if the device is currently in use.
    /// </summary>
    /// <param name="id">The device unique identifier (GUID).</param>
    /// <param name="request">The partial device update payload. Null fields are ignored.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The patched device.</returns>
    /// <response code="200">Device patched successfully.</response>
    /// <response code="404">Device not found.</response>
    /// <response code="409">Cannot update Name/Brand of an in-use device.</response>
    /// <response code="422">Validation failed.</response>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(DeviceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Patch(
        Guid id,
        [FromBody] PatchDeviceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _deviceService.PatchAsync(id, request.ToDto(), cancellationToken);
        return Ok(result.ToResponse());
    }

    /// <summary>
    /// Deletes a device. Devices that are currently in use cannot be deleted.
    /// </summary>
    /// <param name="id">The device unique identifier (GUID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Device deleted successfully.</response>
    /// <response code="404">Device not found.</response>
    /// <response code="409">Cannot delete an in-use device.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _deviceService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}