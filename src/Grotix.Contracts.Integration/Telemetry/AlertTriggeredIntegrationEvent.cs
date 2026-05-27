namespace GrotixBackend.Contracts.Integration.Telemetry;

/// <summary>Alerta crítica sostenida tras superar umbrales de zona.</summary>
public sealed record AlertTriggeredIntegrationEvent(
    int ZoneId,
    int SensorId,
    double Value,
    double Threshold,
    DateTime Timestamp);
