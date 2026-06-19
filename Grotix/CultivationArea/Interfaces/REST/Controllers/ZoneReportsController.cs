using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Application.Internal.ZoneReports;
using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.CultivationArea.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/zones")]
[Authorize]
public sealed class ZoneReportsController(
    IUserAccessContextService userAccessContextService,
    IFarmQueryService farmQueryService,
    IZoneQueryService zoneQueryService,
    IZoneMemberService zoneMemberService,
    IZoneReportService zoneReportService) : ControllerBase
{
    /// <summary>Resumen JSON del informe (vista previa).</summary>
    [HttpGet("{zoneId:int}/reports/summary")]
    public async Task<IActionResult> GetSummary(
        int zoneId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        if (!CanReadReports()) return Forbid();

        var zone = await zoneQueryService.Handle(new GetZoneByIdQuery(zoneId));
        if (zone == null) return NotFound();
        if (!await CanAccessZoneAsync(zone, cancellationToken)) return Forbid();

        var data = await zoneReportService.BuildAsync(zoneId, from, to, cancellationToken);
        if (data == null) return NotFound();

        return Ok(ToResponse(data));
    }

    /// <summary>Exporta informe de zona en PDF.</summary>
    [HttpGet("{zoneId:int}/reports/export")]
    public async Task<IActionResult> ExportPdf(
        int zoneId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken = default)
    {
        if (!CanReadReports()) return Forbid();

        var zone = await zoneQueryService.Handle(new GetZoneByIdQuery(zoneId));
        if (zone == null) return NotFound();
        if (!await CanAccessZoneAsync(zone, cancellationToken)) return Forbid();

        try
        {
            var pdf = await zoneReportService.BuildPdfAsync(zoneId, from, to, cancellationToken);
            var fileName = $"grotix-zona-{zoneId}-{DateTime.UtcNow:yyyyMMdd}.pdf";
            return File(pdf, "application/pdf", fileName);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private static object ToResponse(ZoneReportData data) => new
    {
        generatedAt = data.GeneratedAtUtc,
        period = new { from = data.PeriodStartUtc, to = data.PeriodEndUtc },
        zone = new
        {
            id = data.Zone.Id,
            name = data.Zone.Name,
            cropName = data.Zone.CropName,
            irrigationMode = data.Zone.IrrigationMode,
            currentPhase = data.Zone.CurrentPhase,
            latitude = data.Zone.Latitude,
            longitude = data.Zone.Longitude
        },
        farm = new
        {
            id = data.Farm.Id,
            name = data.Farm.Name,
            location = data.Farm.Location,
            associationId = data.Farm.AssociationId,
            associationName = data.Farm.AssociationName
        },
        devices = data.Devices.Select(d => new
        {
            deviceId = d.DeviceId,
            d.Model,
            macAddress = d.MacAddress,
            d.Status,
            d.SensorCount,
            d.ActuatorCount,
            d.LastSeen
        }),
        telemetry = new
        {
            readingsCount = data.Telemetry.ReadingsCount,
            avgTemperature = data.Telemetry.AvgTemperature,
            avgHumidityAir = data.Telemetry.AvgHumidityAir,
            avgHumiditySoil = data.Telemetry.AvgHumiditySoil,
            avgLightIntensity = data.Telemetry.AvgLightIntensity
        },
        irrigation = new
        {
            cyclesCount = data.Irrigation.CyclesCount,
            totalVolumeLiters = data.Irrigation.TotalVolumeLiters,
            totalDurationMinutes = data.Irrigation.TotalDurationMinutes
        },
        analysisReports = data.AnalysisReports.Select(r => new
        {
            r.DetectedPhase,
            r.HealthScore,
            r.CreatedAt
        }),
        alerts = data.Alerts.Select(a => new
        {
            alertType = a.AlertType,
            a.Message,
            a.CreatedAt
        })
    };

    private bool CanReadReports() =>
        User.IsInRole("admin") ||
        User.IsInRole("staff") ||
        User.IsInRole("user_admin") ||
        User.HasPermission("TELEMETRY_VIEW");

    private async Task<bool> CanAccessZoneAsync(Zone zone, CancellationToken cancellationToken)
    {
        if (User.IsInRole("admin") || User.IsInRole("staff")) return true;

        var accessContext = await ResolveAccessContextAsync(cancellationToken);
        if (accessContext == null) return false;

        var farm = await farmQueryService.Handle(new GetFarmByIdQuery(zone.FarmId));
        if (farm == null) return false;

        var isOrgAdmin = User.IsInRole("user_admin") &&
                         accessContext.AssociationId == farm.AssociationId;

        if (isOrgAdmin)
            return true;

        return await zoneMemberService.CanUserAccessZoneAsync(
            zone.Id,
            accessContext.UserId,
            isOrgAdmin: false);
    }

    private async Task<UserAccessContext?> ResolveAccessContextAsync(CancellationToken cancellationToken)
    {
        var identityId = User.GetIdentityId();
        if (identityId == null) return null;
        return await userAccessContextService.GetByIdentityIdAsync(identityId.Value, cancellationToken);
    }
}
