using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Model.Queries;

namespace GrotixBackend.Profiles.Application.ACL;

public sealed class UserAccessContextService(IUserQueryService userQueryService) : IUserAccessContextService
{
    public async Task<UserAccessContext?> GetByIdentityIdAsync(int identityId, CancellationToken cancellationToken = default)
    {
        var user = await userQueryService.Handle(new GetUserByIdentityQuery(identityId));
        if (user == null)
            return null;

        return new UserAccessContext(
            user.Id,
            user.IdentityId,
            user.AssociationId,
            user.IsActive);
    }
}
