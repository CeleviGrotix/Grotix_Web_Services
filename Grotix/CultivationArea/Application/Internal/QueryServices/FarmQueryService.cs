using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Queries;
using GrotixBackend.CultivationArea.Domain.Repositories;

namespace GrotixBackend.CultivationArea.Application.Internal.QueryServices;

public class FarmQueryService(IFarmRepository farmRepository) : IFarmQueryService
{
    public async Task<Farm?> Handle(GetFarmByIdQuery query) =>
        await farmRepository.GetByIdAsync(query.FarmId);

    public async Task<IReadOnlyList<Farm>> Handle(ListFarmsForUserQuery query) =>
        await farmRepository.ListByUserIdAsync(query.UserId);
}
