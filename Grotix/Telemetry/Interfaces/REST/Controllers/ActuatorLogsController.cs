using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.Telemetry.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.Telemetry.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/telemetry/actuators")]
[Authorize]
public sealed class ActuatorLogsController(IActuatorLogRepository actuatorLogRepository) : ControllerBase
{
    [HttpGet("{actuatorId:int}/logs")]
    public async Task<IActionResult> List(
        int actuatorId,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        if (!User.HasPermission(KnownPermissionCodes.TelemetryView) && !User.IsInRole("admin"))
            return Forbid();

        var logs = await actuatorLogRepository.ListByActuatorAsync(
            actuatorId,
            Math.Clamp(limit, 1, 500),
            cancellationToken);

        return Ok(logs.Select(l => new
        {
            logId = l.Id,
            actuatorId = l.ActuatorId,
            action = l.Action,
            duration = l.Duration,
            timestamp = l.Timestamp,
            flowRate = l.FlowRate
        }));
    }
}
