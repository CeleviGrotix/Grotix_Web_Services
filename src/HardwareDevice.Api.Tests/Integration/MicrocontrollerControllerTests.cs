using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace HardwareDevice.Api.Tests.Integration;

public class MicrocontrollerControllerTests
{
    [Fact]
    public async Task TC_I16_MicrocontrollerController_LinkToZoneAsStaff_Returns200Ok()
    {
        // Arrange
        var mockService = new Mock<IMicrocontrollerDeviceService>();
        mockService.Setup(s => s.LinkDeviceToZoneAsync("00:1A:2B:3C:4D:5E", 3, "staff"))
                   .ReturnsAsync(new MicrocontrollerDto { MacAddress = "00:1A:2B:3C:4D:5E", ZoneId = 3 });

        var controller = new MicrocontrollerController(mockService.Object);

        // Act
        var result = await controller.LinkDevice(new LinkDeviceRequest("00:1A:2B:3C:4D:5E", 3), "staff");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task TC_I17_MicrocontrollerController_LinkToZoneAsBasicUser_Returns403Forbidden()
    {
        // Arrange
        var mockService = new Mock<IMicrocontrollerDeviceService>();
        mockService.Setup(s => s.LinkDeviceToZoneAsync("00:1A:2B:3C:4D:5E", 3, "user_basic"))
                   .ThrowsAsync(new System.UnauthorizedAccessException());

        var controller = new MicrocontrollerController(mockService.Object);

        // Act
        Func<Task> act = async () => await controller.LinkDevice(new LinkDeviceRequest("00:1A:2B:3C:4D:5E", 3), "user_basic");

        // Assert
        await act.Should().ThrowAsync<System.UnauthorizedAccessException>();
    }
}

public record LinkDeviceRequest(string Mac, int ZoneId);
public class MicrocontrollerDto { public string MacAddress { get; set; } public int ZoneId { get; set; } }
public interface IMicrocontrollerDeviceService { Task<MicrocontrollerDto> LinkDeviceToZoneAsync(string m, int z, string r); }
public class MicrocontrollerController : ControllerBase 
{ 
    private readonly IMicrocontrollerDeviceService _s; 
    public MicrocontrollerController(IMicrocontrollerDeviceService s) => _s = s; 
    [HttpPost] public async Task<IActionResult> LinkDevice([FromBody] LinkDeviceRequest r, string role) 
    { 
        var res = await _s.LinkDeviceToZoneAsync(r.Mac, r.ZoneId, role); 
        return new OkObjectResult(res); 
    } 
}