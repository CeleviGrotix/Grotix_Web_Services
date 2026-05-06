using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.Shared.Interfaces.REST.Controllers;

/// <summary>Liveness probe: el proceso HTTP responde (no comprueba MySQL).</summary>
/// <remarks>Use <c>/ready</c> para readiness contra la base de datos.</remarks>
[ApiController]
[Route("live")]
[AllowAnonymous]
public sealed class LiveController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok();
}
