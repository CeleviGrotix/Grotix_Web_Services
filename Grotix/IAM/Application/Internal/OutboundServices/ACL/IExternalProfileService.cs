namespace GrotixBackend.IAM.Application.Internal.OutboundServices.ACL;

public interface IExternalProfileService
{
    /// <param name="roleId">Debe existir en la tabla <c>role</c> (p. ej. user_basic=4, admin=1).</param>
    Task<int> CreateUserAndReturnId(int identityId, string username, int roleId = 4);
}