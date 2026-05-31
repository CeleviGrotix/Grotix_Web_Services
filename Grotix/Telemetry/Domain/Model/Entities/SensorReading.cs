namespace GrotixBackend.Telemetry.Domain.Model.Entities;

/// <summary>
/// Lectura completa de un ciclo de telemetría del ESP32 (tabla <c>sensor_reading</c>).
/// Un solo registro almacena las cuatro mediciones del dispositivo en un instante dado.
/// </summary>
public class SensorReading
{
    public int DeviceId { get; set; }
    public int ZoneId { get; set; }
    public DateTime Timestamp { get; set; }
    public double Temperature { get; set; }
    public double HumidityAir { get; set; }
    public double HumiditySoil { get; set; }
    public double LightIntensity { get; set; }
}
