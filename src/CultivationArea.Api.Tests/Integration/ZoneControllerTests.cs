using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace CultivationArea.Api.Tests.Integration;

public class ZoneControllerTests
{
    [Fact]
    public async Task TC_I18_ZoneController_CreateZoneWithinLimits_Returns201Created()
    {
        // Arrange
        var mockService = new Mock<IZoneApplicationService>();
        mockService.Setup(s => s.CreateZoneAsync("Invernadero 1", 4))
                       .ReturnsAsync(new ZoneResult { Success = true, IsConflict = false });

        var controller = new ZoneController(mockService.Object);

        // Act
        var result = await controller.CreateZone(new CreateZoneRequest("Invernadero 1", 4));

        // Assert
        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task TC_I19_ZoneController_CreateZoneExceedingContractLimit_Returns409Conflict()
    {
        // Arrange
        var mockService = new Mock<IZoneApplicationService>();
        mockService.Setup(s => s.CreateZoneAsync("Invernadero Extremo", 10))
                       .ReturnsAsync(new ZoneResult { Success = false, IsConflict = true });

        var controller = new ZoneController(mockService.Object);

        // Act
        var result = await controller.CreateZone(new CreateZoneRequest("Invernadero Extremo", 10));

        // Assert
        var conflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflictResult.StatusCode.Should().Be(409);
    }
}

public record CreateZoneRequest(string Name, int CurrentZones);
public class ZoneResult { public bool Success { get; set; } public bool IsConflict { get; set; } }
public interface IZoneApplicationService { Task<ZoneResult> CreateZoneAsync(string n, int c); }
public class ZoneController : ControllerBase 
{ 
    private readonly IZoneApplicationService _s; 
    public ZoneController(IZoneApplicationService s) => _s = s; 
    [HttpPost] public async Task<IActionResult> CreateZone([FromBody] CreateZoneRequest r) 
    { 
        var res = await _s.CreateZoneAsync(r.Name, r.CurrentZones); 
        return res.IsConflict ? new ConflictObjectResult("Excede límite") : new CreatedResult("", res); 
    } 
}
