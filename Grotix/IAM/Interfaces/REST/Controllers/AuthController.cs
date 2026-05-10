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
    public record RegisterRequest(string Email, string Password, string InviteToken);

    /// <summary>Registrar cuenta; el correo debe coincidir con el de la invitación (<c>InviteToken</c>).</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var id = await mediator.Send(new CreateAccountCommand(request.Email, request.Password, request.InviteToken));
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
    public new IActionResult SignOut() => NoContent();
}
