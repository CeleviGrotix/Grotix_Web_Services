namespace GrotixBackend.Contracts.Profiles.Provisioning;

public interface IAdminProfileRegistrationService
{
    Task<int> CreateUserAndReturnIdAsync(
        AdminCreateProfileUserRequest request,
        CancellationToken cancellationToken = default);
}
