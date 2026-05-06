using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.Profiles.Domain.Repositories;

public interface IStaffRepository : IAsyncRepository<Staff>
{
    Task<Staff?> GetByUserIdAsync(int userId);
}
