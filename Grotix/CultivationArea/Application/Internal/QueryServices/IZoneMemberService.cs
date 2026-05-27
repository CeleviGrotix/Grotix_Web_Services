namespace GrotixBackend.CultivationArea.Application.Internal.QueryServices;

public sealed record ZoneMemberDto(
    int UserId,
    string? Name,
    string Email,
    int RoleId,
    string RoleName,
    DateTime InvitedAt,
    int? InvitedBy);

public interface IZoneMemberService
{
    Task<IReadOnlyList<ZoneMemberDto>> ListAsync(int zoneId, int? roleId, CancellationToken cancellationToken = default);
    Task<int> InviteAsync(
        int zoneId,
        string email,
        int roleId,
        int invitedByUserId,
        CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(int zoneId, int userId, CancellationToken cancellationToken = default);
}
