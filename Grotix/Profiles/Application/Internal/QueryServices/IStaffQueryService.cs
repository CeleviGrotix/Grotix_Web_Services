using GrotixBackend.Profiles.Domain.Model.Aggregates;

namespace GrotixBackend.Profiles.Application.Internal.QueryServices;

public interface IStaffQueryService
{
    Task<IReadOnlyList<Staff>> GetAllAsync();
    Task<Staff?> GetByIdAsync(int staffId);
}
