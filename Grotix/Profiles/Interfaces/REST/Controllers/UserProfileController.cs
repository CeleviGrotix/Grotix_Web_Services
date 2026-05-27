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
    IUserNotificationCommandService userNotificationCommandService,
    IUserNotificationQueryService userNotificationQueryService,
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

    public record CreateNotificationRequest(string Title, string Message, string? Type);

    [HttpPost("{userId:int}/notifications")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> CreateNotification(int userId, [FromBody] CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var notification = await userNotificationCommandService.CreateAsync(
                userId,
                request.Title,
                request.Message,
                request.Type,
                cancellationToken);

            return Ok(ToNotificationDto(notification));
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

    [HttpGet("me/notifications")]
    [Authorize]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var caller = await GetCallerProfileAsync(cancellationToken);
        if (caller == null) return Unauthorized();

        var notifications = await userNotificationQueryService.ListByUserAsync(caller.Id, unreadOnly, limit);
        return Ok(notifications.Select(ToNotificationDto));
    }

    [HttpGet("me/notifications/unread-count")]
    [Authorize]
    public async Task<IActionResult> GetMyUnreadNotificationCount(CancellationToken cancellationToken = default)
    {
        var caller = await GetCallerProfileAsync(cancellationToken);
        if (caller == null) return Unauthorized();

        var unreadCount = await userNotificationQueryService.CountUnreadAsync(caller.Id);
        return Ok(new { unreadCount });
    }

    [HttpPatch("me/notifications/{notificationId:int}/read")]
    [Authorize]
    public async Task<IActionResult> MarkMyNotificationAsRead(int notificationId, CancellationToken cancellationToken = default)
    {
        var caller = await GetCallerProfileAsync(cancellationToken);
        if (caller == null) return Unauthorized();

        var updated = await userNotificationCommandService.MarkAsReadAsync(caller.Id, notificationId, cancellationToken);
        if (!updated) return NotFound();
        return Ok(new { success = true });
    }

    [HttpPatch("me/notifications/read-all")]
    [Authorize]
    public async Task<IActionResult> MarkAllMyNotificationsAsRead(CancellationToken cancellationToken = default)
    {
        var caller = await GetCallerProfileAsync(cancellationToken);
        if (caller == null) return Unauthorized();

        var updated = await userNotificationCommandService.MarkAllAsReadAsync(caller.Id, cancellationToken);
        return Ok(new { success = true, updated });
    }

    private async Task<Domain.Model.Aggregates.User?> GetCallerProfileAsync(CancellationToken cancellationToken)
    {
        var identityId = User.GetIdentityId();
        if (identityId == null)
            return null;

        return await userQueryService.Handle(new GetUserByIdentityQuery(identityId.Value));
    }

    private static object ToNotificationDto(Domain.Model.Aggregates.UserNotification notification) => new
    {
        id = notification.Id,
        userId = notification.UserId,
        title = notification.Title,
        message = notification.Message,
        type = notification.Type,
        isRead = notification.IsRead,
        createdAt = notification.CreatedAt,
        readAt = notification.ReadAt
    };
}