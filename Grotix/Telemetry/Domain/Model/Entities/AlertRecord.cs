namespace GrotixBackend.Telemetry.Domain.Model.Entities;

/// <summary>Registro de alerta publicada (tabla <c>alert_log</c>).</summary>
public class AlertRecord
{
    public int Id { get; set; }
    public int ZoneId { get; set; }
    public int SensorId { get; set; }
    public string SensorType { get; set; } = null!;
    public double Value { get; set; }
    public double MinThreshold { get; set; }
    public double MaxThreshold { get; set; }
    public double BreachedThreshold { get; set; }
    public string BreachDirection { get; set; } = null!;
    public DateTime TriggeredAt { get; set; }
}
