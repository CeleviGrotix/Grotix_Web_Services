namespace GrotixBackend.Contracts.Profiles.Provisioning;

public interface IExternalProfileService
{
    Task<int> CreateUserAndReturnId(CreateProfileUserRequest request);
}
