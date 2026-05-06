namespace GrotixBackend.Profiles.Domain.Model.Aggregates;

/// <summary>Entidad del informe Profile: rol con permisos agregados vía N:N (<c>role_permission</c>).</summary>
public class Role
{
    public int Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    public ICollection<Permission> Permissions { get; private set; } = new List<Permission>();

    protected Role() { }

    public Role(string name, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del rol no puede estar vacío.");
        Name = name.Trim();
        Description = description;
    }
}
