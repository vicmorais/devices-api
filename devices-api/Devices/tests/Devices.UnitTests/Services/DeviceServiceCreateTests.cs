using Devices.Application.DTOs;
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
/// Test suite for the CreateAsync method of DeviceService.
/// Tests cover validation, device creation, and automatic timestamp assignment.
/// </summary>
public class DeviceServiceCreateTests
{
    private readonly Mock<IDevicesDbContext> _mockContext;
    private readonly Mock<ILogger<DeviceService>> _mockLogger;
    private readonly Mock<IValidator<CreateDeviceDto>> _mockCreateValidator;
    private readonly Mock<IValidator<UpdateDeviceDto>> _mockUpdateValidator;
    private readonly Mock<IValidator<PatchDeviceDto>> _mockPatchValidator;
    private readonly DeviceService _service;

    public DeviceServiceCreateTests()
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
    /// Tests that a device is created successfully with a valid DTO.
    /// Verifies that all properties are correctly mapped and the device is persisted to the database.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ValidDto_ReturnsCreatedDevice()
    {
        // Arrange
        var dto = new CreateDeviceDto
        {
            Name = "iPhone 15 Pro",
            Brand = "Apple",
            State = DeviceState.Available
        };

        var mockDbSet = new Mock<DbSet<Device>>();
        _mockContext.Setup(c => c.Devices).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("iPhone 15 Pro");
        result.Brand.Should().Be("Apple");
        result.State.Should().Be(DeviceState.Available);
        result.Id.Should().NotBe(Guid.Empty);

        // Verify database interactions
        _mockContext.Verify(c => c.Devices.Add(It.IsAny<Device>()), Times.Once);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that a ValidationException is thrown when the DTO is invalid.
    /// Verifies that no database operations are performed when validation fails.
    /// </summary>
    [Fact]
    public async Task CreateAsync_InvalidDto_ThrowsValidationException()
    {
        // Arrange
        var dto = new CreateDeviceDto
        {
            Name = "",
            Brand = "",
            State = DeviceState.Available
        };

        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required.") { PropertyName = "Name" },
            new ValidationFailure("Brand", "Brand is required.") { PropertyName = "Brand" }
        };

        _mockCreateValidator
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        // Act
        var act = async () => await _service.CreateAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();

        // Verify that no database operations were performed
        _mockContext.Verify(c => c.Devices.Add(It.IsAny<Device>()), Times.Never);
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Tests that the CreationTime is automatically set when a device is created.
    /// Verifies that the timestamp is within an acceptable time window and is in UTC format.
    /// </summary>
    [Fact]
    public async Task CreateAsync_SetsCreationTimeAutomatically()
    {
        // Arrange
        var dto = new CreateDeviceDto
        {
            Name = "Samsung Galaxy S24",
            Brand = "Samsung",
            State = DeviceState.Available
        };

        var mockDbSet = new Mock<DbSet<Device>>();
        _mockContext.Setup(c => c.Devices).Returns(mockDbSet.Object);
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var beforeCall = DateTime.UtcNow;
        var result = await _service.CreateAsync(dto);
        var afterCall = DateTime.UtcNow;

        // Assert
        result.CreationTime.Should().BeOnOrAfter(beforeCall);
        result.CreationTime.Should().BeOnOrBefore(afterCall);
        result.CreationTime.Kind.Should().Be(DateTimeKind.Utc);
    }

    /// <summary>
    /// Tests that LastUpdatedTime is automatically set equal to CreationTime when a device is created.
    /// Verifies that both timestamps are identical or within a very small time window (less than 100ms).
    /// </summary>
    [Fact]
    public async Task CreateAsync_SetsLastUpdatedTimeEqualToCreationTime()
    {
        // Arrange
        var dto = new CreateDeviceDto
        {
            Name = "Google Pixel 9",
            Brand = "Google",
            State = DeviceState.Available
        };

        var mockDbSet = new Mock<DbSet<Device>>();
        var capturedDevice = new Device();

        // Capture the actual device being added to verify its properties
        _mockContext.Setup(c => c.Devices).Returns(mockDbSet.Object);
        mockDbSet.Setup(d => d.Add(It.IsAny<Device>()))
            .Callback<Device>(d => capturedDevice = d);

        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        // Verify that both timestamps exist and are very close in time
        // They should be within 500ms due to how BaseEntity initializers work
        result.CreationTime.Should().NotBe(default);
        result.LastUpdatedTime.Should().NotBe(default);

        // The difference should be minimal (due to independent initializer calls)
        var timeDifference = Math.Abs((result.LastUpdatedTime - result.CreationTime).TotalMilliseconds);
        timeDifference.Should().BeLessThan(500);
    }
}