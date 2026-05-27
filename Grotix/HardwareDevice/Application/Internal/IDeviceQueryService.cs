using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;

namespace GrotixBackend.HardwareDevice.Application.Internal;

public interface IDeviceQueryService
{
    Task<IReadOnlyList<Microcontroller>> ListAsync(string? status, int? zoneId);

    Task<DeviceDetail?> GetDetailAsync(int deviceId);

    Task<ZoneLinkInfo?> GetZoneLinkAsync(int deviceId);

    Task<IReadOnlyList<ZoneDeviceSummary>> ListByZoneAsync(int zoneId);

    Task<DeviceStatusInfo?> GetStatusAsync(int deviceId);

    Task<DeviceTelemetrySnapshot?> GetTelemetryAsync(int deviceId, IReadOnlyList<string>? sensorTypes);
}

public sealed record DeviceDetail(
    Microcontroller Device,
    IReadOnlyList<DeviceSensor> Sensors,
    IReadOnlyList<DeviceActuator> Actuators);

public sealed record ZoneLinkInfo(int ZoneId, string? ZoneName, string? CropName);

public sealed record ZoneDeviceSummary(
    int DeviceId,
    string Model,
    string MacAddress,
    string Status,
    DateTime? LastSeen,
    int SensorCount,
    int ActuatorCount);

public sealed record DeviceStatusInfo(int DeviceId, string Status, DateTime? LastSeen, int? UptimeSeconds);

public sealed record DeviceTelemetrySnapshot(
    int DeviceId,
    DateTime Timestamp,
    IReadOnlyList<SensorReadingSnapshot> Readings,
    int? BatteryLevel,
    int? SignalStrength);

public sealed record SensorReadingSnapshot(int SensorId, string Type, double Value, string Unit);
