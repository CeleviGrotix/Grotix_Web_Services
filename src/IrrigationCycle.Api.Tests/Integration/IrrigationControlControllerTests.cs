using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Irrigation.Api.Tests.Integration;

public class IrrigationControlControllerTests
{
    [Fact]
    public async Task TC_I21_IrrigationController_StartManualAuthorized_Returns200Ok()
    {
        // Arrange
        var mockService = new Mock<IIrrigationCommandService>();
        mockService.Setup(s => s.TriggerManualIrrigationAsync(3, "user_advanced"))
                   .ReturnsAsync(new IrrigationTriggerResult { Success = true, CycleId = 99 });

        var controller = new IrrigationController(mockService.Object);

        // Act
        var result = await controller.StartManual(new IrrigationRequest(3), "user_advanced");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task TC_I22_IrrigationController_StartManualOnForeignZone_Returns403Forbidden()
    {
        // Arrange
        var mockService = new Mock<IIrrigationCommandService>();
        mockService.Setup(s => s.TriggerManualIrrigationAsync(5, "user_advanced"))
                   .ReturnsAsync(new IrrigationTriggerResult { Success = false, IsForbidden = true });

        var controller = new IrrigationController(mockService.Object);

        // Act
        var result = await controller.StartManual(new IrrigationRequest(5), "user_advanced");

        // Assert
        var forbiddenResult = result.Should().BeOfType<ForbidResult>().Subject;
    }
}

public record IrrigationRequest(int ZoneId);
public class IrrigationTriggerResult { public bool Success { get; set; } public int CycleId { get; set; } public bool IsForbidden { get; set; } }
public interface IIrrigationCommandService { Task<IrrigationTriggerResult> TriggerManualIrrigationAsync(int z, string r); }
public class IrrigationController : ControllerBase 
{ 
    private readonly IIrrigationCommandService _s; 
    public IrrigationController(IIrrigationCommandService s) => _s = s; 
    [HttpPost] public async Task<IActionResult> StartManual([FromBody] IrrigationRequest r, string role) 
    { 
        var res = await _s.TriggerManualIrrigationAsync(r.ZoneId, role); 
        return res.IsForbidden ? new ForbidResult() : new OkObjectResult(res); 
    } 
}