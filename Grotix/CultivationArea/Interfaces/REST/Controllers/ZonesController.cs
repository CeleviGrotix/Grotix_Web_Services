using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.CultivationArea.Application.Internal.CommandServices;
using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Commands;
using GrotixBackend.CultivationArea.Domain.Model.Queries;
using GrotixBackend.CultivationArea.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.CultivationArea.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/zones")]
[Authorize]
public class ZonesController(
    IUserAccessContextService userAccessContextService,
    IFarmQueryService farmQueryService,
    IZoneCommandService zoneCommandService,
    IZoneQueryService zoneQueryService,
    IZoneMemberService zoneMemberService) : ControllerBase
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
        string? Name,
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
                request.Name,
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

    public sealed record AssignZoneMemberRequest(int UserId);

    /// <summary>Lista personal asignado a la zona (permiso de visibilidad).</summary>
    [HttpGet("{zoneId:int}/members")]
    public async Task<IActionResult> ListMembers(int zoneId, CancellationToken cancellationToken = default)
    {
        var zone = await zoneQueryService.Handle(new GetZoneByIdQuery(zoneId));
        if (zone == null) return NotFound();
        if (!await CanManageZoneMembersAsync(zone))
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "No tienes permiso para gestionar miembros de esta zona." });

        var members = await zoneMemberService.ListAsync(zoneId, cancellationToken);
        return Ok(members.Select(m => new
        {
            userId = m.UserId,
            name = m.Name,
            email = m.Email,
            roleId = m.RoleId,
            roleName = m.RoleName,
            assignedAt = m.AssignedAt,
            assignedByUserId = m.AssignedByUserId
        }));
    }

    /// <summary>Asigna un miembro existente de la organización a la zona.</summary>
    [HttpPost("{zoneId:int}/members")]
    public async Task<IActionResult> AssignMember(
        int zoneId,
        [FromBody] AssignZoneMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        var zone = await zoneQueryService.Handle(new GetZoneByIdQuery(zoneId));
        if (zone == null) return NotFound();
        if (!await CanManageZoneMembersAsync(zone))
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "No tienes permiso para gestionar miembros de esta zona." });

        var callerId = await ResolveProfileUserIdAsync();
        if (!callerId.HasValue) return Unauthorized();

        try
        {
            await zoneMemberService.AssignAsync(zoneId, request.UserId, callerId.Value, cancellationToken);
            return Ok(new { success = true });
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

    /// <summary>Quita la asignación de un miembro a la zona (no lo expulsa de la organización).</summary>
    [HttpDelete("{zoneId:int}/members/{userId:int}")]
    public async Task<IActionResult> RemoveMember(
        int zoneId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var zone = await zoneQueryService.Handle(new GetZoneByIdQuery(zoneId));
        if (zone == null) return NotFound();
        if (!await CanManageZoneMembersAsync(zone))
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "No tienes permiso para gestionar miembros de esta zona." });

        var removed = await zoneMemberService.RemoveAsync(zoneId, userId, cancellationToken);
        if (!removed) return NotFound();
        return Ok(new { success = true });
    }

    private async Task<UserAccessContext?> ResolveAccessContextAsync()
    {
        var identityId = User.GetIdentityId();
        if (identityId == null) return null;
        return await userAccessContextService.GetByIdentityIdAsync(identityId.Value);
    }

    private async Task<int?> ResolveProfileUserIdAsync()
    {
        var accessContext = await ResolveAccessContextAsync();
        return accessContext?.UserId;
    }

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

    private async Task<bool> CanManageZoneMembersAsync(Zone zone)
    {
        if (User.IsInRole("admin") || User.IsInRole("staff")) return true;
        if (!User.IsInRole("user_admin")) return false;

        var accessContext = await ResolveAccessContextAsync();
        if (accessContext?.AssociationId == null) return false;

        var farm = await farmQueryService.Handle(new GetFarmByIdQuery(zone.FarmId));
        return farm != null && farm.AssociationId == accessContext.AssociationId.Value;
    }
}
