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
/// Test suite for the DeleteAsync method of DeviceService.
/// Tests cover device not found, device in use, and successful deletion scenarios.
/// </summary>
public class DeviceServiceDeleteTests
{
    private readonly Mock<IDevicesDbContext> _mockContext;
    private readonly Mock<ILogger<DeviceService>> _mockLogger;
    private readonly Mock<IValidator<CreateDeviceDto>> _mockCreateValidator;
    private readonly Mock<IValidator<UpdateDeviceDto>> _mockUpdateValidator;
    private readonly Mock<IValidator<PatchDeviceDto>> _mockPatchValidator;
    private readonly DeviceService _service;

    public DeviceServiceDeleteTests()
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
    /// Tests that a DeviceNotFoundException is thrown when attempting to delete a non-existent device.
    /// Verifies that no database operations are performed when the device is not found.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_DeviceNotFound_ThrowsDeviceNotFoundException()
    {
        // Arrange
        var deviceId = Guid.NewGuid();

        var mockDbSet = new Mock<DbSet<Device>>();
        _mockContext.Setup(c => c.Devices).Returns(mockDbSet.Object);
        mockDbSet.Setup(d => d.FindAsync(new object[] { deviceId }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(( Device? ) null);

        // Act
        var act = async () => await _service.DeleteAsync(deviceId);

        // Assert
        await act.Should().ThrowAsync<DeviceNotFoundException>()
            .WithMessage($"*{deviceId}*");

        // Verify that no removal or save operations were performed
        _mockContext.Verify(c => c.Devices.Remove(It.IsAny<Device>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that a DeviceInUseException is thrown when attempting to delete a device that is currently in use.
    /// Verifies that the device is not removed and no database operations are performed.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_DeviceInUse_ThrowsDeviceInUseException()
    {
        // Arrange
        var device = new Device
        {
            Name = "iPhone 15 Pro",
            Brand = "Apple",
            State = DeviceState.InUse
        };

        var mockDbSet = new Mock<DbSet<Device>>();
        _mockContext.Setup(c => c.Devices).Returns(mockDbSet.Object);
        mockDbSet.Setup(d => d.FindAsync(new object[] { device.Id }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        // Act
        var act = async () => await _service.DeleteAsync(device.Id);

        // Assert
        await act.Should().ThrowAsync<DeviceInUseException>()
            .WithMessage("*delete*");

        // Verify that the device was not removed and save was not called
        _mockContext.Verify(c => c.Devices.Remove(It.IsAny<Device>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that a device with Available state is successfully deleted.
    /// Verifies that the device is removed from the database and changes are saved.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_DeviceAvailable_DeletesSuccessfully()
    {
        // Arrange
        var device = new Device
        {
            Name = "Samsung Galaxy S24",
            Brand = "Samsung",
            State = DeviceState.Available
        };

        var mockDbSet = new Mock<DbSet<Device>>();
        _mockContext.Setup(c => c.Devices).Returns(mockDbSet.Object);
        mockDbSet.Setup(d => d.FindAsync(new object[] { device.Id }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _service.DeleteAsync(device.Id);

        // Assert
        // Verify that the device was removed
        _mockContext.Verify(c => c.Devices.Remove(device), Times.Once);
        // Verify that changes were saved to the database
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that a device with Inactive state is successfully deleted.
    /// Verifies that the device is removed from the database and changes are saved.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_DeviceInactive_DeletesSuccessfully()
    {
        // Arrange
        var device = new Device
        {
            Name = "Google Pixel 9",
            Brand = "Google",
            State = DeviceState.Inactive
        };

        var mockDbSet = new Mock<DbSet<Device>>();
        _mockContext.Setup(c => c.Devices).Returns(mockDbSet.Object);
        mockDbSet.Setup(d => d.FindAsync(new object[] { device.Id }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(device);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _service.DeleteAsync(device.Id);

        // Assert
        // Verify that the device was removed
        _mockContext.Verify(c => c.Devices.Remove(device), Times.Once);
        // Verify that changes were saved to the database
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}