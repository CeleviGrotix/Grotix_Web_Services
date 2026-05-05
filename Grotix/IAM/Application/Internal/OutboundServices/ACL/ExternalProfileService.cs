using GrotixBackend.Profiles.Application.Internal.CommandServices;
using GrotixBackend.Profiles.Domain.Model.Commands;

namespace GrotixBackend.IAM.Application.Internal.OutboundServices.ACL;

public class ExternalProfileService(IUserCommandService userCommandService) : IExternalProfileService
{
    public async Task<int> CreateUserAndReturnId(int identityId, string email)
    {
        var createUserCommand = new CreateUserCommand(
            identityId,
            null,
            email,
            null,  
            null   
        );

        var user = await userCommandService.Handle(createUserCommand);
        return user?.Id ?? 0;
    }
}