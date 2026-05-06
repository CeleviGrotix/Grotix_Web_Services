using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GrotixBackend.Shared.Interfaces.REST.Controllers;

/// <summary>Readiness probe: ping a MySQL central vía health checks (<c>SELECT 1</c>).</summary>
/// <remarks>
/// <para><b>/live</b> solo confirma que el proceso HTTP responde; no usa la base de datos.</para>
/// <para><b>/ready</b> debe fallar (503) si MySQL no está disponible — útil para readiness en orquestadores.</para>
/// </remarks>
[ApiController]
[Route("ready")]
[AllowAnonymous]
public sealed class ReadyController(HealthCheckService healthChecks) : ControllerBase
{
    private static readonly Func<HealthCheckRegistration, bool> ReadyPredicate =
        r => r.Tags.Contains("ready");

    /// <summary>Ejecuta los checks etiquetados <c>ready</c> (actualmente MySQL).</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var report = await healthChecks.CheckHealthAsync(ReadyPredicate, cancellationToken);
        var payload = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        };

        return report.Status == HealthStatus.Healthy
            ? Ok(payload)
            : StatusCode(StatusCodes.Status503ServiceUnavailable, payload);
    }
}
