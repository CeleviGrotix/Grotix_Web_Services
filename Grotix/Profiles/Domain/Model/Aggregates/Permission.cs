namespace GrotixBackend.Profiles.Domain.Model.Aggregates;

using GrotixBackend.Profiles.Domain.Model.ValueObjects;

/// <summary>Entidad del informe Profile: permiso vinculable a roles (tabla <c>permission</c>).</summary>
public class Permission
{
    public int Id { get; private set; }
    public string Code { get; private set; } = null!;
    public string? Description { get; private set; }

    public ICollection<Role> Roles { get; private set; } = new List<Role>();

    protected Permission() { }

    public Permission(PermissionCode code, string? description = null)
    {
        Code = code.Value;
        Description = description;
    }
}
