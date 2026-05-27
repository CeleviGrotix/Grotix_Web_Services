using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Domain.Model.Queries;
using GrotixBackend.HardwareDevice.Domain.Repositories;
using GrotixBackend.Telemetry.Domain.Repositories;

namespace GrotixBackend.HardwareDevice.Application.Internal;

public sealed class DeviceQueryService(
    IMicrocontrollerRepository deviceRepository,
    IDeviceSensorRepository sensorRepository,
    IDeviceActuatorRepository actuatorRepository,
    ISensorReadingRepository sensorReadingRepository,
    IZoneQueryService zoneQueryService,
    ICropQueryService cropQueryService) : IDeviceQueryService
{
    public Task<IReadOnlyList<Domain.Model.Aggregates.Microcontroller>> ListAsync(string? status, int? zoneId) =>
        deviceRepository.ListAsync(status, zoneId);

    public async Task<DeviceDetail?> GetDetailAsync(int deviceId)
    {
        var device = await deviceRepository.GetByIdAsync(deviceId);
        if (device == null)
            return null;

        var sensors = await sensorRepository.ListByDeviceAsync(deviceId);
        var actuators = await actuatorRepository.ListByDeviceAsync(deviceId);
        return new DeviceDetail(device, sensors, actuators);
    }

    public async Task<ZoneLinkInfo?> GetZoneLinkAsync(int deviceId)
    {
        var device = await deviceRepository.GetByIdAsync(deviceId);
        if (device?.ZoneId == null)
            return null;

        var zone = await zoneQueryService.Handle(new GetZoneByIdQuery(device.ZoneId.Value));
        if (zone == null)
            return new ZoneLinkInfo(device.ZoneId.Value, null, null);

        var crop = await cropQueryService.Handle(new GetCropByIdQuery(zone.CropId));
        return new ZoneLinkInfo(zone.Id, $"Zone {zone.Id}", crop?.CommonName);
    }

    public async Task<IReadOnlyList<ZoneDeviceSummary>> ListByZoneAsync(int zoneId)
    {
        var devices = await deviceRepository.ListByZoneAsync(zoneId);
        var result = new List<ZoneDeviceSummary>();
        foreach (var device in devices)
        {
            var sensorCount = await sensorRepository.CountByDeviceAsync(device.Id);
            var actuatorCount = await actuatorRepository.CountByDeviceAsync(device.Id);
            result.Add(new ZoneDeviceSummary(
                device.Id,
                device.Model,
                device.MacAddress,
                device.Status,
                device.LastSeen,
                sensorCount,
                actuatorCount));
        }

        return result;
    }

    public async Task<DeviceStatusInfo?> GetStatusAsync(int deviceId)
    {
        var device = await deviceRepository.GetByIdAsync(deviceId);
        if (device == null)
            return null;

        int? uptime = device.LastSeen.HasValue
            ? (int)Math.Max(0, (DateTime.UtcNow - device.LastSeen.Value).TotalSeconds)
            : null;

        return new DeviceStatusInfo(device.Id, device.Status, device.LastSeen, uptime);
    }

    public async Task<DeviceTelemetrySnapshot?> GetTelemetryAsync(int deviceId, IReadOnlyList<string>? sensorTypes)
    {
        var device = await deviceRepository.GetByIdAsync(deviceId);
        if (device == null)
            return null;

        var sensors = await sensorRepository.ListByDeviceAsync(deviceId);
        if (sensorTypes is { Count: > 0 })
        {
            var normalized = sensorTypes.Select(t => t.Trim().ToUpperInvariant()).ToHashSet();
            sensors = sensors.Where(s => normalized.Contains(s.Type)).ToList();
        }

        var readings = new List<SensorReadingSnapshot>();
        foreach (var sensor in sensors)
        {
            var latest = await sensorReadingRepository.ListBySensorAsync(
                sensor.Id,
                start: DateTime.UtcNow.AddDays(-7),
                end: null,
                limit: 1);

            if (latest.Count == 0)
                continue;

            readings.Add(new SensorReadingSnapshot(
                sensor.Id,
                sensor.Type,
                latest[0].Value,
                sensor.Unit));
        }

        return new DeviceTelemetrySnapshot(
            device.Id,
            DateTime.UtcNow,
            readings,
            device.BatteryLevel,
            device.SignalStrength);
    }
}
