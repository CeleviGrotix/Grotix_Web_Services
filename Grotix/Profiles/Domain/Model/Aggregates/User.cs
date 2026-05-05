namespace GrotixBackend.Profiles.Domain.Model.Aggregates;

public class User
{
    public int Id { get; private set; }
    public int IdentityId { get; private set; } // FK hacia la tabla de IAM
    public string? Name { get; private set; }
    public string? TaxId { get; private set; }
    public string Email { get; private set; }
    public string? Phone { get; private set; }
    public int RoleId { get; private set; }
    public string Preferences { get; private set; } // JSON string
    public int AssociationId { get; private set; }
    public string ProfilePicture { get; private set; }

    protected User() { } // Requerido por EF Core

    public User(int identityId, string? name, string email, string? taxId, string? phone)
    {
        this.IdentityId = identityId;
        this.Name = name;
        this.Email = email;
        this.RoleId = 1;
        this.TaxId = taxId;
        this.Phone = phone;
    }
}