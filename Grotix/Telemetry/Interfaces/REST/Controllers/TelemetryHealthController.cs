using GrotixBackend.BuildingBlocks.RabbitMq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace GrotixBackend.Telemetry.Interfaces.REST.Controllers;

[ApiController]
[Route("health")]
[AllowAnonymous]
public sealed class TelemetryHealthController(
    HealthCheckService healthChecks,
    IOptions<RabbitMqOptions> rabbitMqOptions,
    IServiceProvider services) : ControllerBase
{
    [HttpGet("live")]
    public IActionResult Live()
    {
        var brokerOk = IsMessageBrokerReachable();
        return Ok(new
        {
            status = brokerOk ? "OK" : "DEGRADED",
            timestamp = DateTime.UtcNow,
            messageBroker = brokerOk
        });
    }

    [HttpGet("ready")]
    public async Task<IActionResult> Ready(CancellationToken cancellationToken)
    {
        var report = await healthChecks.CheckHealthAsync(cancellationToken);
        var brokerOk = IsMessageBrokerReachable();

        var databaseOk = report.Entries.TryGetValue("timescale", out var ts)
            && ts.Status == HealthStatus.Healthy;
        var coreOk = report.Entries.TryGetValue("mysql", out var mysql)
            && mysql.Status == HealthStatus.Healthy;

        var allOk = databaseOk && coreOk && brokerOk;
        var payload = new
        {
            status = allOk ? "OK" : "UNHEALTHY",
            checks = new
            {
                database = databaseOk,
                coreDatabase = coreOk,
                messageBroker = brokerOk
            }
        };

        return allOk ? Ok(payload) : StatusCode(StatusCodes.Status503ServiceUnavailable, payload);
    }

    private bool IsMessageBrokerReachable()
    {
        if (!rabbitMqOptions.Value.Enabled)
            return true;

        var holder = services.GetService(typeof(RabbitMqConnectionHolder)) as RabbitMqConnectionHolder;
        return holder?.TryGetConnection() != null;
    }
}
