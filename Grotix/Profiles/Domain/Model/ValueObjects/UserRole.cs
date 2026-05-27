namespace GrotixBackend.Profiles.Domain.Model.ValueObjects;

/// <summary>Coincide con los IDs en la tabla <c>role</c> tras la migración de roles.</summary>
public enum RoleType
{
    admin = 1,
    staff = 2,
    user_admin = 3,
    user_basic = 4,
    user_advanced = 5
}

public class UserRole : IEquatable<UserRole>
{
    public int RoleId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private UserRole() { }

    private UserRole(RoleType role)
    {
        RoleId = (int)role;
        Name = role.ToString();
    }

    public static UserRole Default() => new(RoleType.user_basic);
    public static UserRole Admin() => new(RoleType.admin);
    public static UserRole Staff() => new(RoleType.staff);
    public static UserRole From(int roleId) =>
        Enum.IsDefined(typeof(RoleType), roleId)
            ? new UserRole((RoleType)roleId)
            : throw new ArgumentException($"RoleId '{roleId}' no válido.");

    public bool Equals(UserRole? other) => other is not null && RoleId == other.RoleId;
    public override bool Equals(object? obj) => obj is UserRole r && Equals(r);
    public override int GetHashCode() => RoleId.GetHashCode();
    public override string ToString() => Name;
}