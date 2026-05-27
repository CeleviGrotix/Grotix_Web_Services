using GrotixBackend.Contracts.Auth.Lookup;
using GrotixBackend.Profiles.Domain.Repositories;

namespace GrotixBackend.Profiles.Application.ACL;

public sealed class UserAuthorizationContextService(
    IUserRepository userRepository,
    IRoleRepository roleRepository) : IUserAuthorizationContextService
{
    public async Task<UserAuthorizationContext?> GetByIdentityIdAsync(
        int identityId,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdentityIdAsync(identityId);
        if (user == null)
            return null;

        var role = await roleRepository.GetByIdAsync(user.RoleId);
        var roleNames = role == null
            ? Array.Empty<string>()
            : new[] { role.Name };
        var permissionCodes = await roleRepository.GetPermissionCodesByRoleIdAsync(user.RoleId);

        return new UserAuthorizationContext(user.IsActive, roleNames, permissionCodes);
    }
}
