namespace GrotixBackend.CultivationArea.Domain.Model.ValueObjects;

public static class IrrigationModes
{
    public const string Manual = "MANUAL";
    public const string Automatic = "AUTOMATIC";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Manual,
        Automatic
    };

    public static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? Automatic : value.Trim().ToUpperInvariant();

    public static bool IsValid(string? value) =>
        All.Contains(Normalize(value));

    public static bool IsAutomatic(string? value) =>
        string.Equals(Normalize(value), Automatic, StringComparison.Ordinal);

    public static string AllowedValuesLabel() =>
        string.Join(", ", All.OrderBy(v => v, StringComparer.Ordinal));
}
