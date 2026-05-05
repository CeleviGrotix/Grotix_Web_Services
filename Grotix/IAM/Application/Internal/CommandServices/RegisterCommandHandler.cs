using MediatR;
using GrotixBackend.IAM.Domain.Model.Aggregates;
using GrotixBackend.IAM.Domain.Model.Commands;
using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.IAM.Application.Internal.OutboundServices.ACL;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.IAM.Application.Internal.CommandServices;

public class RegisterCommandHandler(
    IIdentityRepository identityRepository,
    IUnitOfWork unitOfWork,
    IExternalProfileService profileService
) : IRequestHandler<RegisterCommand, int>
{
    public async Task<int> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        if (await identityRepository.ExistsByEmailAsync(command.Email))
            throw new ApplicationException($"El correo '{command.Email}' ya está registrado.");

        var identity = new Identity(command.Email, command.Password);

        await identityRepository.AddAsync(identity);
        await unitOfWork.CompleteAsync();

        await profileService.CreateUserAndReturnId(identity.Id, identity.UserName);

        return identity.Id;
    }
}