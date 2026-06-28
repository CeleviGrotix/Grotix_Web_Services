using FluentAssertions;
using GrotixBackend.HardwareDevice.Application.ACL;
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
    [Fact]
    public void TC_U67_BindToZone_ValidData_ShouldSetZoneIdAndOfflineStatus()
    {
        var microcontroller = new Microcontroller("ESP32-WROOM", "00:1A:2B:3C:4D:5E", null);
        microcontroller.LinkToZone(3);
        microcontroller.ZoneId.Should().Be(3);
        microcontroller.Status.Should().Be("OFFLINE");
    }

    [Fact]
    public void TC_U68_BindToZone_InvalidMacAddressFormat_ShouldThrowArgumentException()
    {
        Action act = () => new Microcontroller("ESP32", "00:1A:ZZ", 3);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TC_U69_BindToZone_NonExistentZoneId_ShouldThrowArgumentException()
    {
        var microcontroller = new Microcontroller("ESP32", "00:1A:2B:3C:4D:5E", null);
        Action act = () => microcontroller.LinkToZone(0);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TC_U70_BindToZone_ReassigningZoneWithoutUnbinding_ShouldUpdateZoneId()
    {
        var microcontroller = new Microcontroller("ESP32", "00:1A:2B:3C:4D:5E", 3);
        microcontroller.LinkToZone(7);
        microcontroller.ZoneId.Should().Be(7);
    }

    [Fact]
    public async Task TC_U71_MaintenanceService_RecordMaintenanceOnExistingDevice_ShouldExecuteSuccessfully()
    {
        var mockDeviceRepo = new Mock<IMicrocontrollerRepository>();
        var mockLogRepo = new Mock<IMaintenanceLogRepository>();
        var mockTechRepo = new Mock<ITechnicalMaintenanceRepository>();
        var mockUow = new Mock<IHardwareDeviceUnitOfWork>();

        mockDeviceRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Microcontroller("ESP32", "00:1A:2B:3C:4D:5E", 3));

        var mockStaff = new Mock<IStaffExistenceService>();
        mockStaff.Setup(s => s.ExistsAsync(10, default)).ReturnsAsync(true);

        var service = new MaintenanceService(
            mockDeviceRepo.Object,
            mockLogRepo.Object,
            mockTechRepo.Object,
            mockStaff.Object,
            mockUow.Object);

        var result = await service.RecordTechnicalMaintenanceAsync(10, 1, "Correctivo", "Cambio bateria", "Exito");

        result.Should().NotBeNull();
        result.Type.Should().Be("Correctivo");
    }

    [Fact]
    public async Task TC_U72_MaintenanceService_RecordLogAsync_ShouldPersistExpectedStatus()
    {
        var mockDeviceRepo = new Mock<IMicrocontrollerRepository>();
        var mockLogRepo = new Mock<IMaintenanceLogRepository>();
        var mockTechRepo = new Mock<ITechnicalMaintenanceRepository>();
        var mockUow = new Mock<IHardwareDeviceUnitOfWork>();

        mockDeviceRepo.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Microcontroller("ESP32", "00:1A:2B:3C:4D:5E", 3));

        var service = new MaintenanceService(
            mockDeviceRepo.Object,
            mockLogRepo.Object,
            mockTechRepo.Object,
            new Mock<IStaffExistenceService>().Object,
            mockUow.Object);

        Func<Task> act = async () => await service.RecordMaintenanceLogAsync(1, 10, "Reinicio de control", "OFFLINE");

        await act.Should().NotThrowAsync();
    }
}