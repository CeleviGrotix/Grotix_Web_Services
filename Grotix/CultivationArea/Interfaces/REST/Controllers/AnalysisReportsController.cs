using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.CultivationArea.Application.Internal;
using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.CultivationArea.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/zones")]
[Authorize]
public sealed class AnalysisReportsController(
    IUserAccessContextService userAccessContextService,
    IFarmQueryService farmQueryService,
    IZoneQueryService zoneQueryService,
    IZoneMemberService zoneMemberService,
    IAnalysisReportService analysisReportService) : ControllerBase
{
    public sealed record CreateAnalysisReportRequest(string DetectedPhase, float HealthScore);

    [HttpPost("{zoneId:int}/analysis-reports")]
    public async Task<IActionResult> Create(
        int zoneId,
        [FromBody] CreateAnalysisReportRequest request,
        CancellationToken cancellationToken)
    {
        var zone = await zoneQueryService.Handle(new GetZoneByIdQuery(zoneId));
        if (zone == null) return NotFound();
        if (!await CanAccessZoneAsync(zone)) return Forbid();
        if (!CanManageReports()) return Forbid();

        try
        {
            var report = await analysisReportService.CreateAsync(
                zoneId,
                request.DetectedPhase,
                request.HealthScore,
                cancellationToken);

            return Created(string.Empty, ToResource(report));
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

    [HttpGet("{zoneId:int}/analysis-reports")]
    public async Task<IActionResult> List(
        int zoneId,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var zone = await zoneQueryService.Handle(new GetZoneByIdQuery(zoneId));
        if (zone == null) return NotFound();
        if (!await CanAccessZoneAsync(zone)) return Forbid();
        var reports = await analysisReportService.ListByZoneAsync(
            zoneId,
            Math.Clamp(limit, 1, 200),
            cancellationToken);

        return Ok(reports.Select(ToResource));
    }

    private static object ToResource(AnalysisReport report) => new
    {
        reportId = report.Id,
        zoneId = report.ZoneId,
        detectedPhase = report.DetectedPhase,
        healthScore = report.HealthScore,
        createdAt = report.CreatedAt
    };

    private bool CanManageReports() =>
        User.IsInRole("admin") || User.IsInRole("staff") || User.IsInRole("user_admin");

    private async Task<bool> CanAccessZoneAsync(Zone zone)
    {
        if (User.IsInRole("admin") || User.IsInRole("staff")) return true;

        var accessContext = await ResolveAccessContextAsync();
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

    private async Task<UserAccessContext?> ResolveAccessContextAsync()
    {
        var identityId = User.GetIdentityId();
        if (identityId == null) return null;
        return await userAccessContextService.GetByIdentityIdAsync(identityId.Value);
    }
}
