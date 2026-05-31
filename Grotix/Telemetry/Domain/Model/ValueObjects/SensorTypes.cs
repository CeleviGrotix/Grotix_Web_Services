namespace GrotixBackend.Telemetry.Domain.Model.ValueObjects;

public static class SensorTypes
{
    public const string SoilMoisture = "SOIL_MOISTURE";
    public const string AirTemperature = "AIR_TEMPERATURE";
    public const string AirHumidity = "AIR_HUMIDITY";
    public const string LightIntensity = "LIGHT_INTENSITY";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        SoilMoisture,
        AirTemperature,
        AirHumidity,
        LightIntensity
    };

    public static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().ToUpperInvariant();
}
