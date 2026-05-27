using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GrotixBackend.HardwareDevice.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/hardware/health")]
[AllowAnonymous]
public sealed class HardwareHealthController(HealthCheckService healthChecks) : ControllerBase
{
    [HttpGet("live")]
    public IActionResult Live() =>
        Ok(new { status = "OK", timestamp = DateTime.UtcNow });

    [HttpGet("ready")]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken)
    {
        var report = await healthChecks.CheckHealthAsync(cancellationToken);
        var coreOk = report.Entries.TryGetValue("mysql", out var mysql) && mysql.Status == HealthStatus.Healthy;

        var payload = new
        {
            status = coreOk ? "OK" : "UNHEALTHY",
            checks = new { coreDatabase = coreOk }
        };

        return coreOk ? Ok(payload) : StatusCode(StatusCodes.Status503ServiceUnavailable, payload);
    }
}
