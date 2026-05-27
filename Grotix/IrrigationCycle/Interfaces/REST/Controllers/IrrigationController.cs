using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.IrrigationCycle.Application.ACL;
using GrotixBackend.IrrigationCycle.Application.Internal;
using GrotixBackend.IrrigationCycle.Domain.Model.ValueObjects;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.IrrigationCycle.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/irrigation")]
[Authorize]
public sealed class IrrigationController(
    IUserAccessContextService userAccessContextService,
    IZoneAccessService zoneAccessService,
    IIrrigationCommandService commandService,
    IIrrigationQueryService queryService,
    IIrrigationScheduleService scheduleService) : ControllerBase
{
    public sealed record StartIrrigationRequest(double? VolumeLiters, int? DurationMinutes);

    public sealed record StopIrrigationRequest(string? Reason);

    [HttpPost("start/{zoneId:int}")]
    public async Task<IActionResult> Start(
        int zoneId,
        [FromBody] StartIrrigationRequest? request,
        CancellationToken cancellationToken)
    {
        if (!CanExecute()) return Forbid();
        if (!await CanAccessZoneAsync(zoneId, cancellationToken)) return Forbid();

        try
        {
            var cycle = await commandService.StartManualAsync(
                zoneId,
                request?.VolumeLiters,
                request?.DurationMinutes,
                cancellationToken);
            return Ok(new { cycleId = cycle.Id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("stop/{zoneId:int}")]
    public async Task<IActionResult> Stop(
        int zoneId,
        [FromBody] StopIrrigationRequest? request,
        CancellationToken cancellationToken)
    {
        if (!CanExecute()) return Forbid();
        if (!await CanAccessZoneAsync(zoneId, cancellationToken)) return Forbid();

        try
        {
            var cycle = await commandService.AbortActiveByZoneAsync(
                zoneId,
                request?.Reason,
                cancellationToken);
            return Ok(ToCycleDto(cycle));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("schedules")]
    public async Task<IActionResult> ListSchedules([FromQuery] int? zoneId, CancellationToken cancellationToken)
    {
        if (!CanRead()) return Forbid();
        if (zoneId.HasValue && !await CanAccessZoneAsync(zoneId.Value, cancellationToken)) return Forbid();

        var schedules = await scheduleService.ListAsync(zoneId);
        return Ok(schedules.Select(s => new
        {
            id = s.Id,
            zoneId = s.ZoneId,
            daysOfTheWeek = s.DaysOfTheWeek,
            startTime = s.StartTime.ToString("HH:mm"),
            durationMinutes = s.DurationMinutes,
            isActive = s.IsActive,
            createdAt = s.CreatedAt
        }));
    }

    public sealed record CreateScheduleBody(
        int ZoneId,
        string DaysOfTheWeek,
        string StartTime,
        int DurationMinutes);

    [HttpPost("schedules")]
    public async Task<IActionResult> CreateSchedule(
        [FromBody] CreateScheduleBody body,
        CancellationToken cancellationToken)
    {
        if (!CanExecute()) return Forbid();
        if (!await CanAccessZoneAsync(body.ZoneId, cancellationToken)) return Forbid();

        if (!TimeOnly.TryParse(body.StartTime, out var startTime))
            return BadRequest(new { message = "StartTime inválido (use HH:mm)." });

        try
        {
            var schedule = await scheduleService.CreateAsync(
                new CreateScheduleRequest(body.ZoneId, body.DaysOfTheWeek, startTime, body.DurationMinutes),
                cancellationToken);
            return Ok(new { scheduleId = schedule.Id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    public sealed record PatchScheduleBody(
        string? DaysOfTheWeek,
        string? StartTime,
        int? DurationMinutes,
        bool? IsActive);

    [HttpPatch("schedules/{id:int}")]
    public async Task<IActionResult> PatchSchedule(
        int id,
        [FromBody] PatchScheduleBody body,
        CancellationToken cancellationToken)
    {
        if (!CanExecute()) return Forbid();

        TimeOnly? startTime = null;
        if (!string.IsNullOrWhiteSpace(body.StartTime))
        {
            if (!TimeOnly.TryParse(body.StartTime, out var parsed))
                return BadRequest(new { message = "StartTime inválido." });
            startTime = parsed;
        }

        try
        {
            await scheduleService.UpdateAsync(
                id,
                new UpdateScheduleRequest(body.DaysOfTheWeek, startTime, body.DurationMinutes, body.IsActive),
                cancellationToken);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("schedules/{id:int}")]
    public async Task<IActionResult> DeleteSchedule(int id, CancellationToken cancellationToken)
    {
        if (!CanExecute()) return Forbid();

        try
        {
            await scheduleService.DeleteAsync(id, cancellationToken);
            return Ok(new { success = true });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("history")]
    public async Task<IActionResult> History(
        [FromQuery] int? zoneId,
        [FromQuery] DateTime? startTime,
        [FromQuery] DateTime? endTime,
        [FromQuery] int? limit,
        CancellationToken cancellationToken)
    {
        if (!CanRead()) return Forbid();
        if (zoneId.HasValue && !await CanAccessZoneAsync(zoneId.Value, cancellationToken)) return Forbid();

        var cycles = await queryService.GetHistoryAsync(zoneId, startTime, endTime, limit);
        return Ok(cycles.Select(ToCycleDto));
    }

    [HttpGet("history/{zoneId:int}")]
    public Task<IActionResult> HistoryForZone(
        int zoneId,
        [FromQuery] DateTime? startTime,
        [FromQuery] DateTime? endTime,
        [FromQuery] int? limit,
        CancellationToken cancellationToken) =>
        History(zoneId, startTime, endTime, limit, cancellationToken);

    [HttpGet("active")]
    public async Task<IActionResult> Active([FromQuery] int? zoneId, CancellationToken cancellationToken)
    {
        if (!CanRead()) return Forbid();
        if (zoneId.HasValue && !await CanAccessZoneAsync(zoneId.Value, cancellationToken)) return Forbid();

        var cycles = await queryService.GetActiveAsync(zoneId);
        return Ok(cycles.Select(ToCycleDto));
    }

    [HttpGet("active/{zoneId:int}")]
    public Task<IActionResult> ActiveForZone(int zoneId, CancellationToken cancellationToken) =>
        Active(zoneId, cancellationToken);

    private static object ToCycleDto(Domain.Model.Aggregates.IrrigationCycleRecord c) => new
    {
        id = c.Id,
        zoneId = c.ZoneId,
        startTime = c.StartTime,
        endTime = c.EndTime,
        volumeLiters = c.VolumeLiters,
        status = CycleStatuses.ToApiStatus(c.Status),
        abortReason = c.AbortReason
    };

    private bool CanRead() =>
        User.IsInRole("admin") ||
        User.HasPermission(KnownPermissionCodes.TelemetryView) ||
        User.HasPermission(KnownPermissionCodes.ManualControlExecute);

    private bool CanExecute() =>
        User.IsInRole("admin") || User.HasPermission(KnownPermissionCodes.ManualControlExecute);

    private async Task<bool> CanAccessZoneAsync(int zoneId, CancellationToken cancellationToken)
    {
        var profileId = await ResolveProfileUserIdAsync(cancellationToken);
        return await zoneAccessService.CanAccessZoneAsync(zoneId, User.IsInRole("admin"), profileId);
    }

    private async Task<int?> ResolveProfileUserIdAsync(CancellationToken cancellationToken)
    {
        var identityId = User.GetIdentityId();
        if (identityId == null) return null;
        var ctx = await userAccessContextService.GetByIdentityIdAsync(identityId.Value, cancellationToken);
        return ctx?.UserId;
    }
}
