using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Security;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.CultivationArea.Infrastructure.Repositories;

public sealed class ZoneMemberService(
    CultivationAreaDbContext cultivationDb,
    ProfilesDbContext profilesDb) : IZoneMemberService
{
    public async Task<IReadOnlyList<ZoneMemberDto>> ListAsync(
        int zoneId,
        int? roleId,
        CancellationToken cancellationToken = default)
    {
        var associationId = await ResolveAssociationIdByZoneAsync(zoneId, cancellationToken);
        if (associationId == null)
            return [];

        var roles = await profilesDb.Roles
            .AsNoTracking()
            .ToDictionaryAsync(r => r.Id, r => r.Name, cancellationToken);

        var usersQuery = profilesDb.Users
            .AsNoTracking()
            .Where(u => u.AssociationId == associationId && u.IsActive);

        if (roleId.HasValue)
            usersQuery = usersQuery.Where(u => u.RoleId == roleId.Value);

        var users = await usersQuery
            .OrderBy(u => u.Id)
            .ToListAsync(cancellationToken);

        return users
            .Select(u => new ZoneMemberDto(
                u.Id,
                u.Name,
                u.Email.Value,
                u.RoleId,
                roles.TryGetValue(u.RoleId, out var roleName) ? roleName : "unknown",
                u.CreatedAt,
                null))
            .ToList();
    }

    public async Task<int> InviteAsync(
        int zoneId,
        string email,
        int roleId,
        int invitedByUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email requerido.");

        var associationId = await ResolveAssociationIdByZoneAsync(zoneId, cancellationToken)
            ?? throw new ArgumentException("La zona no tiene una asociación vinculada.");

        var normalizedEmail = email.Trim().ToLowerInvariant();

        var roleExists = await profilesDb.Roles
            .AsNoTracking()
            .AnyAsync(r => r.Id == roleId, cancellationToken);
        if (!roleExists)
            throw new ArgumentException("RoleId inválido.");

        var memberEmails = await profilesDb.Users
            .AsNoTracking()
            .Where(u => u.AssociationId == associationId && u.IsActive)
            .Select(u => u.Email)
            .ToListAsync(cancellationToken);

        var memberExists = memberEmails.Any(e =>
            string.Equals(e.Value, normalizedEmail, StringComparison.OrdinalIgnoreCase));

        if (memberExists)
            throw new InvalidOperationException("El correo ya pertenece a la asociación de la zona.");

        var pendingInviteExists = await profilesDb.AssociationInvites
            .AsNoTracking()
            .AnyAsync(
                i => i.AssociationId == associationId &&
                     i.InviteEmail == normalizedEmail &&
                     i.UsedAt == null,
                cancellationToken);

        if (pendingInviteExists)
            throw new InvalidOperationException("Ya existe una invitación pendiente para este correo.");

        var token = InviteTokenHasher.GenerateToken();
        var tokenHash = InviteTokenHasher.Hash(token);

        var invite = new AssociationInvite(
            associationId,
            normalizedEmail,
            tokenHash,
            roleId,
            expiresAt: DateTime.UtcNow.AddDays(7),
            createdByUserId: invitedByUserId);

        await profilesDb.AssociationInvites.AddAsync(invite, cancellationToken);
        await profilesDb.SaveChangesAsync(cancellationToken);
        return invite.Id;
    }

    public async Task<bool> RemoveAsync(int zoneId, int userId, CancellationToken cancellationToken = default)
    {
        var associationId = await ResolveAssociationIdByZoneAsync(zoneId, cancellationToken);
        if (associationId == null)
            return false;

        var user = await profilesDb.Users
            .FirstOrDefaultAsync(
                u => u.Id == userId &&
                     u.AssociationId == associationId &&
                     u.IsActive,
                cancellationToken);

        if (user == null)
            return false;

        user.AssignAssociation(null);
        await profilesDb.SaveChangesAsync(cancellationToken);
        return true;
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

        if (farm.AssociationId > 0)
            return farm.AssociationId;

        if (farm.UserId == null)
            return null;

        var owner = await profilesDb.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == farm.UserId, cancellationToken);

        return owner?.AssociationId;
    }
}
