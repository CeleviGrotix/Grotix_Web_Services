namespace GrotixBackend.Profiles.Domain.Model;

using GrotixBackend.Profiles.Domain.Model.ValueObjects;

/// <summary>Roles considerados agricultores / organización (excluye <c>admin</c> y <c>staff</c> del sistema).</summary>
public static class FarmerRoles
{
    public static readonly int[] Ids =
    [
        (int)RoleType.user_admin,
        (int)RoleType.user_basic,
        (int)RoleType.user_advanced
    ];

    public static bool IsFarmerRole(int roleId) =>
        roleId is (int)RoleType.user_admin or (int)RoleType.user_basic or (int)RoleType.user_advanced;
}
