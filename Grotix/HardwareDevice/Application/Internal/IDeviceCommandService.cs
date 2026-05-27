using GrotixBackend.HardwareDevice.Domain.Model.Aggregates;

namespace GrotixBackend.HardwareDevice.Application.Internal;

public interface IDeviceCommandService
{
    Task<Microcontroller> RegisterAsync(RegisterDeviceRequest request, CancellationToken cancellationToken = default);

    Task UpdateAsync(int deviceId, UpdateDeviceRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(int deviceId, CancellationToken cancellationToken = default);

    Task LinkToZoneAsync(int deviceId, int zoneId, CancellationToken cancellationToken = default);

    Task UnlinkFromZoneAsync(int deviceId, int zoneId, CancellationToken cancellationToken = default);
}

public sealed record RegisterDeviceRequest(
    int? ZoneId,
    string Model,
    string MacAddress,
    IReadOnlyList<RegisterSensorRequest>? Sensors);

public sealed record RegisterSensorRequest(
    string Type,
    string Unit,
    int Pin,
    double? MinPhysical,
    double? MaxPhysical);

public sealed record UpdateDeviceRequest(int? ZoneId, string? Model, string? MacAddress);
