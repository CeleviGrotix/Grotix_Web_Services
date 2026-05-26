using GrotixBackend.Contracts.Auth.Lookup;
using GrotixBackend.IAM.Domain.Repositories;

namespace GrotixBackend.IAM.Application.ACL;

public sealed class IdentityLookupService(IIdentityRepository identityRepository) : IIdentityLookupService
{
    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        => identityRepository.ExistsByEmailAsync(email);
}
