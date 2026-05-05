using GrotixBackend.IAM.Application.Internal.OutboundServices.ACL;
using GrotixBackend.Profiles.Application.Internal.CommandServices;
using GrotixBackend.Profiles.Domain.Model.Commands;

namespace GrotixBackend.Profiles.Application.ACL;

public class ExternalProfileService(IUserCommandService userCommandService) : IExternalProfileService
{
    public async Task<int> CreateUserAndReturnId(int identityId, string email)
    {
        var command = new CreateUserCommand(
            IdentityId: identityId,
            Email: email
        );

        var user = await userCommandService.Handle(command);
        return user.Id;
    }
}