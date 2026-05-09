using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.Shared.Interfaces.REST.Controllers;

/// <summary>Liveness probe: el proceso HTTP responde (no comprueba MySQL).</summary>
/// <remarks>Use <c>/ready/core</c> (MySQL) y <c>/ready/telemetry</c> (Timescale) según corresponda.</remarks>
[ApiController]
[Route("live")]
[AllowAnonymous]
public sealed class LiveController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok();
}
