using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.CultivationArea.Domain.Repositories;

public interface IZoneMemberRepository : IAsyncRepository<ZoneMember>
{
    Task<bool> ExistsAsync(int zoneId, int userId);

    Task<IReadOnlyList<ZoneMember>> ListByZoneIdAsync(int zoneId);

    Task<ZoneMember?> GetByZoneAndUserAsync(int zoneId, int userId);

    Task<bool> IsUserAssignedToZoneAsync(int zoneId, int userId);

    Task<IReadOnlyList<int>> ListAssignedZoneIdsForFarmAsync(int farmId, int userId);

    Task<IReadOnlyList<int>> ListFarmIdsForUserAsync(int userId);
}
