namespace GrotixBackend.Profiles.Domain.Model.Aggregates;

/// <summary>Filas N:N entre <c>role</c> y <c>permission</c> (tabla <c>role_permission</c>).</summary>
/// <remarks>
/// EF Core exige navegaciones hacia ambos extremos para configurar <c>UsingEntity&lt;TJoin&gt;</c> sin errores de compilación.
/// </remarks>
public class RolePermissionLink
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }

    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
