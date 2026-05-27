namespace GrotixBackend.Contracts.Profiles.Provisioning;

public interface IExternalProfileService
{
    Task<bool> ExistsForIdentityAsync(int identityId, CancellationToken cancellationToken = default);

    Task<int> CreateUserAndReturnId(CreateProfileUserRequest request);
}
