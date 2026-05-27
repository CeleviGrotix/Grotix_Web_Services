namespace GrotixBackend.Contracts.Profiles.Access;

public interface IAssociationOwnerLookupService
{
    Task<int?> GetOwnerUserIdAsync(int associationId, CancellationToken cancellationToken = default);
}
