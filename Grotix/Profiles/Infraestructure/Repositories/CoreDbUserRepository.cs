using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Profiles.Infrastructure.Repositories;

/// <summary>Informe Profile: persistencia Core DB para agregado usuario.</summary>
public class CoreDbUserRepository(AppDbContext context)
    : BaseRepository<User>(context), IUserRepository
{
    public async Task<User?> GetByIdentityIdAsync(int identityId)
    {
        return await Context.Set<User>()
            .FirstOrDefaultAsync(u => u.IdentityId == identityId);
    }
}
