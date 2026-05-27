using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.CultivationArea.Infrastructure.Repositories;

public class CropRepository(CultivationAreaDbContext context)
    : BaseRepository<Crop>(context), ICropRepository
{
    public Task<bool> ExistsByCommonNameAsync(
        string commonName,
        int? excludingCropId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = commonName.Trim();
        return Context.Set<Crop>()
            .AsNoTracking()
            .AnyAsync(
                c => c.CommonName == normalizedName &&
                     (!excludingCropId.HasValue || c.Id != excludingCropId.Value),
                cancellationToken);
    }

    public Task<bool> ExistsByScientificNameAsync(
        string scientificName,
        int? excludingCropId = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = scientificName.Trim();
        return Context.Set<Crop>()
            .AsNoTracking()
            .AnyAsync(
                c => c.ScientificName == normalizedName &&
                     (!excludingCropId.HasValue || c.Id != excludingCropId.Value),
                cancellationToken);
    }
}
