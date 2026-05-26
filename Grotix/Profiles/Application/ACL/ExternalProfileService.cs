using GrotixBackend.Contracts.Profiles.Provisioning;
using GrotixBackend.Profiles.Application.Internal.CommandServices;
using GrotixBackend.Profiles.Domain.Model.Commands;

namespace GrotixBackend.Profiles.Application.ACL;

public class ExternalProfileService(IUserCommandService userCommandService) : IExternalProfileService
{
    public async Task<int> CreateUserAndReturnId(CreateProfileUserRequest request)
    {
        var command = new CreateUserCommand(
            IdentityId: request.IdentityId,
            Email: request.Email,
            RoleId: request.RoleId,
            AssociationId: request.AssociationId
        );

        var user = await userCommandService.Handle(command);
        return user.Id;
    }
}