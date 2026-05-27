namespace GrotixBackend.Contracts.Profiles.Invites;

public interface IAssociationInviteAccessService
{
    Task<ValidatedAssociationInvite> ValidateForRegistrationAsync(
        string inviteToken,
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> TryMarkUsedAsync(int inviteId, CancellationToken cancellationToken = default);
}
