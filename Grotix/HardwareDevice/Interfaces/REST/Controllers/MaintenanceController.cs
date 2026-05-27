using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.HardwareDevice.Application.ACL;
using GrotixBackend.HardwareDevice.Application.Internal;
using GrotixBackend.HardwareDevice.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.HardwareDevice.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/hardware")]
[Authorize]
public sealed class MaintenanceController(
    IUserAccessContextService userAccessContextService,
    IZoneAccessService zoneAccessService,
    IDeviceQueryService deviceQueryService,
    IMaintenanceService maintenanceService,
    IActionQueueRepository actionQueueRepository) : ControllerBase
{
    public sealed record CreateMaintenanceLogRequest(string Action, string StatusAfter);

    public sealed record CreateTechnicalMaintenanceRequest(
        int StaffId,
        string Type,
        string Description,
        string? Results);

    [HttpPost("devices/{deviceId:int}/maintenance-logs")]
    public async Task<IActionResult> CreateMaintenanceLog(
        int deviceId,
        [FromBody] CreateMaintenanceLogRequest request,
        CancellationToken cancellationToken)
    {
        if (!CanWrite()) return Forbid();

        var detail = await deviceQueryService.GetDetailAsync(deviceId);
        if (detail == null) return NotFound();
        if (!await CanAccessDeviceAsync(detail.Device, cancellationToken)) return Forbid();

        var profileId = await ResolveProfileUserIdAsync(cancellationToken);
        if (profileId == null) return Unauthorized();

        try
        {
            var log = await maintenanceService.RecordMaintenanceLogAsync(
                deviceId,
                profileId.Value,
                request.Action,
                request.StatusAfter,
                cancellationToken);

            return Created(string.Empty, new
            {
                logId = log.Id,
                deviceId = log.DeviceId,
                userId = log.UserId,
                action = log.Action,
                statusAfter = log.StatusAfter,
                timestamp = log.Timestamp
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("devices/{deviceId:int}/maintenance-logs")]
    public async Task<IActionResult> ListMaintenanceLogs(
        int deviceId,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        if (!CanRead()) return Forbid();

        var detail = await deviceQueryService.GetDetailAsync(deviceId);
        if (detail == null) return NotFound();
        if (!await CanAccessDeviceAsync(detail.Device, cancellationToken)) return Forbid();

        var logs = await maintenanceService.ListMaintenanceLogsAsync(deviceId, Math.Clamp(limit, 1, 200), cancellationToken);
        return Ok(logs.Select(l => new
        {
            logId = l.Id,
            deviceId = l.DeviceId,
            userId = l.UserId,
            action = l.Action,
            statusAfter = l.StatusAfter,
            timestamp = l.Timestamp
        }));
    }

    [HttpPost("devices/{deviceId:int}/technical-maintenance")]
    public async Task<IActionResult> CreateTechnicalMaintenance(
        int deviceId,
        [FromBody] CreateTechnicalMaintenanceRequest request,
        CancellationToken cancellationToken)
    {
        if (!CanWrite()) return Forbid();

        var detail = await deviceQueryService.GetDetailAsync(deviceId);
        if (detail == null) return NotFound();
        if (!await CanAccessDeviceAsync(detail.Device, cancellationToken)) return Forbid();

        try
        {
            var record = await maintenanceService.RecordTechnicalMaintenanceAsync(
                request.StaffId,
                deviceId,
                request.Type,
                request.Description,
                request.Results,
                cancellationToken);

            return Created(string.Empty, new
            {
                maintenanceId = record.Id,
                staffId = record.StaffId,
                deviceId = record.DeviceId,
                type = record.Type,
                description = record.Description,
                date = record.Date,
                results = record.Results
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("devices/{deviceId:int}/technical-maintenance")]
    public async Task<IActionResult> ListTechnicalMaintenance(
        int deviceId,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        if (!CanRead()) return Forbid();

        var detail = await deviceQueryService.GetDetailAsync(deviceId);
        if (detail == null) return NotFound();
        if (!await CanAccessDeviceAsync(detail.Device, cancellationToken)) return Forbid();

        var records = await maintenanceService.ListTechnicalMaintenanceByDeviceAsync(
            deviceId,
            Math.Clamp(limit, 1, 200),
            cancellationToken);

        return Ok(records.Select(r => new
        {
            maintenanceId = r.Id,
            staffId = r.StaffId,
            deviceId = r.DeviceId,
            type = r.Type,
            description = r.Description,
            date = r.Date,
            results = r.Results
        }));
    }

    [HttpGet("actuators/{actuatorId:int}/action-queue")]
    public async Task<IActionResult> ListActionQueue(
        int actuatorId,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        if (!CanRead()) return Forbid();

        var items = await actionQueueRepository.ListByActuatorAsync(
            actuatorId,
            Math.Clamp(limit, 1, 200),
            cancellationToken);

        return Ok(items.Select(a => new
        {
            actionId = a.Id,
            actuatorId = a.ActuatorId,
            command = a.Command,
            status = a.Status,
            createdAt = a.CreatedAt
        }));
    }

    private bool CanRead() =>
        User.IsInRole("admin") ||
        User.HasPermission(KnownPermissionCodes.DeviceConfig) ||
        User.HasPermission(KnownPermissionCodes.TelemetryView);

    private bool CanWrite() =>
        User.IsInRole("admin") || User.HasPermission(KnownPermissionCodes.DeviceConfig);

    private async Task<bool> CanAccessDeviceAsync(
        Domain.Model.Aggregates.Microcontroller device,
        CancellationToken cancellationToken)
    {
        if (device.ZoneId == null)
            return User.IsInRole("admin");

        var profileId = await ResolveProfileUserIdAsync(cancellationToken);
        return await zoneAccessService.CanAccessZoneAsync(
            device.ZoneId.Value,
            User.IsInRole("admin"),
            profileId);
    }

    private async Task<int?> ResolveProfileUserIdAsync(CancellationToken cancellationToken)
    {
        var identityId = User.GetIdentityId();
        if (identityId == null) return null;
        var ctx = await userAccessContextService.GetByIdentityIdAsync(identityId.Value, cancellationToken);
        return ctx?.UserId;
    }
}
