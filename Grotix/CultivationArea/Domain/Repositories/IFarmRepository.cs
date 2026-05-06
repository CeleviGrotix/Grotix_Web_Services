using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.CultivationArea.Domain.Repositories;

public interface IFarmRepository : IAsyncRepository<Farm>
{
    Task<IReadOnlyList<Farm>> ListByUserIdAsync(int userId);
}
