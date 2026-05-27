namespace GrotixBackend.Contracts.Profiles.Access;

public interface IAssociationExistenceService
{
    Task<bool> ExistsAsync(int associationId, CancellationToken cancellationToken = default);
}
