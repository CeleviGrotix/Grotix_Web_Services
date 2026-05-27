namespace GrotixBackend.Contracts.Auth.Lookup;

public interface IUserAuthorizationContextService
{
    Task<UserAuthorizationContext?> GetByIdentityIdAsync(int identityId, CancellationToken cancellationToken = default);
}
