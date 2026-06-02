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

        var latestReading = await sensorReadingRepository.GetLatestByDeviceAsync(deviceId);

        var readings = sensors
            .Select(sensor =>
            {
                var value = MapSensorValue(latestReading, sensor.Type);
                return value.HasValue
                    ? new SensorReadingSnapshot(sensor.Id, sensor.Type, value.Value, sensor.Unit)
                    : null;
            })
            .Where(r => r != null)
            .Select(r => r!)
            .ToList();

        return new DeviceTelemetrySnapshot(
            device.Id,
            DateTime.UtcNow,
            readings,
            device.BatteryLevel,
            device.SignalStrength);
    }

    public async Task<ZoneHealthResult> GetZoneHealthAsync(int zoneId)
    {
        var devices = await deviceRepository.ListByZoneAsync(zoneId);

        var deviceHealthList = devices
            .Select(d => new ZoneDeviceHealth(
                d.Id,
                d.Model,
                d.Status,
                d.LastSeen,
                d.Status == Domain.Model.ValueObjects.DeviceStatuses.Online))
            .ToList();

        var allActive = deviceHealthList.Count > 0 && deviceHealthList.All(d => d.IsActive);

        return new ZoneHealthResult(zoneId, allActive, deviceHealthList.Count, deviceHealthList);
    }

    private static double? MapSensorValue(
        Telemetry.Domain.Model.Entities.SensorReading? reading, string sensorType) =>
        reading == null ? null : sensorType.ToUpperInvariant() switch
        {
            Telemetry.Domain.Model.ValueObjects.SensorTypes.AirTemperature => reading.Temperature,
            Telemetry.Domain.Model.ValueObjects.SensorTypes.AirHumidity    => reading.HumidityAir,
            Telemetry.Domain.Model.ValueObjects.SensorTypes.SoilMoisture   => reading.HumiditySoil,
            Telemetry.Domain.Model.ValueObjects.SensorTypes.LightIntensity => reading.LightIntensity,
            _ => null
        };
}
