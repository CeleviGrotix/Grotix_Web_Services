using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Profiles.Infrastructure.Repositories;

public sealed class AssociationInviteRepository(AppDbContext context)
    : BaseRepository<AssociationInvite>(context), IAssociationInviteRepository
{
    public async Task<AssociationInvite?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        var normalized = tokenHash.Trim();
        return await Context.Set<AssociationInvite>()
            .FirstOrDefaultAsync(i => i.TokenHash == normalized, cancellationToken);
    }

    public async Task<bool> TryMarkUsedAsync(int inviteId, CancellationToken cancellationToken = default)
    {
        var rows = await Context.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE association_invite SET UsedAt = UTC_TIMESTAMP(6) WHERE InviteID = {inviteId} AND UsedAt IS NULL",
            cancellationToken);
        return rows == 1;
    }
}
