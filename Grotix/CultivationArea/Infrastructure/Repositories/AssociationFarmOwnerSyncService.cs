using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.CultivationArea.Domain.Repositories;

namespace GrotixBackend.CultivationArea.Infrastructure.Repositories;

public sealed class AssociationFarmOwnerSyncService(
    IAssociationOwnerLookupService associationOwnerLookupService,
    IFarmRepository farmRepository,
    ICultivationAreaUnitOfWork unitOfWork) : IAssociationFarmOwnerSyncService
{
    public async Task SyncUnownedFarmsAsync(int associationId, CancellationToken cancellationToken = default)
    {
        var ownerUserId = await associationOwnerLookupService.GetOwnerUserIdAsync(associationId, cancellationToken);
        if (ownerUserId == null)
            return;

        var updated = await farmRepository.AssignOwnerToUnownedFarmsAsync(associationId, ownerUserId.Value);
        if (updated > 0)
            await unitOfWork.CompleteAsync();
    }
}
