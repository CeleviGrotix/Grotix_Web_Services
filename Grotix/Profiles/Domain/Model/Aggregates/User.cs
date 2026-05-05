// Profiles/Domain/Model/Aggregates/User.cs
namespace GrotixBackend.Profiles.Domain.Model.Aggregates;

public class User
{
    public int Id { get; private set; }
    public int IdentityId { get; private set; }
    public string? Name { get; private set; }
    public string Email { get; private set; } = null!;
    public string? TaxId { get; private set; }
    public string? Phone { get; private set; }
    public int RoleId { get; private set; }

    protected User() { }

    public User(int identityId, string email, int roleId = 3, string? name = null, string? taxId = null, string? phone = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email no puede estar vacío.");

        IdentityId = identityId;
        Email = email;
        RoleId = roleId;
        Name = name;
        TaxId = taxId;
        Phone = phone;
    }

    public void UpdateProfile(string? name, string? taxId, string? phone)
    {
        Name = name;
        TaxId = taxId;
        Phone = phone;
    }
}