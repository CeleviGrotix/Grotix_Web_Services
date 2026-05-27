using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Profiles.Infrastructure.Repositories;

public class RoleRepository(ProfilesDbContext context)
    : BaseRepository<Role>(context), IRoleRepository
{
    public async Task<IReadOnlyList<string>> GetPermissionCodesByRoleIdAsync(int roleId)
    {
        return await Context.Set<Role>()
            .AsNoTracking()
            .Where(r => r.Id == roleId)
            .SelectMany(r => r.Permissions.Select(p => p.Code))
            .ToListAsync();
    }
}
