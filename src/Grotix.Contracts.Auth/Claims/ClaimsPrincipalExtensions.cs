using System.Security.Claims;

namespace GrotixBackend.Contracts.Auth.Claims;

public static class ClaimsPrincipalExtensions
{
    public static int? GetIdentityId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(JwtClaimTypes.IdentityId)?.Value;
        return int.TryParse(value, out var id) ? id : null;
    }

    public static bool HasPermission(this ClaimsPrincipal user, string permissionCode)
    {
        if (string.IsNullOrWhiteSpace(permissionCode))
            return false;

        var expected = permissionCode.Trim().ToUpperInvariant();
        return user.FindAll(JwtClaimTypes.Permission)
            .Any(claim => string.Equals(claim.Value, expected, StringComparison.Ordinal));
    }
}
