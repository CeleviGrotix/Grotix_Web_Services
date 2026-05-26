using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.CultivationArea.Application.Internal.CommandServices;
using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Commands;
using GrotixBackend.CultivationArea.Domain.Model.Queries;
using GrotixBackend.CultivationArea.Interfaces.REST.Transform;
using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Model.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.CultivationArea.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/farms")]
[Authorize]
public class FarmsController(
    IUserQueryService userQueryService,
    IFarmCommandService farmCommandService,
    IFarmQueryService farmQueryService,
    IZoneCommandService zoneCommandService,
    IZoneQueryService zoneQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListMine()
    {
        var profileId = await ResolveProfileUserIdAsync();
        if (profileId == null) return Unauthorized();
        var farms = await farmQueryService.Handle(new ListFarmsForUserQuery(profileId.Value));
        return Ok(farms.Select(CultivationAreaResourceAssembler.ToFarmResource).ToList());
    }

    public record CreateFarmRequest(string Name, string Location);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFarmRequest request)
    {
        var profileId = await ResolveProfileUserIdAsync();
        if (profileId == null) return Unauthorized();
        var farm = await farmCommandService.Handle(new CreateFarmCommand(profileId.Value, request.Name, request.Location));
        return CreatedAtAction(nameof(GetById), new { farmId = farm.Id }, CultivationAreaResourceAssembler.ToFarmResource(farm));
    }

    [HttpGet("{farmId:int}")]
    public async Task<IActionResult> GetById(int farmId)
    {
        var farm = await farmQueryService.Handle(new GetFarmByIdQuery(farmId));
        if (farm == null) return NotFound();
        if (!await CanAccessFarmAsync(farm)) return Forbid();
        return Ok(CultivationAreaResourceAssembler.ToFarmResource(farm));
    }

    public record PatchFarmRequest(string Name, string Location);

    [HttpPatch("{farmId:int}")]
    public async Task<IActionResult> Patch(int farmId, [FromBody] PatchFarmRequest request)
    {
        var farm = await farmQueryService.Handle(new GetFarmByIdQuery(farmId));
        if (farm == null) return NotFound();
        if (!await CanAccessFarmAsync(farm)) return Forbid();
        var updated = await farmCommandService.Handle(new UpdateFarmCommand(farmId, request.Name, request.Location));
        return Ok(CultivationAreaResourceAssembler.ToFarmResource(updated));
    }

    [HttpGet("{farmId:int}/zones")]
    public async Task<IActionResult> ListZones(int farmId)
    {
        var farm = await farmQueryService.Handle(new GetFarmByIdQuery(farmId));
        if (farm == null) return NotFound();
        if (!await CanAccessFarmAsync(farm)) return Forbid();
        var zones = await zoneQueryService.Handle(new ListZonesForFarmQuery(farmId));
        return Ok(zones.Select(CultivationAreaResourceAssembler.ToZoneResource).ToList());
    }

    public record CreateZoneRequest(
        int CropId,
        double Latitude,
        double Longitude,
        string? CurrentPhase,
        DateTime? PhaseStartDate,
        string? ImageUrl);

    [HttpPost("{farmId:int}/zones")]
    public async Task<IActionResult> CreateZone(int farmId, [FromBody] CreateZoneRequest request)
    {
        var farm = await farmQueryService.Handle(new GetFarmByIdQuery(farmId));
        if (farm == null) return NotFound();
        if (!await CanAccessFarmAsync(farm)) return Forbid();
        try
        {
            var zone = await zoneCommandService.Handle(new CreateZoneCommand(
                farmId,
                request.CropId,
                request.Latitude,
                request.Longitude,
                request.CurrentPhase,
                request.PhaseStartDate,
                request.ImageUrl));
            return Created($"/api/v1/zones/{zone.Id}", CultivationAreaResourceAssembler.ToZoneResource(zone));
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

    private async Task<bool> CanAccessFarmAsync(Farm farm)
    {
        if (User.IsInRole("admin")) return true;
        var profileId = await ResolveProfileUserIdAsync();
        return profileId.HasValue && farm.UserId == profileId.Value;
    }
}
