using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.HardwareDevice.Application.ACL;
using GrotixBackend.HardwareDevice.Application.Internal;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.HardwareDevice.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/hardware")]
[Authorize]
public sealed class DevicesController(
    IUserAccessContextService userAccessContextService,
    IZoneAccessService zoneAccessService,
    IDeviceQueryService deviceQueryService,
    IDeviceCommandService deviceCommandService,
    IDeviceDiagnosticService deviceDiagnosticService) : ControllerBase
{
    [HttpGet("devices")]
    public async Task<IActionResult> List(
        [FromQuery] string? status,
        [FromQuery] int? zoneId,
        CancellationToken cancellationToken)
    {
        if (!CanRead()) return Forbid();
        if (zoneId.HasValue && !await CanAccessZoneAsync(zoneId.Value, cancellationToken))
            return Forbid();

        var devices = await deviceQueryService.ListAsync(status, zoneId);
        return Ok(devices.Select(d => new
        {
            id = d.Id,
            zoneId = d.ZoneId,
            model = d.Model,
            macAddress = d.MacAddress,
            status = d.Status,
            lastSeen = d.LastSeen,
            createdAt = d.CreatedAt
        }));
    }

    public sealed record CreateDeviceRequest(
        int? ZoneId,
        string Model,
        string MacAddress,
        CreateSensorRequest[]? Sensors);

    public sealed record CreateSensorRequest(
        string Type,
        string Unit,
        int Pin,
        double? MinPhysical,
        double? MaxPhysical);

    [HttpPost("devices")]
    public async Task<IActionResult> Create([FromBody] CreateDeviceRequest request, CancellationToken cancellationToken)
    {
        if (!CanWrite()) return Forbid();
        if (request.ZoneId.HasValue && !await CanAccessZoneAsync(request.ZoneId.Value, cancellationToken))
            return Forbid();

        try
        {
            IReadOnlyList<RegisterSensorRequest>? sensors = request.Sensors?
                .Select(s => new RegisterSensorRequest(s.Type, s.Unit, s.Pin, s.MinPhysical, s.MaxPhysical))
                .ToList();

            var device = await deviceCommandService.RegisterAsync(
                new RegisterDeviceRequest(request.ZoneId, request.Model, request.MacAddress, sensors),
                cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = device.Id }, new { deviceId = device.Id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("devices/{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        if (!CanRead()) return Forbid();

        var detail = await deviceQueryService.GetDetailAsync(id);
        if (detail == null) return NotFound();
        if (!await CanAccessDeviceAsync(detail.Device, cancellationToken)) return Forbid();

        return Ok(new
        {
            deviceId = detail.Device.Id,
            zoneId = detail.Device.ZoneId,
            model = detail.Device.Model,
            macAddress = detail.Device.MacAddress,
            status = detail.Device.Status,
            lastSeen = detail.Device.LastSeen,
            sensors = detail.Sensors.Select(s => new
            {
                sensorId = s.Id,
                type = s.Type,
                unit = s.Unit,
                pin = s.Pin,
                status = s.Status
            }),
            actuators = detail.Actuators.Select(a => new
            {
                actuatorId = a.Id,
                type = a.Type,
                pin = a.Pin,
                status = a.Status,
                lastSeen = a.LastSeen
            })
        });
    }

    public sealed record PatchDeviceRequest(int? ZoneId, string? Model, string? MacAddress);

    [HttpPatch("devices/{id:int}")]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchDeviceRequest request, CancellationToken cancellationToken)
    {
        if (!CanWrite()) return Forbid();

        var device = await deviceQueryService.GetDetailAsync(id);
        if (device == null) return NotFound();
        if (!await CanAccessDeviceAsync(device.Device, cancellationToken)) return Forbid();
        if (request.ZoneId.HasValue && !await CanAccessZoneAsync(request.ZoneId.Value, cancellationToken))
            return Forbid();

        try
        {
            await deviceCommandService.UpdateAsync(
                id,
                new UpdateDeviceRequest(request.ZoneId, request.Model, request.MacAddress),
                cancellationToken);
            return Ok(new { success = true });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("devices/{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        if (!CanWrite()) return Forbid();

        var device = await deviceQueryService.GetDetailAsync(id);
        if (device == null) return NotFound();
        if (!await CanAccessDeviceAsync(device.Device, cancellationToken)) return Forbid();

        try
        {
            await deviceCommandService.DeleteAsync(id, cancellationToken);
            return Ok(new { success = true });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("devices/{id:int}/link-to-zone/{zoneId:int}")]
    public async Task<IActionResult> LinkToZone(int id, int zoneId, CancellationToken cancellationToken)
    {
        if (!CanWrite()) return Forbid();
        if (!await CanAccessZoneAsync(zoneId, cancellationToken)) return Forbid();

        try
        {
            await deviceCommandService.LinkToZoneAsync(id, zoneId, cancellationToken);
            return Ok(new { success = true });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("devices/{id:int}/unlink-from-zone/{zoneId:int}")]
    public async Task<IActionResult> UnlinkFromZone(int id, int zoneId, CancellationToken cancellationToken)
    {
        if (!CanWrite()) return Forbid();

        try
        {
            await deviceCommandService.UnlinkFromZoneAsync(id, zoneId, cancellationToken);
            return Ok(new { success = true });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("devices/{id:int}/zone")]
    public async Task<IActionResult> GetZone(int id, CancellationToken cancellationToken)
    {
        if (!CanRead()) return Forbid();

        var link = await deviceQueryService.GetZoneLinkAsync(id);
        if (link == null) return NotFound();
        return Ok(new { zoneId = link.ZoneId, zoneName = link.ZoneName, cropName = link.CropName });
    }

    [HttpGet("zones/{zoneId:int}/devices")]
    public async Task<IActionResult> ListByZone(int zoneId, CancellationToken cancellationToken)
    {
        if (!CanRead()) return Forbid();
        if (!await CanAccessZoneAsync(zoneId, cancellationToken)) return Forbid();

        var devices = await deviceQueryService.ListByZoneAsync(zoneId);
        return Ok(devices.Select(d => new
        {
            deviceId = d.DeviceId,
            model = d.Model,
            macAddress = d.MacAddress,
            status = d.Status,
            lastSeen = d.LastSeen,
            sensorCount = d.SensorCount,
            actuatorCount = d.ActuatorCount
        }));
    }

    [HttpGet("devices/{id:int}/telemetry")]
    public async Task<IActionResult> GetTelemetry(
        int id,
        [FromQuery] string[]? sensorTypes,
        CancellationToken cancellationToken)
    {
        if (!CanRead()) return Forbid();

        var snapshot = await deviceQueryService.GetTelemetryAsync(id, sensorTypes);
        if (snapshot == null) return NotFound();

        return Ok(new
        {
            deviceId = snapshot.DeviceId,
            timestamp = snapshot.Timestamp,
            readings = snapshot.Readings.Select(r => new
            {
                sensorId = r.SensorId,
                type = r.Type,
                value = r.Value,
                unit = r.Unit
            }),
            batteryLevel = snapshot.BatteryLevel,
            signalStrength = snapshot.SignalStrength
        });
    }

    [HttpGet("devices/{id:int}/status")]
    public async Task<IActionResult> GetStatus(int id, CancellationToken cancellationToken)
    {
        if (!CanRead()) return Forbid();

        var status = await deviceQueryService.GetStatusAsync(id);
        if (status == null) return NotFound();

        return Ok(new
        {
            deviceId = status.DeviceId,
            status = status.Status,
            lastSeen = status.LastSeen,
            uptimeSeconds = status.UptimeSeconds
        });
    }

    public sealed record PatchDeviceStatusRequest(string Status, DateTime? LastSeen);

    [HttpPatch("devices/{id:int}/status")]
    public async Task<IActionResult> PatchStatus(int id, [FromBody] PatchDeviceStatusRequest request, CancellationToken cancellationToken)
    {
        if (!CanWrite()) return Forbid();

        try
        {
            await deviceCommandService.UpdateStatusAsync(
                id,
                new UpdateDeviceStatusRequest(request.Status, request.LastSeen),
                cancellationToken);
            return Ok(new { success = true });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    public sealed record DiagnosticRequest(bool? IncludeSensors, bool? IncludeActuators);

    [HttpGet("devices/{id:int}/diagnostic")]
    public async Task<IActionResult> Diagnostic(
        int id,
        [FromQuery] bool includeSensors = true,
        [FromQuery] bool includeActuators = true,
        CancellationToken cancellationToken = default)
    {
        if (!User.HasPermission(KnownPermissionCodes.HardwareDiagnostic) && !User.IsInRole("admin"))
            return Forbid();

        var report = await deviceDiagnosticService.RunAsync(id, includeSensors, includeActuators);
        if (report == null) return NotFound();

        return Ok(new
        {
            deviceId = report.DeviceId,
            timestamp = report.Timestamp,
            overallStatus = report.OverallStatus,
            checks = report.Checks
        });
    }

    private bool CanRead() =>
        User.IsInRole("admin") ||
        User.HasPermission(KnownPermissionCodes.DeviceConfig) ||
        User.HasPermission(KnownPermissionCodes.TelemetryView);

    private bool CanWrite() =>
        User.IsInRole("admin") || User.HasPermission(KnownPermissionCodes.DeviceConfig);

    private async Task<bool> CanAccessZoneAsync(int zoneId, CancellationToken cancellationToken)
    {
        var profileId = await ResolveProfileUserIdAsync(cancellationToken);
        return await zoneAccessService.CanAccessZoneAsync(zoneId, User.IsInRole("admin"), profileId);
    }

    private async Task<bool> CanAccessDeviceAsync(
        Domain.Model.Aggregates.Microcontroller device,
        CancellationToken cancellationToken)
    {
        if (device.ZoneId == null)
            return User.IsInRole("admin");

        return await CanAccessZoneAsync(device.ZoneId.Value, cancellationToken);
    }

    private async Task<int?> ResolveProfileUserIdAsync(CancellationToken cancellationToken)
    {
        var identityId = User.GetIdentityId();
        if (identityId == null) return null;
        var ctx = await userAccessContextService.GetByIdentityIdAsync(identityId.Value, cancellationToken);
        return ctx?.UserId;
    }
}
