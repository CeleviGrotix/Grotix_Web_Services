namespace GrotixBackend.CultivationArea.Application.Internal.QueryServices;

public sealed record ZoneMemberDto(
    int UserId,
    string? Name,
    string Email,
    int RoleId,
    string RoleName,
    DateTime AssignedAt,
    int AssignedByUserId);

public interface IZoneMemberService
{
    Task<IReadOnlyList<ZoneMemberDto>> ListAsync(int zoneId, CancellationToken cancellationToken = default);

    Task AssignAsync(
        int zoneId,
        int userId,
        int assignedByUserId,
        CancellationToken cancellationToken = default);

    Task<bool> RemoveAsync(int zoneId, int userId, CancellationToken cancellationToken = default);

    Task<bool> CanUserAccessZoneAsync(int zoneId, int userId, bool isOrgAdmin, CancellationToken cancellationToken = default);
}
