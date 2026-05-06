using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Queries;
using GrotixBackend.CultivationArea.Domain.Repositories;

namespace GrotixBackend.CultivationArea.Application.Internal.QueryServices;

public class ZoneQueryService(IZoneRepository zoneRepository) : IZoneQueryService
{
    public async Task<Zone?> Handle(GetZoneByIdQuery query) =>
        await zoneRepository.GetByIdAsync(query.ZoneId);

    public async Task<IReadOnlyList<Zone>> Handle(ListZonesForFarmQuery query) =>
        await zoneRepository.ListByFarmIdAsync(query.FarmId);
}
