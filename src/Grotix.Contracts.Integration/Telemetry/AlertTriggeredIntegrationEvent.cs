namespace GrotixBackend.Contracts.Integration.Telemetry;

/// <summary>Alerta crítica sostenida tras superar umbrales de zona.</summary>
public sealed record AlertTriggeredIntegrationEvent(
    int ZoneId,
    int SensorId,
    string SensorType,
    double Value,
    double Threshold,
    double MinThreshold,
    double MaxThreshold,
    string BreachDirection,
    DateTime Timestamp);
