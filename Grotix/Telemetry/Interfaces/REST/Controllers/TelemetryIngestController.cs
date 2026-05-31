using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Contracts.Integration.Telemetry;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.Telemetry.Application.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.Telemetry.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/telemetry")]
[Authorize]
public sealed class TelemetryIngestController(ITelemetryIngestService ingestService) : ControllerBase
{
    /// <summary>Paquete de telemetría completo del ESP32.</summary>
    public sealed record IngestReadingRequest(
        int DeviceId,
        int ZoneId,
        double Temperature,
        double HumidityAir,
        double HumiditySoil,
        double LightIntensity,
        DateTime? Timestamp);

    /// <summary>Ingesta un paquete de telemetría del ESP32 (misma lógica que <c>telemetry.received</c> por RabbitMQ).</summary>
    [HttpPost("ingest")]
    public async Task<IActionResult> Ingest(
        [FromBody] IngestReadingRequest request,
        CancellationToken cancellationToken)
    {
        if (!CanIngest()) return Forbid();

        if (request.DeviceId <= 0)
            return BadRequest(new { message = "DeviceId inválido." });
        if (request.ZoneId <= 0)
            return BadRequest(new { message = "ZoneId inválido." });

        var evt = new TelemetryReceivedIntegrationEvent(
            request.DeviceId,
            request.ZoneId,
            request.Temperature,
            request.HumidityAir,
            request.HumiditySoil,
            request.LightIntensity,
            request.Timestamp?.ToUniversalTime() ?? DateTime.UtcNow);

        await ingestService.IngestAsync(evt, cancellationToken);
        return Accepted(new { deviceId = request.DeviceId, zoneId = request.ZoneId, ingested = true });
    }

    private bool CanIngest() =>
        User.IsInRole("admin") || User.HasPermission(KnownPermissionCodes.DeviceConfig);
}
