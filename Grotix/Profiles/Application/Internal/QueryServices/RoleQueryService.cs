using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Repositories;

namespace GrotixBackend.Profiles.Application.Internal.QueryServices;

public class RoleQueryService(IRoleRepository roleRepository) : IRoleQueryService
{
    public async Task<IReadOnlyList<Role>> GetAllAsync()
    {
        var list = await roleRepository.ListAsync();
        return list.OrderBy(r => r.Id).ToList();
    }
}
