using System.Security.Claims;

namespace GrotixBackend.Contracts.Auth.Claims;

public static class JwtClaimTypes
{
    public const string IdentityId = ClaimTypes.Sid;
    public const string Permission = "permission";
}
