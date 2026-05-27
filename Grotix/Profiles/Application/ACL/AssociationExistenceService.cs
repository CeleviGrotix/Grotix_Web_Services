using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.Profiles.Domain.Repositories;

namespace GrotixBackend.Profiles.Application.ACL;

public sealed class AssociationExistenceService(IAssociationRepository associationRepository)
    : IAssociationExistenceService
{
    public Task<bool> ExistsAsync(int associationId, CancellationToken cancellationToken = default) =>
        associationRepository.ExistsAsync(associationId);
}
