using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Queries;

namespace GrotixBackend.CultivationArea.Application.Internal.QueryServices;

public interface IFarmQueryService
{
    Task<Farm?> Handle(GetFarmByIdQuery query);
    Task<IReadOnlyList<Farm>> Handle(ListFarmsForUserQuery query);
    Task<IReadOnlyList<Farm>> Handle(ListFarmsForAssociationQuery query);
    Task<IReadOnlyList<Farm>> Handle(ListAllFarmsQuery query);
}
