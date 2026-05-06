using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Queries;
using GrotixBackend.CultivationArea.Domain.Repositories;

namespace GrotixBackend.CultivationArea.Application.Internal.QueryServices;

public class CropQueryService(ICropRepository cropRepository) : ICropQueryService
{
    public async Task<Crop?> Handle(GetCropByIdQuery query) =>
        await cropRepository.GetByIdAsync(query.CropId);

    public async Task<IReadOnlyList<Crop>> ListAllAsync()
    {
        var list = await cropRepository.ListAsync();
        return list.OrderBy(c => c.CommonName).ToList();
    }
}
