namespace GrotixBackend.Contracts.Integration.Telemetry;

/// <summary>Paquete de telemetría completo capturado en el Edge y publicado al broker.</summary>
public sealed record TelemetryReceivedIntegrationEvent(
    int DeviceId,
    int ZoneId,
    double Temperature,
    double HumidityAir,
    double HumiditySoil,
    double LightIntensity,
    DateTime Timestamp);
