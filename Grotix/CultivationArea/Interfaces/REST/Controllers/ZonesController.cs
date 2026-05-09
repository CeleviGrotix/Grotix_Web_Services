using GrotixBackend.CultivationArea.Application.Internal.CommandServices;
using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Commands;
using GrotixBackend.CultivationArea.Domain.Model.Queries;
using GrotixBackend.CultivationArea.Interfaces.REST.Transform;
using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Model.Queries;
using GrotixBackend.Profiles.Interfaces.REST.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.CultivationArea.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/zones")]
[Authorize]
public class ZonesController(
    IUserQueryService userQueryService,
    IFarmQueryService farmQueryService,
    IZoneCommandService zoneCommandService,
    IZoneQueryService zoneQueryService) : ControllerBase
{
    [HttpGet("{zoneId:int}")]
    public async Task<IActionResult> GetById(int zoneId)
    {
        var zone = await zoneQueryService.Handle(new GetZoneByIdQuery(zoneId));
        if (zone == null) return NotFound();
        if (!await CanAccessZoneAsync(zone)) return Forbid();
        return Ok(CultivationAreaResourceAssembler.ToZoneResource(zone));
    }

    public record PatchZoneRequest(
        int? CropId,
        double? Latitude,
        double? Longitude,
        string? CurrentPhase,
        DateTime? PhaseStartDate,
        string? ImageUrl);

    [HttpPatch("{zoneId:int}")]
    public async Task<IActionResult> Patch(int zoneId, [FromBody] PatchZoneRequest request)
    {
        var zone = await zoneQueryService.Handle(new GetZoneByIdQuery(zoneId));
        if (zone == null) return NotFound();
        if (!await CanAccessZoneAsync(zone)) return Forbid();
        try
        {
            var updated = await zoneCommandService.Handle(new UpdateZoneCommand(
                zoneId,
                request.CropId,
                request.Latitude,
                request.Longitude,
                request.CurrentPhase,
                request.PhaseStartDate,
                request.ImageUrl));
            return Ok(CultivationAreaResourceAssembler.ToZoneResource(updated));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private async Task<int?> ResolveProfileUserIdAsync()
    {
        var identityId = User.GetIdentityId();
        if (identityId == null) return null;
        var profile = await userQueryService.Handle(new GetUserByIdentityQuery(identityId.Value));
        return profile?.Id;
    }

    private async Task<bool> CanAccessZoneAsync(Zone zone)
    {
        if (User.IsInRole("admin")) return true;
        var farm = await farmQueryService.Handle(new GetFarmByIdQuery(zone.FarmId));
        if (farm == null) return false;
        var profileId = await ResolveProfileUserIdAsync();
        return profileId.HasValue && farm.UserId == profileId.Value;
    }
}
