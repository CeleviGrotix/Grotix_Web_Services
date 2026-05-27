namespace GrotixBackend.Contracts.Auth.Lookup;

public interface IIdentityLookupService
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<int?> GetIdentityIdByEmailAsync(string email, CancellationToken cancellationToken = default);
}
