using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.CultivationArea.Infrastructure.Repositories;

public class ZoneRepository(AppDbContext context)
    : BaseRepository<Zone>(context), IZoneRepository
{
    public async Task<IReadOnlyList<Zone>> ListByFarmIdAsync(int farmId)
    {
        return await Context.Set<Zone>()
            .Where(z => z.FarmId == farmId)
            .OrderBy(z => z.Id)
            .ToListAsync();
    }
}
