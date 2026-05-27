using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Commands;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Profiles.Domain.Security;

namespace GrotixBackend.Profiles.Application.Internal.CommandServices;

public sealed class AssociationInviteCommandService(
    IAssociationInviteRepository inviteRepository,
    IAssociationRepository associationRepository,
    IProfilesUnitOfWork unitOfWork
) : IAssociationInviteCommandService
{
    public async Task<CreateAssociationInviteResult> Handle(CreateAssociationInviteCommand command)
    {
        if (!await associationRepository.ExistsAsync(command.AssociationId))
            throw new ArgumentException($"La asociación {command.AssociationId} no existe.");

        var inviteEmailVo = UserEmail.Create(command.InviteEmail);

        if (await inviteRepository.HasPendingInviteForEmailAsync(command.AssociationId, inviteEmailVo.Value))
            throw new ArgumentException(
                "Ya existe una invitación pendiente para este correo en esta asociación.");

        if (command.RoleId != (int)RoleType.user_admin &&
            command.RoleId != (int)RoleType.user_basic &&
            command.RoleId != (int)RoleType.user_advanced)
            throw new ArgumentException(
                "El rol invitado debe ser user_admin (3), user_basic (4) o user_advanced (5).");

        var plaintext = InviteTokenHasher.GenerateToken();
        var hash = InviteTokenHasher.Hash(plaintext);

        var invite = new AssociationInvite(
            command.AssociationId,
            inviteEmailVo.Value,
            hash,
            command.RoleId,
            command.ExpiresAt,
            command.CreatedByUserId);

        await inviteRepository.AddAsync(invite);
        await unitOfWork.CompleteAsync();

        return new CreateAssociationInviteResult(invite.Id, plaintext, command.ExpiresAt);
    }
}
