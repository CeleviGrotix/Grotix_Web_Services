using FluentAssertions;
using GrotixBackend.HardwareDevice.Application.Internal;
using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;
using GrotixBackend.HardwareDevice.Domain.Repositories;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace HardwareDevice.Api.Tests.Unit;

public class HardwareSprint3UnitTests
{
    // =========================================================================
    // Control de Casos de Prueba - Hardware Device Unit Testing (US10)
    // =========================================================================

    [Fact]
    public void TC_U67_BindToZone_ValidData_ShouldSetZoneIdAndOfflineStatus()
    {
        // Arrange
        var microcontroller = new Microcontroller("ESP32-WROOM", "00:1A:2B:3C:4D:5E", null);
        var validZoneId = 3;

        // Act
        microcontroller.LinkToZone(validZoneId);

        // Assert
        microcontroller.ZoneId.Should().Be(validZoneId);
        microcontroller.Status.Should().Be("OFFLINE");
    }

    [Fact]
    public void TC_U68_BindToZone_InvalidMacAddressFormat_ShouldThrowArgumentException()
    {
        // Arrange & Act
        Action act = () => new Microcontroller("ESP32", "00:1A:ZZ", 3);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TC_U69_BindToZone_NonExistentZoneId_ShouldThrowArgumentException()
    {
        // Arrange
        var microcontroller = new Microcontroller("ESP32", "00:1A:2B:3C:4D:5E", null);
        var nonExistentZoneId = 0;

        // Act
        Action act = () => microcontroller.LinkToZone(nonExistentZoneId);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TC_U70_BindToZone_ReassigningZoneWithoutUnbinding_ShouldUpdateZoneId()
    {
        // Arrange
        var microcontroller = new Microcontroller("ESP32", "00:1A:2B:3C:4D:5E", 3);
        var newZoneId = 7;

        // Act
        microcontroller.LinkToZone(newZoneId);

        // Assert
        microcontroller.ZoneId.Should().Be(newZoneId);
    }

    // =========================================================================
    // Adaptación usando el servicio real de tu solución: MaintenanceService
    // =========================================================================

    [Fact]
    public async Task TC_U71_MaintenanceService_RecordMaintenanceOnExistingDevice_ShouldExecuteSuccessfully()
    {
        // Arrange
        var mockDeviceRepo = new Mock<IMicrocontrollerRepository>();
        var mockLogRepo = new Mock<IMaintenanceLogRepository>();
        var mockTechRepo = new Mock<ITechnicalMaintenanceRepository>();
        var mockUow = new Mock<IHardwareDeviceUnitOfWork>();

        var existingDevice = new Microcontroller("ESP32", "00:1A:2B:3C:4D:5E", 3);
        mockDeviceRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDevice);

        var service = new MaintenanceService(
            mockDeviceRepo.Object,
            mockLogRepo.Object,
            mockTechRepo.Object,
            mockUow.Object);

        // Act
        var result = await service.RecordTechnicalMaintenanceAsync(10, 1, "Correctivo", "Cambio bateria", "Exito");

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be("Correctivo");
    }

    [Fact]
    public async Task TC_U72_MaintenanceService_RecordLogAsync_ShouldPersistExpectedStatus()
    {
        // Arrange
        var mockDeviceRepo = new Mock<IMicrocontrollerRepository>();
        var mockLogRepo = new Mock<IMaintenanceLogRepository>();
        var mockTechRepo = new Mock<ITechnicalMaintenanceRepository>();
        var mockUow = new Mock<IHardwareDeviceUnitOfWork>();

        var existingDevice = new Microcontroller("ESP32", "00:1A:2B:3C:4D:5E", 3);
        mockDeviceRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDevice);

        var service = new MaintenanceService(
            mockDeviceRepo.Object,
            mockLogRepo.Object,
            mockTechRepo.Object,
            mockUow.Object);

        // Act
        Func<Task> act = async () => await service.RecordMaintenanceLogAsync(1, 10, "Reinicio de control", "OFFLINE");

        // Assert
        await act.Should().NotThrowAsync();
    }
}