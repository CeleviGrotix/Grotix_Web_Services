namespace GrotixBackend.IAM.Application.Internal.OutboundServices.ACL;

public interface IExternalProfileService
{
    Task<int> CreateUserAndReturnId(int identityId, string username);
}