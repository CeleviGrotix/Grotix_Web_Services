using FluentAssertions;
using GrotixBackend.HardwareDevice.Application.Internal;
using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;
using GrotixBackend.HardwareDevice.Domain.Model.Entities;
using GrotixBackend.HardwareDevice.Domain.Repositories;
using Moq;
using Xunit;
using GrotixBackend.HardwareDevice.Application.ACL;

namespace HardwareDevice.Api.Tests.Unit;

public class HardwareDomainAndServiceTests
{
    // ==========================================
    // TDD: Microcontroller Aggregate (5 Tests)
    // ==========================================
    [Fact]
    public void MicrocontrollerConstructor_ValidData_CreatesInstanceAndSetsOffline()
    {
        var device = new Microcontroller("ESP32-WROOM", "00:1A:2B:3C:4D:5E", 1);
        
        device.Model.Should().Be("ESP32-WROOM");
        device.MacAddress.Should().Be("00:1A:2B:3C:4D:5E");
        device.ZoneId.Should().Be(1);
        device.Status.Should().Be("OFFLINE"); // Asume que ValueObjects.DeviceStatuses.Offline es "OFFLINE"
    }

    [Fact]
    public void MicrocontrollerConstructor_EmptyModel_ThrowsArgumentException()
    {
        Action act = () => new Microcontroller("", "00:1A:2B:3C:4D:5E", 1);
        act.Should().Throw<ArgumentException>().WithMessage("Model no puede estar vacío.");
    }

    [Fact]
    public void MicrocontrollerConstructor_EmptyMac_ThrowsArgumentException()
    {
        Action act = () => new Microcontroller("ESP32", "", 1);
        act.Should().Throw<ArgumentException>().WithMessage("MacAddress no puede estar vacío.");
    }

    [Fact]
    public void LinkToZone_ValidZoneId_UpdatesZoneId()
    {
        var device = new Microcontroller("ESP32", "00:1A:2B:3C:4D:5E", null);
        device.LinkToZone(5);
        device.ZoneId.Should().Be(5);
    }

    [Fact]
    public void LinkToZone_InvalidZoneId_ThrowsArgumentException()
    {
        var device = new Microcontroller("ESP32", "MAC", null);
        Action act = () => device.LinkToZone(0);
        act.Should().Throw<ArgumentException>().WithMessage("ZoneId inválido.");
    }

    // ==========================================
    // TDD: TechnicalMaintenance Entity (2 Tests)
    // ==========================================
    [Fact]
    public void TechnicalMaintenanceConstructor_ValidData_CreatesInstance()
    {
        var maintenance = new TechnicalMaintenance(10, 20, "Preventivo", "Limpieza de sensor");
        maintenance.StaffId.Should().Be(10);
        maintenance.DeviceId.Should().Be(20);
        maintenance.Type.Should().Be("Preventivo");
    }

    [Fact]
    public void TechnicalMaintenanceConstructor_InvalidStaffId_ThrowsArgumentException()
    {
        Action act = () => new TechnicalMaintenance(0, 20, "Preventivo", "Limpieza");
        act.Should().Throw<ArgumentException>().WithMessage("StaffId inválido.");
    }

    // ==========================================
    // TDD: MaintenanceService (3 Tests)
    // ==========================================
    [Fact]
    public async Task RecordMaintenanceLogAsync_DeviceNotFound_ThrowsKeyNotFoundException()
    {
        var mockRepo = new Mock<IMicrocontrollerRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Microcontroller?)null);

        var service = new MaintenanceService(
            mockRepo.Object, 
            new Mock<IMaintenanceLogRepository>().Object, 
            new Mock<ITechnicalMaintenanceRepository>().Object, 
            new Mock<IStaffExistenceService>().Object,
            new Mock<IHardwareDeviceUnitOfWork>().Object);

        Func<Task> act = async () => await service.RecordMaintenanceLogAsync(1, 10, "Reinicio", "ONLINE");
        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Device 1 not found.");
    }

    [Fact]
    public async Task RecordTechnicalMaintenanceAsync_ValidRequest_ReturnsRecord()
    {
        var mockRepo = new Mock<IMicrocontrollerRepository>();
        mockRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Microcontroller("ESP", "MAC"));

        var service = new MaintenanceService(
            mockRepo.Object, 
            new Mock<IMaintenanceLogRepository>().Object, 
            new Mock<ITechnicalMaintenanceRepository>().Object,
            new Mock<IStaffExistenceService>().Object, 
            new Mock<IHardwareDeviceUnitOfWork>().Object);

        var result = await service.RecordTechnicalMaintenanceAsync(10, 1, "Correctivo", "Cambio bateria", "Exito");
        
        result.Should().NotBeNull();
        result.Type.Should().Be("Correctivo");
        result.Description.Should().Be("Cambio bateria");
    }
    
    [Fact]
    public async Task ListMaintenanceLogsAsync_CallsRepository()
    {
        var mockLogRepo = new Mock<IMaintenanceLogRepository>();
        var service = new MaintenanceService(
            new Mock<IMicrocontrollerRepository>().Object, 
            mockLogRepo.Object, 
            new Mock<ITechnicalMaintenanceRepository>().Object, 
            new Mock<IStaffExistenceService>().Object,
            new Mock<IHardwareDeviceUnitOfWork>().Object);

        await service.ListMaintenanceLogsAsync(1, 10);
        mockLogRepo.Verify(r => r.ListByDeviceAsync(1, 10, default), Times.Once);
    }
}