namespace GrotixBackend.Telemetry.Domain.Model.Entities;

/// <summary>Estado de violaciones consecutivas para publicar alertas.</summary>
public class ThresholdBreachTracker
{
    public int ZoneId { get; set; }
    public int SensorId { get; set; }
    public string SensorType { get; set; } = null!;
    public int ConsecutiveCount { get; set; }
    public DateTime? LastEvaluatedAt { get; set; }
}
