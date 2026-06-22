using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Profiles.Api.Tests.Integration;

public class AuthControllerTests
{
    [Fact]
    public async Task TC_I14_AuthController_RegisterWithValidToken_Returns201Created()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthApplicationService>();
        mockAuthService.Setup(s => s.RegisterWithTokenAsync("valid-token", "user@grotix.pe"))
                       .ReturnsAsync(new RegistrationResult { Success = true, UserId = 45 });

        var controller = new AuthController(mockAuthService.Object);

        // Act
        var result = await controller.Register(new RegisterRequest("valid-token", "user@grotix.pe"));

        // Assert
        var createdResult = result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.StatusCode.Should().Be(201);
    }

    [Fact]
    public async Task TC_I15_AuthController_RegisterWithExpiredToken_Returns400BadRequest()
    {
        // Arrange
        var mockAuthService = new Mock<IAuthApplicationService>();
        mockAuthService.Setup(s => s.RegisterWithTokenAsync("expired-token", "user@grotix.pe"))
                       .ReturnsAsync(new RegistrationResult { Success = false, Error = "Token vencido" });

        var controller = new AuthController(mockAuthService.Object);

        // Act
        var result = await controller.Register(new RegisterRequest("expired-token", "user@grotix.pe"));

        // Assert
        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(400);
    }
}

public record RegisterRequest(string Token, string Email);
public class RegistrationResult { public bool Success { get; set; } public int UserId { get; set; } public string Error { get; set; } }
public interface IAuthApplicationService { Task<RegistrationResult> RegisterWithTokenAsync(string t, string e); }
public class AuthController : ControllerBase 
{ 
    private readonly IAuthApplicationService _s; 
    public AuthController(IAuthApplicationService s) => _s = s; 
    [HttpPost] public async Task<IActionResult> Register([FromBody] RegisterRequest r) 
    { 
        var res = await _s.RegisterWithTokenAsync(r.Token, r.Email); 
        return res.Success ? new CreatedResult("", res) : new BadRequestObjectResult(res.Error); 
    } 
}