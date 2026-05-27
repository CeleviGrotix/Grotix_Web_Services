namespace GrotixBackend.Telemetry.Application.Internal;

public interface ITelemetryQueryService
{
    Task<ZoneTelemetryHistory?> GetZoneHistoryAsync(
        int zoneId,
        DateTime? startTime,
        DateTime? endTime,
        IReadOnlyList<string>? sensorTypes,
        int limit,
        CancellationToken cancellationToken = default);
}

public sealed record ZoneTelemetryHistory(
    int ZoneId,
    DateTime Start,
    DateTime End,
    IReadOnlyList<SensorTelemetrySeries> Sensors);

public sealed record SensorTelemetrySeries(
    int SensorId,
    string Type,
    string Unit,
    IReadOnlyList<SensorReadingPoint> Readings);

public sealed record SensorReadingPoint(double Value, DateTime Timestamp);
