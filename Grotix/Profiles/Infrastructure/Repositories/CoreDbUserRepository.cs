using GrotixBackend.Profiles.Domain.Model;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Profiles.Infrastructure.Repositories;

/// <summary>Informe Profile: persistencia Core DB para agregado usuario.</summary>
public class CoreDbUserRepository(ProfilesDbContext context)
    : BaseRepository<User>(context), IUserRepository
{
    public async Task<User?> GetByIdentityIdAsync(int identityId)
    {
        return await Context.Set<User>()
            .FirstOrDefaultAsync(u => u.IdentityId == identityId);
    }

    public async Task<IReadOnlyList<User>> ListFarmersOrderedByIdAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<User>()
            .AsNoTracking()
            .Where(u => FarmerRoles.Ids.Contains(u.RoleId))
            .OrderBy(u => u.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<User>> ListFarmersByAssociationIdAsync(int associationId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<User>()
            .AsNoTracking()
            .Where(u => u.AssociationId == associationId && FarmerRoles.Ids.Contains(u.RoleId))
            .OrderBy(u => u.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<User?> GetFarmerByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await Context.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null || !FarmerRoles.IsFarmerRole(user.RoleId))
            return null;
        return user;
    }

    public async Task<bool> HasUserAdminForAssociationAsync(int associationId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<User>()
            .AsNoTracking()
            .AnyAsync(
                u => u.AssociationId == associationId && u.RoleId == (int)RoleType.user_admin,
                cancellationToken);
    }
}
