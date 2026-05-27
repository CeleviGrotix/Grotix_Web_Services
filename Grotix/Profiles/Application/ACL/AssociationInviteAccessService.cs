using GrotixBackend.Contracts.Auth.Identity;
using GrotixBackend.Contracts.Auth.Roles;
using GrotixBackend.Contracts.Profiles.Invites;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Profiles.Domain.Security;

namespace GrotixBackend.Profiles.Application.ACL;

public sealed class AssociationInviteAccessService(
    IAssociationInviteRepository inviteRepository) : IAssociationInviteAccessService
{
    public async Task<ValidatedAssociationInvite> ValidateForRegistrationAsync(
        string inviteToken,
        string email,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(inviteToken))
            throw new ApplicationException("Se requiere un token de invitación válido.");

        var tokenHash = InviteTokenHasher.Hash(inviteToken);
        var invite = await inviteRepository.GetByTokenHashAsync(tokenHash, cancellationToken)
                     ?? throw new ApplicationException("Invitación no válida o desconocida.");

        var normalizedEmail = AuthEmail.Normalize(email);
        if (!string.Equals(normalizedEmail, invite.InviteEmail, StringComparison.OrdinalIgnoreCase))
            throw new ApplicationException("El correo debe coincidir exactamente con el de la invitación.");

        if (invite.UsedAt != null)
            throw new ApplicationException("Esta invitación ya fue utilizada.");

        if (invite.ExpiresAt.HasValue && invite.ExpiresAt.Value < DateTime.UtcNow)
            throw new ApplicationException("La invitación ha expirado.");

        if (!KnownRoleIds.IsAssociationUserRole(invite.RoleId))
            throw new InvalidOperationException("La invitación no tiene un rol válido.");

        return new ValidatedAssociationInvite(
            invite.Id,
            invite.AssociationId,
            invite.RoleId,
            invite.InviteEmail);
    }

    public Task<bool> TryMarkUsedAsync(int inviteId, CancellationToken cancellationToken = default) =>
        inviteRepository.TryMarkUsedAsync(inviteId, cancellationToken);
}
