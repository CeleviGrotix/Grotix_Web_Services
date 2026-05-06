using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.CultivationArea.Domain.Model.Queries;

namespace GrotixBackend.CultivationArea.Application.Internal.QueryServices;

public interface ICropQueryService
{
    Task<Crop?> Handle(GetCropByIdQuery query);
    Task<IReadOnlyList<Crop>> ListAllAsync();
}
