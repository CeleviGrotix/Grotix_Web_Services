using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GrotixBackend.IAM.Domain.Model.Commands;

namespace GrotixBackend.IAM.Interfaces.REST.Controllers;

/// <summary>Informe Profile: AuthController — alta de cuenta y sesión.</summary>
[ApiController]
[Route("api/v1/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    public record RegisterRequest(string Email, string Password);

    /// <summary>Registrar cuenta (CreateAccount).</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var id = await mediator.Send(new CreateAccountCommand(request.Email, request.Password));
        return StatusCode(201, new { message = "Registro exitoso.", identityId = id });
    }

    /// <summary>Iniciar sesión.</summary>
    [HttpPost("sign-in")]
    [AllowAnonymous]
    public async Task<IActionResult> SignIn([FromBody] LoginCommand command)
    {
        var response = await mediator.Send(command);
        if (!response.Success)
            return Unauthorized(new { response.Message });
        return Ok(response);
    }

    /// <summary>Cerrar sesión (JWT stateless: el cliente debe descartar el token).</summary>
    [HttpPost("sign-out")]
    [Authorize]
    public IActionResult SignOut() => NoContent();
}
