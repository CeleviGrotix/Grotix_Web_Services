using GrotixBackend.Profiles.Domain.Model.Aggregates;

namespace GrotixBackend.Profiles.Application.Internal.QueryServices;

public interface IRoleQueryService
{
    Task<IReadOnlyList<Role>> GetAllAsync();
}
