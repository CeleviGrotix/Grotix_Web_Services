using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Queries;

namespace GrotixBackend.CultivationArea.Application.Internal.QueryServices;

public interface IZoneQueryService
{
    Task<Zone?> Handle(GetZoneByIdQuery query);
    Task<IReadOnlyList<Zone>> Handle(ListZonesForFarmQuery query);
}
