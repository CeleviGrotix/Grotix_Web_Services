using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.CultivationArea.Infrastructure.Repositories;

public sealed class ZoneMemberService(
    CultivationAreaDbContext cultivationDb,
    ProfilesDbContext profilesDb,
    IZoneMemberRepository zoneMemberRepository,
    ICultivationAreaUnitOfWork unitOfWork) : IZoneMemberService
{
    public async Task<IReadOnlyList<ZoneMemberDto>> ListAsync(
        int zoneId,
        CancellationToken cancellationToken = default)
    {
        var assignments = await zoneMemberRepository.ListByZoneIdAsync(zoneId);
        if (assignments.Count == 0)
            return [];

        var userIds = assignments.Select(a => a.UserId).ToList();
        var roles = await profilesDb.Roles
            .AsNoTracking()
            .ToDictionaryAsync(r => r.Id, r => r.Name, cancellationToken);

        var users = await profilesDb.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id) && u.IsActive)
            .ToListAsync(cancellationToken);

        return assignments
            .Select(a =>
            {
                var user = users.FirstOrDefault(u => u.Id == a.UserId);
                if (user == null)
                    return null;

                return new ZoneMemberDto(
                    user.Id,
                    user.Name,
                    user.Email.Value,
                    user.RoleId,
                    roles.TryGetValue(user.RoleId, out var roleName) ? roleName : "unknown",
                    a.AssignedAt,
                    a.AssignedByUserId);
            })
            .Where(dto => dto != null)
            .Cast<ZoneMemberDto>()
            .ToList();
    }

    public async Task AssignAsync(
        int zoneId,
        int userId,
        int assignedByUserId,
        CancellationToken cancellationToken = default)
    {
        var associationId = await ResolveAssociationIdByZoneAsync(zoneId, cancellationToken)
                            ?? throw new ArgumentException("La zona no tiene una asociación vinculada.");

        var targetUser = await profilesDb.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(
                u => u.Id == userId && u.AssociationId == associationId && u.IsActive,
                cancellationToken)
            ?? throw new ArgumentException("El usuario no pertenece a la organización de la zona.");

        if (await zoneMemberRepository.ExistsAsync(zoneId, userId))
            throw new InvalidOperationException("El usuario ya está asignado a esta zona.");

        await zoneMemberRepository.AddAsync(new ZoneMember(zoneId, targetUser.Id, assignedByUserId));
        await unitOfWork.CompleteAsync();
    }

    public async Task<bool> RemoveAsync(int zoneId, int userId, CancellationToken cancellationToken = default)
    {
        var assignment = await zoneMemberRepository.GetByZoneAndUserAsync(zoneId, userId);
        if (assignment == null)
            return false;

        await zoneMemberRepository.DeleteAsync(assignment);
        await unitOfWork.CompleteAsync();
        return true;
    }

    public async Task<bool> CanUserAccessZoneAsync(
        int zoneId,
        int userId,
        bool isOrgAdmin,
        CancellationToken cancellationToken = default)
    {
        if (isOrgAdmin)
            return true;

        return await zoneMemberRepository.IsUserAssignedToZoneAsync(zoneId, userId);
    }

    private async Task<int?> ResolveAssociationIdByZoneAsync(int zoneId, CancellationToken cancellationToken)
    {
        var zone = await cultivationDb.Zones
            .AsNoTracking()
            .FirstOrDefaultAsync(z => z.Id == zoneId, cancellationToken);
        if (zone == null)
            return null;

        var farm = await cultivationDb.Farms
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == zone.FarmId, cancellationToken);
        if (farm == null)
            return null;

        return farm.AssociationId > 0 ? farm.AssociationId : null;
    }
}
