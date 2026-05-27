using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.CultivationArea.Infrastructure.Repositories;

public class ZoneRepository(CultivationAreaDbContext context)
    : BaseRepository<Zone>(context), IZoneRepository
{
    public async Task<IReadOnlyList<Zone>> ListByFarmIdAsync(int farmId)
    {
        return await Context.Set<Zone>()
            .Where(z => z.FarmId == farmId)
            .OrderBy(z => z.Id)
            .ToListAsync();
    }

    public Task<bool> AnyByCropIdAsync(int cropId, CancellationToken cancellationToken = default) =>
        Context.Set<Zone>().AnyAsync(z => z.CropId == cropId, cancellationToken);
}
