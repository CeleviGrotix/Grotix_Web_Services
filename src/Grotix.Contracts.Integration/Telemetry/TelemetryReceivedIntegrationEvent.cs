namespace GrotixBackend.Contracts.Integration.Telemetry;

/// <summary>Lectura capturada en el Edge y publicada al broker.</summary>
public sealed record TelemetryReceivedIntegrationEvent(
    int DeviceId,
    int SensorId,
    double Value,
    DateTime Timestamp);
