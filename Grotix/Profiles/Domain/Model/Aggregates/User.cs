// Profiles/Domain/Model/Aggregates/User.cs — raíz de agregado UserAggregate (informe Profile).
namespace GrotixBackend.Profiles.Domain.Model.Aggregates;

using GrotixBackend.Profiles.Domain.Model.ValueObjects;

/// <summary>
/// Raíz de agregado <c>UserAggregate</c> del informe: perfil de negocio, vínculo a identidad,
/// rol, asociación agraria y preferencias de notificación.
/// </summary>
public class User
{
    public int Id { get; private set; }
    public int IdentityId { get; private set; }
    public string? Name { get; private set; }
    public UserEmail Email { get; private set; } = null!;
    public string? TaxId { get; private set; }
    public string? Phone { get; private set; }
    public int RoleId { get; private set; }
    public int? AssociationId { get; private set; }
    public string? ProfilePicture { get; private set; }
    /// <summary>JSON en BD (columna Preferences); usar <see cref="GetPreferences"/> / <see cref="UpdatePreferences"/>.</summary>
    public string? PreferencesJson { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    protected User() { }

    public User(
        int identityId,
        UserEmail email,
        int roleId = 3,
        string? name = null,
        string? taxId = null,
        string? phone = null,
        int? associationId = null,
        string? profilePicture = null,
        UserPreferences? preferences = null)
    {
        IdentityId = identityId;
        Email = email;
        RoleId = roleId;
        Name = name;
        TaxId = taxId;
        Phone = NormalizePhoneOrThrow(phone);
        AssociationId = associationId;
        ProfilePicture = profilePicture;
        PreferencesJson = preferences?.ToJson();
    }

    public UserPreferences GetPreferences() =>
        UserPreferences.FromJson(PreferencesJson);

    public void UpdatePreferences(UserPreferences preferences) =>
        PreferencesJson = preferences.ToJson();

    public void UpdateProfile(string? name, string? taxId, string? phone, string? profilePicture)
    {
        Name = name;
        TaxId = taxId;
        Phone = NormalizePhoneOrThrow(phone);
        ProfilePicture = profilePicture;
    }

    public void AssignRole(int roleId)
    {
        if (roleId <= 0)
            throw new ArgumentException("RoleId inválido.");
        RoleId = roleId;
    }

    public void AssignAssociation(int? associationId) => AssociationId = associationId;

    private static string? NormalizePhoneOrThrow(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return null;
        return UserPhone.Create(phone).Value;
    }
}
