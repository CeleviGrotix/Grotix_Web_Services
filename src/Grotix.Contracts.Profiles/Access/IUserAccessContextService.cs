namespace GrotixBackend.Contracts.Profiles.Access;

public interface IUserAccessContextService
{
    Task<UserAccessContext?> GetByIdentityIdAsync(int identityId, CancellationToken cancellationToken = default);
}
