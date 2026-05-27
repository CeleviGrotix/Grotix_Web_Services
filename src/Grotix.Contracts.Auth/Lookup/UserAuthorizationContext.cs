namespace GrotixBackend.Contracts.Auth.Lookup;

public sealed record UserAuthorizationContext(
    bool IsActive,
    IReadOnlyList<string> RoleNames,
    IReadOnlyList<string> PermissionCodes);
