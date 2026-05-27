namespace GrotixBackend.Telemetry.Domain.Model.Entities;

/// <summary>Catálogo de sensores en Timescale (tabla <c>sensor</c>).</summary>
public class Sensor
{
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public int ZoneId { get; set; }
    public string Type { get; set; } = null!;
    public string Unit { get; set; } = null!;
    public double? MinPhysical { get; set; }
    public double? MaxPhysical { get; set; }
}
