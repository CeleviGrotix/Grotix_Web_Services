namespace GrotixBackend.Contracts.Auth.Admin;

public interface IAdminIdentityRegistrationService
{
    Task<AdminRegisterUserResponse> RegisterAsync(
        AdminRegisterUserRequest request,
        CancellationToken cancellationToken = default);
}
