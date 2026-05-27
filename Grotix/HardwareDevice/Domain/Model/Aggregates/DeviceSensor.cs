namespace GrotixBackend.HardwareDevice.Domain.Model.Aggregates;

/// <summary>Sensor físico asociado a un microcontrolador (tabla <c>sensor</c> en Core DB).</summary>
public class DeviceSensor
{
    public int Id { get; private set; }
    public int MicrocontrollerId { get; private set; }
    public int? ZoneId { get; private set; }
    public string Type { get; private set; } = null!;
    public string Unit { get; private set; } = null!;
    public int Pin { get; private set; }
    public string Status { get; private set; } = "NORMAL";
    public double? MinPhysical { get; private set; }
    public double? MaxPhysical { get; private set; }

    protected DeviceSensor() { }

    public DeviceSensor(
        int microcontrollerId,
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

        MicrocontrollerId = microcontrollerId;
        Type = type.Trim().ToUpperInvariant();
        Unit = unit.Trim();
        Pin = pin;
        ZoneId = zoneId;
        MinPhysical = minPhysical;
        MaxPhysical = maxPhysical;
    }

    public void AssignZone(int? zoneId) => ZoneId = zoneId;
}
