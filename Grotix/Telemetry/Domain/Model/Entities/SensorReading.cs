namespace GrotixBackend.Telemetry.Domain.Model.Entities;

/// <summary>Lectura de serie temporal (tabla <c>sensor_reading</c>).</summary>
public class SensorReading
{
    public int SensorId { get; set; }
    public double Value { get; set; }
    public DateTime Timestamp { get; set; }
}
