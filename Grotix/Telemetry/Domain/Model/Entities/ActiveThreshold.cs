namespace GrotixBackend.Telemetry.Domain.Model.Entities;

/// <summary>Override de umbrales por zona (tabla <c>active_thresholds</c>).</summary>
public class ActiveThreshold
{
    public int ZoneId { get; set; }
    public string SensorType { get; set; } = null!;
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
}
