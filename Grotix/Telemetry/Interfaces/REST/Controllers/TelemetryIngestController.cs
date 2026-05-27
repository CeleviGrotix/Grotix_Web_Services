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
    public sealed record IngestReadingRequest(
        int SensorId,
        double Value,
        int? DeviceId,
        DateTime? Timestamp);

    /// <summary>Ingesta una lectura (misma lógica que <c>telemetry.received</c> por RabbitMQ).</summary>
    [HttpPost("ingest")]
    public async Task<IActionResult> Ingest(
        [FromBody] IngestReadingRequest request,
        CancellationToken cancellationToken)
    {
        if (!CanIngest()) return Forbid();

        if (request.SensorId <= 0)
            return BadRequest(new { message = "SensorId inválido." });

        var evt = new TelemetryReceivedIntegrationEvent(
            request.DeviceId ?? 0,
            request.SensorId,
            request.Value,
            request.Timestamp?.ToUniversalTime() ?? DateTime.UtcNow);

        await ingestService.IngestAsync(evt, cancellationToken);
        return Accepted(new { sensorId = request.SensorId, ingested = true });
    }

    private bool CanIngest() =>
        User.IsInRole("admin") || User.HasPermission(KnownPermissionCodes.DeviceConfig);
}
