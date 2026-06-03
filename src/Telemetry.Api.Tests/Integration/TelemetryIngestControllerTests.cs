using FluentAssertions;
using GrotixBackend.Telemetry.Application.Internal;
using GrotixBackend.Telemetry.Interfaces.REST.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Telemetry.Api.Tests.Integration;

public class TelemetryIngestControllerTests
{
    private TelemetryIngestController SetupController(bool isAuthenticated, string role = "admin")
    {
        var mockService = new Mock<ITelemetryIngestService>();
        var controller = new TelemetryIngestController(mockService.Object);

        if (isAuthenticated)
        {
            var claims = new[] { new Claim(ClaimTypes.Role, role) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }
        else
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity()) }
            };
        }

        return controller;
    }

    [Fact]
    public async Task Ingest_ValidRequestAsAdmin_ReturnsAccepted()
    {
        // Arrange
        var controller = SetupController(isAuthenticated: true, role: "admin");
        var request = new TelemetryIngestController.IngestReadingRequest(1, 2, 25.5, 60, 45, 1000, DateTime.UtcNow);

        // Act
        var result = await controller.Ingest(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<AcceptedResult>();
    }

    [Fact]
    public async Task Ingest_InvalidDeviceId_ReturnsBadRequest()
    {
        // Arrange
        var controller = SetupController(isAuthenticated: true, role: "admin");
        var request = new TelemetryIngestController.IngestReadingRequest(0, 2, 25.5, 60, 45, 1000, DateTime.UtcNow); // DeviceId 0 es inválido

        // Act
        var result = await controller.Ingest(request, CancellationToken.None);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult.Should().NotBeNull();
        badRequestResult?.Value?.ToString().Should().Contain("DeviceId inválido");
    }

    [Fact]
    public async Task Ingest_UnauthorizedUser_ReturnsForbid()
    {
        // Arrange
        var controller = SetupController(isAuthenticated: true, role: "guest"); // Rol sin permisos
        var request = new TelemetryIngestController.IngestReadingRequest(1, 2, 25.5, 60, 45, 1000, DateTime.UtcNow);

        // Act
        var result = await controller.Ingest(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }
}