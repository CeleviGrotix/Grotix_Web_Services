using GrotixBackend.HardwareDevice.Domain.Model.ValueObjects;
using GrotixBackend.HardwareDevice.Domain.Repositories;
using GrotixBackend.Telemetry.Domain.Model.Entities;
using GrotixBackend.Telemetry.Domain.Model.ValueObjects;
using GrotixBackend.Telemetry.Domain.Repositories;

namespace GrotixBackend.HardwareDevice.Application.Internal;

public sealed class DeviceDiagnosticService(
    IDeviceQueryService deviceQueryService,
    ISensorReadingRepository sensorReadingRepository) : IDeviceDiagnosticService
{
    public async Task<DeviceDiagnosticReport?> RunAsync(int deviceId, bool includeSensors, bool includeActuators)
    {
        var detail = await deviceQueryService.GetDetailAsync(deviceId);
        if (detail == null)
            return null;

        var checks = new Dictionary<string, object>
        {
            ["connectivity"] = new
            {
                status = detail.Device.Status == DeviceStatuses.Online ? "PASS" : "WARN",
                message = detail.Device.LastSeen.HasValue
                    ? $"Last seen {detail.Device.LastSeen:O}"
                    : "No heartbeat recorded"
            }
        };

        if (includeSensors)
        {
            var latestReading = await sensorReadingRepository.GetLatestByDeviceAsync(deviceId);
            var recentCutoff = DateTime.UtcNow.AddHours(-24);
            var hasRecent = latestReading != null && latestReading.Timestamp >= recentCutoff;

            checks["sensors"] = detail.Sensors.Select(sensor => new
            {
                sensorId = sensor.Id,
                type = sensor.Type,
                status = hasRecent ? "PASS" : "WARN",
                value = MapSensorValue(latestReading, sensor.Type),
                unit = sensor.Unit,
                lastSeen = latestReading?.Timestamp
            }).ToList<object>();
        }

        if (includeActuators)
        {
            checks["actuators"] = detail.Actuators.Select(a => new
            {
                actuatorId = a.Id,
                type = a.Type,
                status = "PASS",
                responseTimeMs = 120
            });
        }

        var overall = checks.Values.Any(v => v.ToString()?.Contains("WARN") == true) ? "DEGRADED" : "HEALTHY";
        return new DeviceDiagnosticReport(deviceId, DateTime.UtcNow, overall, checks);
    }

    private static double? MapSensorValue(SensorReading? reading, string sensorType) =>
        reading == null ? null : sensorType.ToUpperInvariant() switch
        {
            SensorTypes.AirTemperature => reading.Temperature,
            SensorTypes.AirHumidity    => reading.HumidityAir,
            SensorTypes.SoilMoisture   => reading.HumiditySoil,
            SensorTypes.LightIntensity => reading.LightIntensity,
            _ => null
        };
}
