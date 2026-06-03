using FluentAssertions;
using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.IrrigationCycle.Application.ACL;
using GrotixBackend.IrrigationCycle.Application.Internal;
using GrotixBackend.IrrigationCycle.Domain.Model.Aggregates;
using GrotixBackend.IrrigationCycle.Interfaces.REST.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace IrrigationCycle.Api.Tests.Integration;

public class IrrigationControllerTests
{
    private IrrigationController SetupController(
        bool isAuthorized, 
        Mock<IIrrigationCommandService> mockCommandService, 
        string role = "admin")
    {
        var mockUserAccess = new Mock<IUserAccessContextService>();
        var mockZoneAccess = new Mock<IZoneAccessService>();
        var mockQueryService = new Mock<IIrrigationQueryService>();
        var mockScheduleService = new Mock<IIrrigationScheduleService>();

        mockZoneAccess.Setup(z => z.CanAccessZoneAsync(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int?>()))
                      .ReturnsAsync(isAuthorized);

        var controller = new IrrigationController(
            mockUserAccess.Object, mockZoneAccess.Object, mockCommandService.Object, mockQueryService.Object, mockScheduleService.Object);

        var claims = new[] { new Claim(ClaimTypes.Role, role) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        return controller;
    }

    [Fact]
    public async Task Start_ValidRequest_ReturnsOkWithCycleId()
    {
        // Arrange
        var mockCommand = new Mock<IIrrigationCommandService>();
        
        // ¡LA SOLUCIÓN! Le decimos al Mock que devuelva un ciclo válido para que Controller no dé NullReferenceException
        mockCommand.Setup(s => s.StartManualAsync(It.IsAny<int>(), It.IsAny<double?>(), It.IsAny<int?>(), default))
                   .ReturnsAsync(new IrrigationCycleRecord(1, 50.0, 10));

        var controller = SetupController(isAuthorized: true, mockCommand);
        var request = new IrrigationController.StartIrrigationRequest(50.0, 10);
        
        // Act
        var result = await controller.Start(1, request, CancellationToken.None);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Start_UnauthorizedZoneAccess_ReturnsForbid()
    {
        // Arrange
        var mockCommand = new Mock<IIrrigationCommandService>();
        var controller = SetupController(isAuthorized: false, mockCommand, role: "user");
        var request = new IrrigationController.StartIrrigationRequest(50.0, 10);

        // Act
        var result = await controller.Start(1, request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Start_ActiveCycleExists_ReturnsConflict()
    {
        // Arrange
        var mockCommand = new Mock<IIrrigationCommandService>();
        mockCommand.Setup(s => s.StartManualAsync(It.IsAny<int>(), It.IsAny<double?>(), It.IsAny<int?>(), default))
                   .ThrowsAsync(new InvalidOperationException("Ya hay un ciclo activo"));
                   
        var controller = SetupController(isAuthorized: true, mockCommand);
        var request = new IrrigationController.StartIrrigationRequest(50.0, 10);

        // Act
        var result = await controller.Start(1, request, CancellationToken.None);

        // Assert
        var conflictResult = result as ConflictObjectResult;
        conflictResult.Should().NotBeNull();
        conflictResult!.Value.ToString().Should().Contain("Ya hay un ciclo activo");
    }
}