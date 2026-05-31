using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.Telemetry.Application.ACL;
using GrotixBackend.Telemetry.Application.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.Telemetry.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/telemetry/zones")]
[Authorize]
public sealed class TelemetryController(
    IUserAccessContextService userAccessContextService,
    IZoneAuthorizationService zoneAuthorizationService,
    ITelemetryQueryService telemetryQueryService,
    IZoneThresholdService zoneThresholdService,
    IAlertQueryService alertQueryService) : ControllerBase
{
    [HttpGet("{zoneId:int}")]
    public async Task<IActionResult> GetZoneHistory(
        int zoneId,
        [FromQuery] DateTime? startTime,
        [FromQuery] DateTime? endTime,
        [FromQuery] int limit = 1000,
        CancellationToken cancellationToken = default)
    {
        if (!User.HasPermission(KnownPermissionCodes.TelemetryView) && !User.IsInRole("admin"))
            return Forbid();

        if (!await CanAccessZoneAsync(zoneId, cancellationToken))
            return Forbid();

        var history = await telemetryQueryService.GetZoneHistoryAsync(
            zoneId,
            startTime,
            endTime,
            Math.Clamp(limit, 1, 10_000),
            cancellationToken);

        if (history == null)
            return NotFound();

        return Ok(new
        {
            zoneId = history.ZoneId,
            period = new { start = history.Start, end = history.End },
            readings = history.Readings.Select(r => new
            {
                deviceId = r.DeviceId,
                timestamp = r.Timestamp,
                temperature = r.Temperature,
                humidityAir = r.HumidityAir,
                humiditySoil = r.HumiditySoil,
                lightIntensity = r.LightIntensity
            })
        });
    }

    [HttpGet("{zoneId:int}/alerts")]
    public async Task<IActionResult> GetAlerts(
        int zoneId,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        if (!User.HasPermission(KnownPermissionCodes.TelemetryView) && !User.IsInRole("admin"))
            return Forbid();

        if (!await CanAccessZoneAsync(zoneId, cancellationToken))
            return Forbid();

        if (!await zoneAuthorizationService.ZoneExistsAsync(zoneId, cancellationToken))
            return NotFound();

        var alerts = await alertQueryService.ListByZoneAsync(zoneId, limit, cancellationToken);
        return Ok(alerts.Select(a => new
        {
            id = a.Id,
            zoneId = a.ZoneId,
            sensorId = a.SensorId,
            sensorType = a.SensorType,
            value = a.Value,
            minThreshold = a.MinThreshold,
            maxThreshold = a.MaxThreshold,
            breachedThreshold = a.BreachedThreshold,
            breachDirection = a.BreachDirection,
            triggeredAt = a.TriggeredAt
        }));
    }

    [HttpGet("{zoneId:int}/thresholds")]
    public async Task<IActionResult> GetThresholds(int zoneId, CancellationToken cancellationToken = default)
    {
        if (!User.HasPermission(KnownPermissionCodes.TelemetryView) && !User.IsInRole("admin"))
            return Forbid();

        if (!await CanAccessZoneAsync(zoneId, cancellationToken))
            return Forbid();

        if (!await zoneAuthorizationService.ZoneExistsAsync(zoneId, cancellationToken))
            return NotFound();

        var thresholds = await zoneThresholdService.GetEffectiveThresholdsAsync(zoneId, cancellationToken);
        return Ok(thresholds.Select(t => new
        {
            sensorType = t.SensorType,
            minValue = t.MinValue,
            maxValue = t.MaxValue,
            source = t.Source
        }));
    }

    public sealed record ThresholdPatchItem(string SensorType, double? MinValue, double? MaxValue);

    [HttpPatch("{zoneId:int}/thresholds")]
    public async Task<IActionResult> PatchThresholds(
        int zoneId,
        [FromBody] ThresholdPatchItem[] request,
        CancellationToken cancellationToken = default)
    {
        if (!User.HasPermission(KnownPermissionCodes.ThresholdWrite) && !User.IsInRole("admin"))
            return Forbid();

        if (!await CanAccessZoneAsync(zoneId, cancellationToken))
            return Forbid();

        if (!await zoneAuthorizationService.ZoneExistsAsync(zoneId, cancellationToken))
            return NotFound();

        try
        {
            var updates = request
                .Select(r => new ThresholdUpdateRequest(r.SensorType, r.MinValue, r.MaxValue))
                .ToList();

            await zoneThresholdService.UpdateCustomThresholdsAsync(zoneId, updates, cancellationToken);
            return Ok(new { success = true });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private async Task<bool> CanAccessZoneAsync(int zoneId, CancellationToken cancellationToken)
    {
        var profileId = await ResolveProfileUserIdAsync(cancellationToken);
        return await zoneAuthorizationService.CanAccessZoneAsync(
            zoneId,
            User.IsInRole("admin"),
            profileId,
            cancellationToken);
    }

    private async Task<int?> ResolveProfileUserIdAsync(CancellationToken cancellationToken)
    {
        var identityId = User.GetIdentityId();
        if (identityId == null)
            return null;

        var accessContext = await userAccessContextService.GetByIdentityIdAsync(identityId.Value, cancellationToken);
        return accessContext?.UserId;
    }
}
