using System.Security.Claims;

namespace GrotixBackend.Profiles.Interfaces.REST.Auth;

public static class ClaimsPrincipalExtensions
{
    /// <summary>IdentityID del token JWT (claim <see cref="ClaimTypes.Sid"/>).</summary>
    public static int? GetIdentityId(this ClaimsPrincipal user)
    {
        var v = user.FindFirst(ClaimTypes.Sid)?.Value;
        return int.TryParse(v, out var id) ? id : null;
    }
}
