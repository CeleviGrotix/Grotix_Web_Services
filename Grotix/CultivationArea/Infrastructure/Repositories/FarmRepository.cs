using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.CultivationArea.Infrastructure.Repositories;

public class FarmRepository(CultivationAreaDbContext context)
    : BaseRepository<Farm>(context), IFarmRepository
{
    public async Task<IReadOnlyList<Farm>> ListByUserIdAsync(int userId)
    {
        return await Context.Set<Farm>()
            .Where(f => f.UserId == userId)
            .OrderBy(f => f.Id)
            .ToListAsync();
    }
}
