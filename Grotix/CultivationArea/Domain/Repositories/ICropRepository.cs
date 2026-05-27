using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.CultivationArea.Domain.Repositories;

public interface ICropRepository : IAsyncRepository<Crop>
{
    Task<bool> ExistsByCommonNameAsync(
        string commonName,
        int? excludingCropId = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByScientificNameAsync(
        string scientificName,
        int? excludingCropId = null,
        CancellationToken cancellationToken = default);
}
