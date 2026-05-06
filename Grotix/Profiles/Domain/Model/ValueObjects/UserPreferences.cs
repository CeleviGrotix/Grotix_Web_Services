using System.Text.Json;

namespace GrotixBackend.Profiles.Domain.Model.ValueObjects;

/// <summary>VO del informe Profile: encapsula el JSON de preferencias {"push": true, "email": false}.</summary>
public sealed class UserPreferences : IEquatable<UserPreferences>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public bool Push { get; }
    public bool Email { get; }

    public UserPreferences(bool push, bool email)
    {
        Push = push;
        Email = email;
    }

    public static UserPreferences FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new UserPreferences(false, false);
        try
        {
            var dto = JsonSerializer.Deserialize<PreferencesDto>(json, JsonOptions)
                      ?? throw new FormatException();
            return new UserPreferences(dto.Push, dto.Email);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("El JSON de preferencias no es válido.", ex);
        }
    }

    public string ToJson() => JsonSerializer.Serialize(new PreferencesDto(Push, Email), JsonOptions);

    public bool Equals(UserPreferences? other) =>
        other is not null && Push == other.Push && Email == other.Email;

    public override bool Equals(object? obj) => obj is UserPreferences p && Equals(p);
    public override int GetHashCode() => HashCode.Combine(Push, Email);

    private sealed record PreferencesDto(bool Push, bool Email);
}
