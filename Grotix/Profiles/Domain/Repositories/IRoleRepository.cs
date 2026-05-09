using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.Profiles.Domain.Repositories;

public interface IRoleRepository : IAsyncRepository<Role>
{
    /// <summary>Códigos de permiso (<c>permission.Code</c>) asociados al rol en <c>role_permission</c>.</summary>
    Task<IReadOnlyList<string>> GetPermissionCodesByRoleIdAsync(int roleId);
}
