namespace GrotixBackend.Profiles.Domain.Model.ValueObjects;

/// <summary>Coincide con los IDs sembrados en la tabla <c>role</c> (Admin=1, Staff=2, User=3).</summary>
public enum RoleType { Admin = 1, Staff = 2, User = 3 }

public class UserRole : IEquatable<UserRole>
{
    public int RoleId { get; private set; }
    public string Name { get; private set; }

    private UserRole() { }

    private UserRole(RoleType role)
    {
        RoleId = (int)role;
        Name = role.ToString();
    }

    public static UserRole Default() => new(RoleType.User);
    public static UserRole Admin() => new(RoleType.Admin);
    public static UserRole Staff() => new(RoleType.Staff);
    public static UserRole From(int roleId) =>
        Enum.IsDefined(typeof(RoleType), roleId)
            ? new UserRole((RoleType)roleId)
            : throw new ArgumentException($"RoleId '{roleId}' no válido.");

    public bool Equals(UserRole? other) => other is not null && RoleId == other.RoleId;
    public override bool Equals(object? obj) => obj is UserRole r && Equals(r);
    public override int GetHashCode() => RoleId.GetHashCode();
    public override string ToString() => Name;
}