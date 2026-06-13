using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;

namespace GrotixBackend.HardwareDevice.Application.Internal;

public interface IDeviceCommandService
{
    Task<Microcontroller> RegisterAsync(RegisterDeviceRequest request, CancellationToken cancellationToken = default);

    Task UpdateAsync(int deviceId, UpdateDeviceRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int deviceId, CancellationToken cancellationToken = default);

    Task LinkToZoneAsync(int deviceId, int zoneId, CancellationToken cancellationToken = default);

    Task UnlinkFromZoneAsync(int deviceId, int zoneId, CancellationToken cancellationToken = default);

    Task UpdateStatusAsync(
        int deviceId,
        UpdateDeviceStatusRequest request,
        CancellationToken cancellationToken = default);

    Task<DeviceSensor> AddSensorAsync(
        int deviceId,
        RegisterSensorRequest request,
        CancellationToken cancellationToken = default);

    Task<DeviceActuator> AddActuatorAsync(
        int deviceId,
        RegisterActuatorRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteSensorAsync(int deviceId, int sensorId, CancellationToken cancellationToken = default);

    Task DeleteActuatorAsync(int deviceId, int actuatorId, CancellationToken cancellationToken = default);
}

public sealed record RegisterDeviceRequest(
    int? ZoneId,
    string Model,
    string MacAddress,
    IReadOnlyList<RegisterSensorRequest>? Sensors,
    IReadOnlyList<RegisterActuatorRequest>? Actuators = null);

public sealed record RegisterSensorRequest(
    string Model,
    string Type,
    string Unit,
    int Pin,
    double? MinPhysical,
    double? MaxPhysical);

public sealed record RegisterActuatorRequest(string Type, int Pin);

public sealed record UpdateDeviceRequest(int? ZoneId, string? Model, string? MacAddress);

public sealed record UpdateDeviceStatusRequest(string Status, DateTime? LastSeen);
