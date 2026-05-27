using MediatR;
using GrotixBackend.Contracts.Auth.Identity;
using GrotixBackend.Contracts.Auth.Notifications;
using GrotixBackend.Contracts.Auth.Security;
using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.Contracts.Profiles.Provisioning;
using GrotixBackend.Contracts.Profiles.Invites;
using GrotixBackend.IAM.Domain.Model.Aggregates;
using GrotixBackend.IAM.Domain.Model.Commands;
using GrotixBackend.IAM.Domain.Model.ValueObjects;
using GrotixBackend.IAM.Domain.Repositories;

namespace GrotixBackend.IAM.Application.Internal.CommandServices;

/// <summary>Orquesta alta de cuenta (informe: CreateAccountHandler).</summary>
public class CreateAccountHandler(
    IIdentityRepository identityRepository,
    IIamUnitOfWork unitOfWork,
    IExternalProfileService profileService,
    IPasswordHasher passwordHasher,
    IMediator mediator,
    IAssociationInviteAccessService inviteAccessService,
    IAssociationFarmOwnerSyncService associationFarmOwnerSyncService
) : IRequestHandler<CreateAccountCommand, int>
{
    private const int UserAdminRoleId = 3;

    public async Task<int> Handle(CreateAccountCommand command, CancellationToken cancellationToken)
    {
        Identity.VerifyPasswordStrength(command.Password);
        var normalizedEmail = AuthEmail.Normalize(command.Email);

        var invite = await inviteAccessService.ValidateForRegistrationAsync(
            command.InviteToken,
            command.Email,
            cancellationToken);

        var existingIdentity = await identityRepository.GetByEmailAsync(normalizedEmail);
        if (existingIdentity is not null)
        {
            if (!passwordHasher.Verify(command.Password, existingIdentity.HashedPassword.HashedValue))
                throw new ApplicationException(
                    "Ya existe un registro iniciado con este correo. Usa la misma contraseña del primer intento.");

            if (await profileService.ExistsForIdentityAsync(existingIdentity.Id, cancellationToken))
                throw new ApplicationException($"El correo '{command.Email}' ya está registrado.");

            return await CompleteRegistrationAsync(existingIdentity, invite, cancellationToken);
        }

        var hash = passwordHasher.Hash(command.Password);
        var identity = new Identity(normalizedEmail, PasswordHash.FromHash(hash));

        await identityRepository.AddAsync(identity);
        await unitOfWork.CompleteAsync();

        return await CompleteRegistrationAsync(identity, invite, cancellationToken);
    }

    private async Task<int> CompleteRegistrationAsync(
        Identity identity,
        ValidatedAssociationInvite invite,
        CancellationToken cancellationToken)
    {
        await profileService.CreateUserAndReturnId(new CreateProfileUserRequest(
            identity.Id,
            identity.UserName,
            invite.RoleId,
            invite.AssociationId));

        if (invite.RoleId == UserAdminRoleId)
            await associationFarmOwnerSyncService.SyncUnownedFarmsAsync(invite.AssociationId, cancellationToken);

        var marked = await inviteAccessService.TryMarkUsedAsync(invite.InviteId, cancellationToken);
        if (!marked)
            throw new ApplicationException("No se pudo confirmar la invitación (posible uso simultáneo). Intenta de nuevo.");

        await mediator.Publish(new UserRegisteredNotification(identity.Id, identity.UserName), cancellationToken);

        return identity.Id;
    }
}
