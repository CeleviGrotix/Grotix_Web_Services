using MediatR;
using GrotixBackend.IAM.Domain.Model.Aggregates;
using GrotixBackend.IAM.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Model.Notifications;
using GrotixBackend.IAM.Domain.Model.ValueObjects;
using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.IAM.Application.Internal.OutboundServices.ACL;
using GrotixBackend.Profiles.Domain.Services;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.IAM.Application.Internal.CommandServices;

/// <summary>Orquesta alta de cuenta (informe: CreateAccountHandler).</summary>
public class CreateAccountHandler(
    IIdentityRepository identityRepository,
    IUnitOfWork unitOfWork,
    IExternalProfileService profileService,
    IPasswordHasher passwordHasher,
    IMediator mediator
) : IRequestHandler<CreateAccountCommand, int>
{
    public async Task<int> Handle(CreateAccountCommand command, CancellationToken cancellationToken)
    {
        if (await identityRepository.ExistsByEmailAsync(command.Email))
            throw new ApplicationException($"El correo '{command.Email}' ya está registrado.");

        Identity.VerifyPasswordStrength(command.Password);
        var hash = passwordHasher.Hash(command.Password);
        var identity = new Identity(command.Email, PasswordHash.FromHash(hash));

        await identityRepository.AddAsync(identity);
        await unitOfWork.CompleteAsync();

        await profileService.CreateUserAndReturnId(identity.Id, identity.UserName);

        await mediator.Publish(new UserRegisteredNotification(identity.Id, identity.UserName), cancellationToken);

        return identity.Id;
    }
}
