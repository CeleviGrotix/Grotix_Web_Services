namespace GrotixBackend.HardwareDevice.Domain.Model.Aggregates;

/// <summary>Dispositivo edge (tabla <c>microcontroller</c>).</summary>
public class Microcontroller
{
    public int Id { get; private set; }
    public int? ZoneId { get; private set; }
    public string Model { get; private set; } = null!;
    public string MacAddress { get; private set; } = null!;
    public string Status { get; private set; } = null!;
    public DateTime? LastSeen { get; private set; }
    public int? BatteryLevel { get; private set; }
    public int? SignalStrength { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected Microcontroller() { }

    public Microcontroller(string model, string macAddress, int? zoneId = null)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model no puede estar vacío.");
        if (string.IsNullOrWhiteSpace(macAddress))
            throw new ArgumentException("MacAddress no puede estar vacío.");

        Model = model.Trim();
        MacAddress = macAddress.Trim().ToUpperInvariant();
        ZoneId = zoneId;
        Status = ValueObjects.DeviceStatuses.Offline;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string? model, string? macAddress, int? zoneId)
    {
        if (!string.IsNullOrWhiteSpace(model))
            Model = model.Trim();
        if (!string.IsNullOrWhiteSpace(macAddress))
            MacAddress = macAddress.Trim().ToUpperInvariant();
        if (zoneId.HasValue)
            ZoneId = zoneId;
    }

    public void LinkToZone(int zoneId)
    {
        if (zoneId <= 0)
            throw new ArgumentException("ZoneId inválido.");
        ZoneId = zoneId;
    }

    public void UnlinkFromZone() => ZoneId = null;

    public void UpdateStatus(string status, DateTime? lastSeen = null)
    {
        Status = ValueObjects.DeviceStatuses.Normalize(status);
        if (lastSeen.HasValue)
            LastSeen = lastSeen;
    }

    public void UpdateTelemetryMeta(int? batteryLevel, int? signalStrength)
    {
        BatteryLevel = batteryLevel;
        SignalStrength = signalStrength;
    }
}
