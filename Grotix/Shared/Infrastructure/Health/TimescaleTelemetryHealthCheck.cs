using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Npgsql;

namespace GrotixBackend.Shared.Infrastructure.Health;

/// <summary>Readiness de TimescaleDB/PostgreSQL (telemetría): ping vía <c>SELECT 1</c>.</summary>
public sealed class TimescaleTelemetryHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var cs = configuration.GetConnectionString("TelemetryTimescale");
        if (string.IsNullOrWhiteSpace(cs))
            return HealthCheckResult.Degraded("TelemetryTimescale connection string not configured");

        try
        {
            await using var conn = new NpgsqlConnection(cs);
            await conn.OpenAsync(cancellationToken);
            await using var cmd = new NpgsqlCommand("SELECT 1", conn);
            _ = await cmd.ExecuteScalarAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("TimescaleDB not reachable", ex);
        }
    }
}
