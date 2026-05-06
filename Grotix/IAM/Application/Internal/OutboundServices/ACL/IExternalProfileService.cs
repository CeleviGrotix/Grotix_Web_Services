namespace GrotixBackend.IAM.Application.Internal.OutboundServices.ACL;

public interface IExternalProfileService
{
    /// <param name="roleId">Debe existir en la tabla <c>role</c> (p. ej. User=3, Admin=1).</param>
    Task<int> CreateUserAndReturnId(int identityId, string username, int roleId = 3);
}