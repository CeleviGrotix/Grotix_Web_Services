using MediatR;
using GrotixBackend.Contracts.Profiles.Provisioning;
using GrotixBackend.IAM.Domain.Model.Aggregates;
using GrotixBackend.IAM.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Model.Notifications;
using GrotixBackend.IAM.Domain.Model.ValueObjects;
using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.Profiles.Domain.Services;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Profiles.Domain.Security;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.Shared.Domain.Repositories;

namespace GrotixBackend.IAM.Application.Internal.CommandServices;

/// <summary>Orquesta alta de cuenta (informe: CreateAccountHandler).</summary>
public class CreateAccountHandler(
    IIdentityRepository identityRepository,
    IUnitOfWork unitOfWork,
    IExternalProfileService profileService,
    IPasswordHasher passwordHasher,
    IMediator mediator,
    IAssociationInviteRepository inviteRepository
) : IRequestHandler<CreateAccountCommand, int>
{
    public async Task<int> Handle(CreateAccountCommand command, CancellationToken cancellationToken)
    {
        if (await identityRepository.ExistsByEmailAsync(command.Email))
            throw new ApplicationException($"El correo '{command.Email}' ya está registrado.");

        if (string.IsNullOrWhiteSpace(command.InviteToken))
            throw new ApplicationException("Se requiere un token de invitación válido.");

        var tokenHash = InviteTokenHasher.Hash(command.InviteToken);
        var invite = await inviteRepository.GetByTokenHashAsync(tokenHash, cancellationToken)
                     ?? throw new ApplicationException("Invitación no válida o desconocida.");

        var registrationEmail = UserEmail.Create(command.Email);
        if (!string.Equals(registrationEmail.Value, invite.InviteEmail, StringComparison.OrdinalIgnoreCase))
            throw new ApplicationException("El correo debe coincidir exactamente con el de la invitación.");

        if (invite.UsedAt != null)
            throw new ApplicationException("Esta invitación ya fue utilizada.");

        if (invite.ExpiresAt.HasValue && invite.ExpiresAt.Value < DateTime.UtcNow)
            throw new ApplicationException("La invitación ha expirado.");

        if (invite.RoleId != (int)RoleType.user_admin &&
            invite.RoleId != (int)RoleType.user_basic &&
            invite.RoleId != (int)RoleType.user_advanced)
            throw new InvalidOperationException("La invitación no tiene un rol válido.");

        Identity.VerifyPasswordStrength(command.Password);
        var hash = passwordHasher.Hash(command.Password);
        var identity = new Identity(registrationEmail.Value, PasswordHash.FromHash(hash));

        await identityRepository.AddAsync(identity);
        await unitOfWork.CompleteAsync();

        await profileService.CreateUserAndReturnId(new CreateProfileUserRequest(
            identity.Id,
            identity.UserName,
            invite.RoleId,
            invite.AssociationId));

        var marked = await inviteRepository.TryMarkUsedAsync(invite.Id, cancellationToken);
        if (!marked)
            throw new ApplicationException("No se pudo confirmar la invitación (posible uso simultáneo). Intenta de nuevo.");

        await mediator.Publish(new UserRegisteredNotification(identity.Id, identity.UserName), cancellationToken);

        return identity.Id;
    }
}
