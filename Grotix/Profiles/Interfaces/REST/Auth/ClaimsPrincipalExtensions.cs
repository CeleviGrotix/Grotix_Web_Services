using System.Security.Claims;

namespace GrotixBackend.Profiles.Interfaces.REST.Auth;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Mismo tipo que <c>TokenService.PermissionClaimType</c> en IAM (JWT al iniciar sesión).</summary>
    public const string PermissionClaimType = "permission";

    /// <summary>IdentityID del token JWT (claim <see cref="ClaimTypes.Sid"/>).</summary>
    public static int? GetIdentityId(this ClaimsPrincipal user)
    {
        var v = user.FindFirst(ClaimTypes.Sid)?.Value;
        return int.TryParse(v, out var id) ? id : null;
    }

    /// <summary>Comprueba el claim <see cref="PermissionClaimType"/>.</summary>
    public static bool HasPermission(this ClaimsPrincipal user, string permissionCode)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
            return false;
        var expected = permissionCode.Trim().ToUpperInvariant();
        return user.FindAll(PermissionClaimType)
            .Any(c => string.Equals(c.Value, expected, StringComparison.Ordinal));
    }
}
