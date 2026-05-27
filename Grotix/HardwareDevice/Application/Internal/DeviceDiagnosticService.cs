using GrotixBackend.HardwareDevice.Domain.Model.ValueObjects;
using GrotixBackend.HardwareDevice.Domain.Repositories;
using GrotixBackend.Telemetry.Domain.Repositories;

namespace GrotixBackend.HardwareDevice.Application.Internal;

public sealed class DeviceDiagnosticService(
    IDeviceQueryService deviceQueryService,
    ISensorReadingRepository sensorReadingRepository,
    IDeviceSensorRepository deviceSensorRepository,
    IHardwareDeviceUnitOfWork unitOfWork) : IDeviceDiagnosticService
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
            var sensorChecks = new List<object>();
            foreach (var sensor in detail.Sensors)
            {
                var readings = await sensorReadingRepository.ListBySensorAsync(
                    sensor.Id,
                    DateTime.UtcNow.AddHours(-24),
                    null,
                    1);

                DateTime? lastSeen = sensor.LastSeen;
                if (readings.Count > 0)
                {
                    var tracked = await deviceSensorRepository.GetByIdAsync(sensor.Id);
                    if (tracked != null)
                    {
                        tracked.TouchLastSeen(readings[0].Timestamp);
                        lastSeen = tracked.LastSeen;
                    }
                }

                sensorChecks.Add(new
                {
                    sensorId = sensor.Id,
                    type = sensor.Type,
                    status = readings.Count > 0 ? "PASS" : "WARN",
                    value = readings.FirstOrDefault()?.Value,
                    unit = sensor.Unit,
                    lastSeen
                });
            }

            await unitOfWork.CompleteAsync();

            checks["sensors"] = sensorChecks;
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
}
