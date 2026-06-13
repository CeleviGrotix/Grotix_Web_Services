using GrotixBackend.HardwareDevice.Domain.Model.ValueObjects;
using GrotixBackend.Telemetry.Domain.Model.ValueObjects;

namespace GrotixBackend.HardwareDevice.Domain.Model.Aggregates;

/// <summary>Sensor físico asociado a un microcontrolador (tabla <c>sensor</c> en Core DB).</summary>
public class DeviceSensor
{
    public int Id { get; private set; }
    public int MicrocontrollerId { get; private set; }
    public int? ZoneId { get; private set; }
    public string Model { get; private set; } = null!;
    public string Type { get; private set; } = null!;
    public string Unit { get; private set; } = null!;
    public int Pin { get; private set; }
    public string Status { get; private set; } = "NORMAL";
    public DateTime? LastSeen { get; private set; }
    public double? MinPhysical { get; private set; }
    public double? MaxPhysical { get; private set; }

    protected DeviceSensor() { }

    public DeviceSensor(
        int microcontrollerId,
        string model,
        string type,
        string unit,
        int pin,
        int? zoneId = null,
        double? minPhysical = null,
        double? maxPhysical = null)
    {
        if (microcontrollerId <= 0)
            throw new ArgumentException("MicrocontrollerId inválido.");
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type requerido.");
        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Unit requerido.");

        Model = SensorModels.Normalize(model);
        if (!SensorModels.IsKnown(Model))
            throw new ArgumentException($"Model de sensor inválido. Valores: {SensorModels.AllowedModelsLabel()}.");

        Type = SensorTypes.Normalize(type);
        if (!SensorTypes.All.Contains(Type))
            throw new ArgumentException(
                $"Type de sensor inválido. Valores: {string.Join(", ", SensorTypes.All.OrderBy(t => t))}.");
        if (!SensorModels.IsTypeAllowed(Model, Type))
            throw new ArgumentException($"El modelo {Model} no soporta el type {Type}.");

        MicrocontrollerId = microcontrollerId;
        Unit = unit.Trim();
        Pin = pin;
        ZoneId = zoneId;
        MinPhysical = minPhysical;
        MaxPhysical = maxPhysical;
    }

    public void AssignZone(int? zoneId) => ZoneId = zoneId;

    public void TouchLastSeen(DateTime? at = null) => LastSeen = at ?? DateTime.UtcNow;
}
