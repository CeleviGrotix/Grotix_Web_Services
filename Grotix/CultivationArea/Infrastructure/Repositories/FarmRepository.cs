using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.CultivationArea.Infrastructure.Repositories;

public class FarmRepository(CultivationAreaDbContext context)
    : BaseRepository<Farm>(context), IFarmRepository
{
    public async Task<IReadOnlyList<Farm>> ListAllAsync()
    {
        return await Context.Set<Farm>()
            .OrderBy(f => f.Id)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Farm>> ListByUserIdAsync(int userId)
    {
        return await Context.Set<Farm>()
            .Where(f => f.UserId == userId)
            .OrderBy(f => f.Id)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Farm>> ListByAssociationIdAsync(int associationId)
    {
        return await Context.Set<Farm>()
            .Where(f => f.AssociationId == associationId)
            .OrderBy(f => f.Id)
            .ToListAsync();
    }

    public async Task<int> AssignOwnerToUnownedFarmsAsync(int associationId, int ownerUserId)
    {
        var farms = await Context.Set<Farm>()
            .Where(f => f.AssociationId == associationId && f.UserId == null)
            .ToListAsync();

        foreach (var farm in farms)
            farm.AssignOwner(ownerUserId);

        return farms.Count;
    }

    public Task<bool> ExistsByAssociationAndNameAsync(
        int associationId,
        string name,
        int? excludingFarmId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = name.Trim();
        return Context.Set<Farm>()
            .AsNoTracking()
            .AnyAsync(
                f => f.AssociationId == associationId &&
                     f.Name == normalizedName &&
                     (!excludingFarmId.HasValue || f.Id != excludingFarmId.Value),
                cancellationToken);
    }
}
