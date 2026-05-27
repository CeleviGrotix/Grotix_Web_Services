using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.CultivationArea.Application.Internal.CommandServices;
using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Commands;
using GrotixBackend.CultivationArea.Domain.Model.Queries;
using GrotixBackend.CultivationArea.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.CultivationArea.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/farms")]
[Authorize]
public class FarmsController(
    IUserAccessContextService userAccessContextService,
    IFarmCommandService farmCommandService,
    IFarmQueryService farmQueryService,
    IAssociationFarmOwnerSyncService associationFarmOwnerSyncService,
    IZoneCommandService zoneCommandService,
    IZoneQueryService zoneQueryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> ListMine()
    {
        var accessContext = await ResolveAccessContextAsync();
        if (accessContext == null) return Unauthorized();

        IReadOnlyList<Farm> farms;
        if (User.IsInRole("admin") || User.IsInRole("staff"))
        {
            farms = await farmQueryService.Handle(new ListAllFarmsQuery());
        }
        else if (accessContext.AssociationId is { } associationId)
        {
            farms = await farmQueryService.Handle(new ListFarmsForAssociationQuery(associationId));
        }
        else
        {
            farms = [];
        }

        return Ok(farms.Select(CultivationAreaResourceAssembler.ToFarmResource).ToList());
    }

    public record CreateFarmRequest(string Name, string Location, int? AssociationId);

    [HttpPost]
    [Authorize(Roles = "admin,staff,user_admin")]
    public async Task<IActionResult> Create([FromBody] CreateFarmRequest request)
    {
        var accessContext = await ResolveAccessContextAsync();
        if (accessContext == null) return Unauthorized();

        try
        {
            var associationId = ResolveAssociationIdForCreate(accessContext, request.AssociationId);
            if (associationId == null)
                return BadRequest(new { message = "AssociationId es requerido o el usuario no pertenece a esa asociación." });

            await associationFarmOwnerSyncService.SyncUnownedFarmsAsync(associationId.Value);

            var farm = await farmCommandService.Handle(new CreateFarmCommand(
                associationId.Value,
                request.Name,
                request.Location));

            return CreatedAtAction(nameof(GetById), new { farmId = farm.Id }, CultivationAreaResourceAssembler.ToFarmResource(farm));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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

    private async Task<UserAccessContext?> ResolveAccessContextAsync()
    {
        var identityId = User.GetIdentityId();
        if (identityId == null) return null;
        return await userAccessContextService.GetByIdentityIdAsync(identityId.Value);
    }

    private int? ResolveAssociationIdForCreate(UserAccessContext accessContext, int? requestedAssociationId)
    {
        if (User.IsInRole("admin") || User.IsInRole("staff"))
            return requestedAssociationId;

        if (accessContext.AssociationId == null)
            return null;

        if (requestedAssociationId.HasValue && requestedAssociationId.Value != accessContext.AssociationId.Value)
            return null;

        return accessContext.AssociationId.Value;
    }

    private async Task<bool> CanAccessFarmAsync(Farm farm)
    {
        if (User.IsInRole("admin") || User.IsInRole("staff")) return true;

        var accessContext = await ResolveAccessContextAsync();
        if (accessContext == null) return false;

        if (accessContext.AssociationId.HasValue && farm.AssociationId == accessContext.AssociationId.Value)
            return true;

        return farm.UserId.HasValue && accessContext.UserId == farm.UserId.Value;
    }
}
