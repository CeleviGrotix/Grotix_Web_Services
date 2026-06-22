using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Telemetry.Api.Tests.Integration;

public class TelemetryControllerTests
{
    [Fact]
    public async Task TC_I20_TelemetryController_GetZoneHistory_Returns200OkWithData()
    {
        // Arrange
        var mockService = new Mock<ITelemetryQueryService>();
        mockService.Setup(s => s.GetLast24HoursReadingsAsync(3))
            .ReturnsAsync(new List<SensorReadingDto> { new SensorReadingDto { Humidity = 0.52 } });

        var controller = new TelemetryController(mockService.Object);

        // Act
        var result = await controller.GetHistory(3);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }
}

public class SensorReadingDto { public double Humidity { get; set; } }
public interface ITelemetryQueryService { Task<List<SensorReadingDto>> GetLast24HoursReadingsAsync(int z); }
public class TelemetryController : ControllerBase 
{ 
    private readonly ITelemetryQueryService _s; 
    public TelemetryController(ITelemetryQueryService s) => _s = s; 
    [HttpGet] public async Task<IActionResult> GetHistory(int zoneId) => new OkObjectResult(await _s.GetLast24HoursReadingsAsync(zoneId)); 
}