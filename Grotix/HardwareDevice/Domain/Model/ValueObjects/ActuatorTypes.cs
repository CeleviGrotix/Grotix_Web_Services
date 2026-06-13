namespace GrotixBackend.HardwareDevice.Domain.Model.ValueObjects;

public static class ActuatorTypes
{
    public const string Valve = "VALVE";
    public const string Pump = "PUMP";
    public const string Irrigation = "IRRIGATION";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Valve,
        Pump,
        Irrigation
    };

    public static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();

    public static bool IsValid(string? value) =>
        All.Contains(Normalize(value));

    public static string AllowedValuesLabel() =>
        string.Join(", ", All.OrderBy(v => v, StringComparer.Ordinal));
}
