using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrotixBackend.IAM.Domain.Model.Commands;

namespace GrotixBackend.IAM.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/authentication")]
public class IdentityController(IMediator mediator) : ControllerBase
{
    public record RegisterRequest(string Email, string Password);

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var id = await mediator.Send(new RegisterCommand(request.Email, request.Password));
        return StatusCode(201, new { message = "Registro exitoso.", identityId = id });
    }

    [HttpPost("sign-in")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn([FromBody] LoginCommand command)
    {
        var response = await mediator.Send(command);
        if (!response.Success)
            return Unauthorized(new { response.Message });
        return Ok(response);
    }
}