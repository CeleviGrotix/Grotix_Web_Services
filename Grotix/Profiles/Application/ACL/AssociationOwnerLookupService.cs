using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.Profiles.Domain.Repositories;

namespace GrotixBackend.Profiles.Application.ACL;

public sealed class AssociationOwnerLookupService(IUserRepository userRepository)
    : IAssociationOwnerLookupService
{
    public async Task<int?> GetOwnerUserIdAsync(int associationId, CancellationToken cancellationToken = default)
    {
        var owner = await userRepository.GetUserAdminByAssociationIdAsync(associationId, cancellationToken);
        return owner?.Id;
    }
}
