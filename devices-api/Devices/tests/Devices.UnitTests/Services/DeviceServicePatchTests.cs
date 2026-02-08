using Devices.Application.DTOs;
using Devices.Application.Exceptions;
using Devices.Application.Interfaces;
using Devices.Application.Services;
using Devices.Domain.Entities;
using Devices.Domain.Enums;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Devices.UnitTests.Services;

/// <summary>
/// Test suite for the PatchAsync method of DeviceService.
/// Tests cover device not found, validation, device in use restrictions, partial updates, and timestamp handling.
/// </summary>
public class DeviceServicePatchTests
{
    private readonly Mock<IDevicesDbContext> _mockContext;
    private readonly Mock<ILogger<DeviceService>> _mockLogger;
    private readonly Mock<IValidator<CreateDeviceDto>> _mockCreateValidator;
    private readonly Mock<IValidator<UpdateDeviceDto>> _mockUpdateValidator;
    private readonly Mock<IValidator<PatchDeviceDto>> _mockPatchValidator;
    private readonly DeviceService _service;

    public DeviceServicePatchTests()
    {
        _mockContext = new Mock<IDevicesDbContext>();
        _mockLogger = new Mock<ILogger<DeviceService>>();
        _mockCreateValidator = new Mock<IValidator<CreateDeviceDto>>();
        _mockUpdateValidator = new Mock<IValidator<UpdateDeviceDto>>();
        _mockPatchValidator = new Mock<IValidator<PatchDeviceDto>>();

        // By default, validators pass
        _mockCreateValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateDeviceDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _mockUpdateValidator.Setup(v => v.ValidateAsync(It.IsAny<UpdateDeviceDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        _mockPatchValidator.Setup(v => v.ValidateAsync(It.IsAny<PatchDeviceDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _service = new DeviceService(
            _mockContext.Object,
            _mockLogger.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object,
            _mockPatchValidator.Object);
    }

    /// <summary>
    /// Tests that a DeviceNotFoundException is thrown when attempting to patch a non-existent device.
    /// Verifies that no database operations are performed when the device is not found.
    /// </summary>
    [Fact]
    public async Task PatchAsync_DeviceNotFound_ThrowsDeviceNotFoundException()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var dto = new PatchDeviceDto
        {
            Name = "Updated Name"
        };

        var mockDbSet = new Mock<DbSet<Device>>();
        _mockContext.Setup(c => c.Devices).Returns(mockDbSet.Object);
        mockDbSet.Setup(d => d.FindAsync(new object[] { deviceId }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(( Device? ) null);

        // Act
        var act = async () => await _service.PatchAsync(deviceId, dto);

        // Assert
        await act.Should().ThrowAsync<DeviceNotFoundException>()
            .WithMessage($"*{deviceId}*");

        // Verify that no save operation was performed
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that a ValidationException is thrown when the patch DTO is invalid.
    /// Verifies that no database operations are performed when validation fails.
    /// </summary>
    [Fact]
    public async Task PatchAsync_InvalidDto_ThrowsValidationException()
    {
        // Arrange
        var device = new Device
        {
            Name = "Original Name",
            Brand = "Original Brand",
            State = DeviceState.Available
        };

        var dto = new PatchDeviceDto
        {
            Name = string.Empty
        };

        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name cannot be empty.") { PropertyName = "Name" }
        };

        _mockPatchValidator
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        var mockDbSet = new Mock<DbSet<Device>>();
        _mockContext.Setup(c => c.Devices).Returns(mockDbSet.Object);

        // Act
        var act = async () => await _service.PatchAsync(device.Id, dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();

        // Verify that no save operation was performed
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that a DeviceInUseException is thrown when attempting to change the Name of an in-use device.
    /// Verifies that the device is not updated when Name changes on an in-use device.
    /// </summary>
    [Fact]
    public async Task PatchAsync_InUseDevice_ChangeName_ThrowsDeviceInUseException()
    {
        // Arrange
        var device = new Device
        {
            Name = "Original Name",
            Brand = "Original Brand",
            State = DeviceState.InUse
        };

        var dto = new PatchDeviceDto
        {
            Name = "Updated Name"
        };

        var mockDbSet = new Mock<DbSet<Device>>();
        _mockContext.Setup(c => c.Devices).Returns(mockDbSet.Object);
        mockDbSet.Setup(d => d.FindAsync(new object[] { device.Id }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        // Act
        var act = async () => await _service.PatchAsync(device.Id, dto);

        // Assert
        await act.Should().ThrowAsync<DeviceInUseException>()
            .WithMessage("*update Name of*");

        // Verify that no save operation was performed
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that a DeviceInUseException is thrown when attempting to change the Brand of an in-use device.
    /// Verifies that the device is not updated when Brand changes on an in-use device.
    /// </summary>
    [Fact]
    public async Task PatchAsync_InUseDevice_ChangeBrand_ThrowsDeviceInUseException()
    {
        // Arrange
        var device = new Device
        {
            Name = "Original Name",
            Brand = "Original Brand",
            State = DeviceState.InUse
        };

        var dto = new PatchDeviceDto
        {
            Brand = "Updated Brand"
        };

        var mockDbSet = new Mock<DbSet<Device>>();
        _mockContext.Setup(c => c.Devices).Returns(mockDbSet.Object);
        mockDbSet.Setup(d => d.FindAsync(new object[] { device.Id }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        // Act
        var act = async () => await _service.PatchAsync(device.Id, dto);

        // Assert
        await act.Should().ThrowAsync<DeviceInUseException>()
            .WithMessage("*update Brand of*");

        // Verify that no save operation was performed
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that an in-use device can be patched when only the State changes and Name/Brand remain null.
    /// Verifies that the device is successfully updated and persisted to the database.
    /// </summary>
    [Fact]
    public async Task PatchAsync_InUseDevice_ChangeState_UpdatesSuccessfully()
    {
        // Arrange
        var device = new Device
        {
            Name = "Original Name",
            Brand = "Original Brand",
            State = DeviceState.InUse
        };

        var dto = new PatchDeviceDto
        {
            State = DeviceState.Available
        };

        var mockDbSet = new Mock<DbSet<Device>>();
        _mockContext.Setup(c => c.Devices).Returns(mockDbSet.Object);
        mockDbSet.Setup(d => d.FindAsync(new object[] { device.Id }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.PatchAsync(device.Id, dto);

        // Assert
        result.Should().NotBeNull();
        result.State.Should().Be(DeviceState.Available);
        result.Name.Should().Be("Original Name");
        result.Brand.Should().Be("Original Brand");

        // Verify that save was called
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that null fields in the patch DTO do not change the existing device values.
    /// Verifies that a patch with all null fields leaves the device unchanged.
    /// </summary>
    [Fact]
    public async Task PatchAsync_NullFields_DoesNotChangeExistingValues()
    {
        // Arrange
        var device = new Device
        {
            Name = "Original Name",
            Brand = "Original Brand",
            State = DeviceState.Available
        };

        var dto = new PatchDeviceDto
        {
            Name = null,
            Brand = null,
            State = null
        };

        var mockDbSet = new Mock<DbSet<Device>>();
        _mockContext.Setup(c => c.Devices).Returns(mockDbSet.Object);
        mockDbSet.Setup(d => d.FindAsync(new object[] { device.Id }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.PatchAsync(device.Id, dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Original Name");
        result.Brand.Should().Be("Original Brand");
        result.State.Should().Be(DeviceState.Available);

        // Verify that save was called (even though nothing changed)
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that only the Name field is updated when patching with only Name provided.
    /// Verifies that Brand and State remain unchanged.
    /// </summary>
    [Fact]
    public async Task PatchAsync_OnlyName_UpdatesOnlyName()
    {
        // Arrange
        var device = new Device
        {
            Name = "Original Name",
            Brand = "Original Brand",
            State = DeviceState.Available
        };

        var dto = new PatchDeviceDto
        {
            Name = "Updated Name",
            Brand = null,
            State = null
        };

        var mockDbSet = new Mock<DbSet<Device>>();
        _mockContext.Setup(c => c.Devices).Returns(mockDbSet.Object);
        mockDbSet.Setup(d => d.FindAsync(new object[] { device.Id }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.PatchAsync(device.Id, dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Name");
        result.Brand.Should().Be("Original Brand");
        result.State.Should().Be(DeviceState.Available);

        // Verify that save was called
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that only the Brand field is updated when patching with only Brand provided.
    /// Verifies that Name and State remain unchanged.
    /// </summary>
    [Fact]
    public async Task PatchAsync_OnlyBrand_UpdatesOnlyBrand()
    {
        // Arrange
        var device = new Device
        {
            Name = "Original Name",
            Brand = "Original Brand",
            State = DeviceState.Available
        };

        var dto = new PatchDeviceDto
        {
            Name = null,
            Brand = "Updated Brand",
            State = null
        };

        var mockDbSet = new Mock<DbSet<Device>>();
        _mockContext.Setup(c => c.Devices).Returns(mockDbSet.Object);
        mockDbSet.Setup(d => d.FindAsync(new object[] { device.Id }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.PatchAsync(device.Id, dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Original Name");
        result.Brand.Should().Be("Updated Brand");
        result.State.Should().Be(DeviceState.Available);

        // Verify that save was called
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that only the State field is updated when patching with only State provided.
    /// Verifies that Name and Brand remain unchanged.
    /// </summary>
    [Fact]
    public async Task PatchAsync_OnlyState_UpdatesOnlyState()
    {
        // Arrange
        var device = new Device
        {
            Name = "Original Name",
            Brand = "Original Brand",
            State = DeviceState.Available
        };

        var dto = new PatchDeviceDto
        {
            Name = null,
            Brand = null,
            State = DeviceState.Inactive
        };

        var mockDbSet = new Mock<DbSet<Device>>();
        _mockContext.Setup(c => c.Devices).Returns(mockDbSet.Object);
        mockDbSet.Setup(d => d.FindAsync(new object[] { device.Id }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.PatchAsync(device.Id, dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Original Name");
        result.Brand.Should().Be("Original Brand");
        result.State.Should().Be(DeviceState.Inactive);

        // Verify that save was called
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that the LastUpdateTime is automatically set to the current UTC time when a device is patched.
    /// Verifies that the timestamp is within an acceptable time window and in UTC format.
    /// </summary>
    [Fact]
    public async Task PatchAsync_SetsLastUpdatedTime()
    {
        // Arrange
        var device = new Device
        {
            Name = "Original Name",
            Brand = "Original Brand",
            State = DeviceState.Available
        };

        var dto = new PatchDeviceDto
        {
            Name = "Updated Name"
        };

        var mockDbSet = new Mock<DbSet<Device>>();
        _mockContext.Setup(c => c.Devices).Returns(mockDbSet.Object);
        mockDbSet.Setup(d => d.FindAsync(new object[] { device.Id }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var beforeCall = DateTime.UtcNow;
        var result = await _service.PatchAsync(device.Id, dto);
        var afterCall = DateTime.UtcNow;

        // Assert
        result.LastUpdatedTime.Should().BeOnOrAfter(beforeCall);
        result.LastUpdatedTime.Should().BeOnOrBefore(afterCall);
        result.LastUpdatedTime.Kind.Should().Be(DateTimeKind.Utc);

        // Verify that save was called
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}