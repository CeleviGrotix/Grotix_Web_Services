using GrotixBackend.Telemetry.Domain.Model.ValueObjects;

namespace GrotixBackend.HardwareDevice.Domain.Model.ValueObjects;

/// <summary>Catálogo de modelos físicos de sensor y los tipos de lectura que pueden exponer.</summary>
public static class SensorModels
{
    public const string Generic = "GENERIC";
    public const string Dht22 = "DHT22";
    public const string Dht11 = "DHT11";
    public const string Ds18b20 = "DS18B20";
    public const string CapacitiveSoil = "CAPACITIVE_SOIL";
    public const string Bh1750 = "BH1750";
    public const string Ldr = "LDR";

    private static readonly Dictionary<string, IReadOnlySet<string>> Catalog =
        new(StringComparer.OrdinalIgnoreCase)
        {
            [Generic] = SensorTypes.All,
            [Dht22] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                SensorTypes.AirTemperature,
                SensorTypes.AirHumidity
            },
            [Dht11] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                SensorTypes.AirTemperature,
                SensorTypes.AirHumidity
            },
            [Ds18b20] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                SensorTypes.AirTemperature
            },
            [CapacitiveSoil] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                SensorTypes.SoilMoisture
            },
            [Bh1750] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                SensorTypes.LightIntensity
            },
            [Ldr] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                SensorTypes.LightIntensity
            }
        };

    public static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? Generic : value.Trim().ToUpperInvariant();

    public static bool IsKnown(string? value) =>
        Catalog.ContainsKey(Normalize(value));

    public static bool IsTypeAllowed(string model, string type)
    {
        var normalizedModel = Normalize(model);
        var normalizedType = SensorTypes.Normalize(type);
        return Catalog.TryGetValue(normalizedModel, out var allowed)
               && allowed.Contains(normalizedType);
    }

    /// <summary>Modelos multi-canal (p. ej. DHT22) pueden registrar varios <c>type</c> en el mismo pin.</summary>
    public static bool AllowsMultipleTypesOnSamePin(string model) =>
        Catalog.TryGetValue(Normalize(model), out var allowed) && allowed.Count > 1;

    public static string AllowedModelsLabel() =>
        string.Join(", ", Catalog.Keys.OrderBy(k => k, StringComparer.Ordinal));

    public static IReadOnlyList<SensorModelOption> ListOptions() =>
        Catalog
            .OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
            .Select(kvp => new SensorModelOption(
                kvp.Key,
                kvp.Value.OrderBy(t => t, StringComparer.Ordinal).ToList(),
                kvp.Value.Count > 1))
            .ToList();
}

public sealed record SensorModelOption(
    string Model,
    IReadOnlyList<string> Types,
    bool AllowsMultipleTypesOnSamePin);
