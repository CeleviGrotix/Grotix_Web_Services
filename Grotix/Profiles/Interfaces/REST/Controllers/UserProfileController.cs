using MediatR;
using GrotixBackend.Contracts.Auth.Claims;
using GrotixBackend.Profiles.Application.Internal.CommandServices;
using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Model.Queries;
using GrotixBackend.Profiles.Interfaces.REST.Resources;
using GrotixBackend.Profiles.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GrotixBackend.Profiles.Interfaces.REST.Controllers;

[ApiController]
[Route("api/v1/profile")]
public class UserProfileController(
    IMediator mediator,
    IUserCommandService userCommandService,
    IUserQueryService userQueryService,
    IStaffQueryService staffQueryService) : ControllerBase  // ← una sola vez
{
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var identityId = User.GetIdentityId();
        if (identityId == null) return Unauthorized();
        var profile = await userQueryService.Handle(new GetUserByIdentityQuery(identityId.Value));
        if (profile == null) return NotFound();
        return Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(profile));
    }

    public record PatchProfileRequest(string? Name, string? TaxId, string? Phone, string? ProfilePicture);

    [HttpPatch("{userId:int}")]
    [Authorize]
    public async Task<IActionResult> PatchProfile(int userId, [FromBody] PatchProfileRequest request)
    {
        var identityId = User.GetIdentityId();
        if (identityId == null) return Unauthorized();

        var caller = await userQueryService.Handle(new GetUserByIdentityQuery(identityId.Value));
        var isAdmin = User.IsInRole("admin");
        if (!isAdmin && (caller == null || caller.Id != userId))
            return Forbid();

        try
        {
            var updated = await userCommandService.Handle(new UpdateUserProfileCommand(
                userId, request.Name, request.TaxId, request.Phone, request.ProfilePicture));
            return Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    public record PatchPreferencesRequest(bool Push, bool Email);

    [HttpPatch("{userId:int}/preferences")]
    [Authorize]
    public async Task<IActionResult> PatchPreferences(int userId, [FromBody] PatchPreferencesRequest request)
    {
        var identityId = User.GetIdentityId();
        if (identityId == null) return Unauthorized();

        var caller = await userQueryService.Handle(new GetUserByIdentityQuery(identityId.Value));
        var isAdmin = User.IsInRole("admin");
        if (!isAdmin && (caller == null || caller.Id != userId))
            return Forbid();

        try
        {
            var updated = await mediator.Send(new UpdateUserPreferencesCommand(userId, request.Push, request.Email));
            return Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(updated));
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    public record AssignRoleRequest(int RoleId);

    [HttpPatch("{userId:int}/role")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AssignRole(int userId, [FromBody] AssignRoleRequest request)
    {
        try
        {
            var updated = await userCommandService.Handle(new AssignUserRoleCommand(userId, request.RoleId));
            return Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(updated));
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

    [HttpGet("me/staff")]
    [Authorize]
    public async Task<IActionResult> GetMyStaffProfile()
    {
        var identityId = User.GetIdentityId();
        if (identityId == null) return Unauthorized();

        var staff = await staffQueryService.GetByIdentityIdAsync(identityId.Value);
        if (staff == null) return NotFound(new { message = "Staff profile not found." });

        return Ok(new {
            staff.Id,
            staff.UserId,
            staff.TechnicalRole,
            staff.LastSystemAccess,
            staff.IsActive
        });
    }
}