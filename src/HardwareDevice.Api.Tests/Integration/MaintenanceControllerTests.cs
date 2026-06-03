using FluentAssertions;
using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.HardwareDevice.Application.ACL;
using GrotixBackend.HardwareDevice.Application.Internal;
using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;
using GrotixBackend.HardwareDevice.Domain.Model.Entities;
using GrotixBackend.HardwareDevice.Domain.Repositories;
using GrotixBackend.HardwareDevice.Interfaces.REST.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace HardwareDevice.Api.Tests.Integration;

public class MaintenanceControllerTests
{
    private MaintenanceController SetupController(bool isAuthorized, Mock<IMaintenanceService> maintenanceService)
    {
        var mockAccess = new Mock<IUserAccessContextService>();
        var mockZoneAccess = new Mock<IZoneAccessService>();
        var mockDeviceQuery = new Mock<IDeviceQueryService>();

        if (isAuthorized)
        {
            mockAccess.Setup(a => a.GetByIdentityIdAsync(100, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new UserAccessContext(1, 1, 10, true));

            mockZoneAccess.Setup(z => z.CanAccessZoneAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int?>()))
                        .ReturnsAsync(true);

            mockDeviceQuery.Setup(d => d.GetDetailAsync(1))
                        .ReturnsAsync(new DeviceDetail(
                            new Microcontroller("ESP", "MAC", 1),
                            new List<DeviceSensor>(),
                            new List<DeviceActuator>()
                        ));
        }


        var controller = new MaintenanceController(
            mockAccess.Object,
            mockZoneAccess.Object,
            mockDeviceQuery.Object,
            maintenanceService.Object,
            new Mock<IActionQueueRepository>().Object);

        var claims = new[] { 
            new Claim(ClaimTypes.Role, "admin"),
            new Claim(JwtClaimTypes.IdentityId, "100") 
        };
        var identity = new ClaimsIdentity(claims, "TestAuth", ClaimTypes.NameIdentifier, ClaimTypes.Role);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        return controller;
    }

    // Clase auxiliar para igualar la firma de tu QueryService
    private record DeviceDetailDto(Microcontroller Device, IEnumerable<DeviceSensor> Sensors, IEnumerable<DeviceActuator> Actuators);

    [Fact]
    public async Task CreateMaintenanceLog_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var mockService = new Mock<IMaintenanceService>();
        mockService.Setup(s => s.RecordMaintenanceLogAsync(1, 1, "Reinicio", "OK", default))
                   .ReturnsAsync(new MaintenanceLog(1, 1, "Reinicio", "OK"));

        var controller = SetupController(isAuthorized: true, mockService);
        var request = new MaintenanceController.CreateMaintenanceLogRequest("Reinicio", "OK");

        // Act
        var result = await controller.CreateMaintenanceLog(1, request, CancellationToken.None);

        // Assert
        var createdResult = result as CreatedResult;
        createdResult.Should().NotBeNull();
        createdResult!.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task CreateTechnicalMaintenance_DeviceNotFound_ReturnsNotFound()
    {
        // Arrange
        var mockService = new Mock<IMaintenanceService>();
        var controller = SetupController(isAuthorized: false, mockService); // isAuthorized = false no mockea GetDetailAsync -> devuelve nulo
        var request = new MaintenanceController.CreateTechnicalMaintenanceRequest(10, "Preventivo", "Desc", null);

        // Act
        var result = await controller.CreateTechnicalMaintenance(99, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task ListMaintenanceLogs_ValidRequest_ReturnsOk()
    {
        // Arrange
        var mockService = new Mock<IMaintenanceService>();
        mockService.Setup(s => s.ListMaintenanceLogsAsync(1, 50, default))
                   .ReturnsAsync(new List<MaintenanceLog> { new MaintenanceLog(1, 1, "Limpieza", "OK") });

        var controller = SetupController(isAuthorized: true, mockService);

        // Act
        var result = await controller.ListMaintenanceLogs(1, 50, CancellationToken.None);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }
}