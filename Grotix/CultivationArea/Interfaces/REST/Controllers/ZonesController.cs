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

    public sealed record InviteZoneMemberRequest(string Email, int? RoleId);

    [HttpGet("{zoneId:int}/members")]
    public async Task<IActionResult> ListMembers(
        int zoneId,
        [FromQuery] int? roleId,
        CancellationToken cancellationToken = default)
    {
        var zone = await zoneQueryService.Handle(new GetZoneByIdQuery(zoneId));
        if (zone == null) return NotFound();
        if (!await CanAccessZoneAsync(zone)) return Forbid();

        var members = await zoneMemberService.ListAsync(zoneId, roleId, cancellationToken);
        return Ok(members.Select(m => new
        {
            userId = m.UserId,
            name = m.Name,
            email = m.Email,
            roleId = m.RoleId,
            roleName = m.RoleName,
            invitedAt = m.InvitedAt,
            invitedBy = m.InvitedBy
        }));
    }

    [HttpPost("{zoneId:int}/invite")]
    public async Task<IActionResult> InviteMember(
        int zoneId,
        [FromBody] InviteZoneMemberRequest request,
        CancellationToken cancellationToken = default)
    {
        var zone = await zoneQueryService.Handle(new GetZoneByIdQuery(zoneId));
        if (zone == null) return NotFound();
        if (!await CanAccessZoneAsync(zone)) return Forbid();

        var callerId = await ResolveProfileUserIdAsync();
        if (!callerId.HasValue) return Unauthorized();

        try
        {
            var inviteId = await zoneMemberService.InviteAsync(
                zoneId,
                request.Email,
                request.RoleId ?? 4,
                callerId.Value,
                cancellationToken);

            return Ok(new { inviteId });
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

    [HttpDelete("{zoneId:int}/members/{userId:int}")]
    public async Task<IActionResult> RemoveMember(
        int zoneId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        var zone = await zoneQueryService.Handle(new GetZoneByIdQuery(zoneId));
        if (zone == null) return NotFound();
        if (!await CanAccessZoneAsync(zone)) return Forbid();

        var removed = await zoneMemberService.RemoveAsync(zoneId, userId, cancellationToken);
        if (!removed) return NotFound();
        return Ok(new { success = true });
    }

    private async Task<int?> ResolveProfileUserIdAsync()
    {
        var identityId = User.GetIdentityId();
        if (identityId == null) return null;
        var accessContext = await userAccessContextService.GetByIdentityIdAsync(identityId.Value);
        return accessContext?.UserId;
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
