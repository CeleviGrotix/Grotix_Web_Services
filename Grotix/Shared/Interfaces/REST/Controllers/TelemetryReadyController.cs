using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GrotixBackend.Shared.Interfaces.REST.Controllers;

/// <summary>Readiness solo para TimescaleDB (telemetría). No mezcla con MySQL.</summary>
[ApiController]
[Route("ready/telemetry")]
[AllowAnonymous]
public sealed class TelemetryReadyController(HealthCheckService healthChecks) : ControllerBase
{
    private static readonly Func<HealthCheckRegistration, bool> TimescalePredicate =
        r => r.Tags.Contains("timescale");

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var report = await healthChecks.CheckHealthAsync(TimescalePredicate, cancellationToken);
        var payload = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                exception = e.Value.Exception?.Message
            })
        };

        return report.Status == HealthStatus.Healthy
            ? Ok(payload)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, payload);
    }
}
