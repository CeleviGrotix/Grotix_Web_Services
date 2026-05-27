namespace GrotixBackend.Contracts.Auth.Roles;

public static class KnownRoleIds
{
    public const int Admin = 1;
    public const int Staff = 2;
    public const int UserAdmin = 3;
    public const int UserBasic = 4;
    public const int UserAdvanced = 5;

    public static bool IsSystemRole(int roleId) =>
        roleId is Admin or Staff;

    public static bool IsAssociationUserRole(int roleId) =>
        roleId is UserAdmin or UserBasic or UserAdvanced;
}
