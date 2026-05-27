using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.CultivationArea.Domain.Repositories;

public interface IFarmRepository : IAsyncRepository<Farm>
{
    Task<IReadOnlyList<Farm>> ListAllAsync();

    Task<IReadOnlyList<Farm>> ListByUserIdAsync(int userId);

    Task<IReadOnlyList<Farm>> ListByAssociationIdAsync(int associationId);

    Task<int> AssignOwnerToUnownedFarmsAsync(int associationId, int ownerUserId);
}
