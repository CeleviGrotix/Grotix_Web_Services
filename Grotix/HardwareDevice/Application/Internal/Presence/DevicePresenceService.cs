using GrotixBackend.HardwareDevice.Domain.Model.ValueObjects;
using GrotixBackend.HardwareDevice.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GrotixBackend.HardwareDevice.Application.Internal.Presence;

public sealed class DevicePresenceService(
    IMicrocontrollerRepository deviceRepository,
    IHardwareDeviceUnitOfWork unitOfWork,
    IDeviceStatusChangedPublisher deviceStatusChangedPublisher,
    IDeviceOfflinePublisher deviceOfflinePublisher,
    IOptions<DevicePresenceOptions> options,
    ILogger<DevicePresenceService> logger) : IDevicePresenceService
{
    private readonly DevicePresenceOptions _options = options.Value;

    public async Task RecordHeartbeatAsync(
        int deviceId,
        DateTime timestamp,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
            return;

        var device = await deviceRepository.GetByIdAsync(deviceId);
        if (device == null)
        {
            logger.LogWarning("Heartbeat ignored: device {DeviceId} not found.", deviceId);
            return;
        }

        var oldStatus = device.Status;
        device.RecordTelemetryHeartbeat(timestamp);
        await unitOfWork.CompleteAsync(cancellationToken);

        if (!string.Equals(oldStatus, device.Status, StringComparison.OrdinalIgnoreCase))
            deviceStatusChangedPublisher.Publish(device, oldStatus, device.Status);
    }

    public async Task MarkStaleDevicesOfflineAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
            return;

        var threshold = DateTime.UtcNow.AddMinutes(-Math.Max(1, _options.OfflineAfterMinutes));
        var staleDevices = await deviceRepository.ListOnlineStaleAsync(threshold, cancellationToken);
        if (staleDevices.Count == 0)
            return;

        foreach (var device in staleDevices)
        {
            var oldStatus = device.Status;
            device.UpdateStatus(DeviceStatuses.Offline, device.LastSeen);
            if (!string.Equals(oldStatus, device.Status, StringComparison.OrdinalIgnoreCase))
            {
                deviceStatusChangedPublisher.Publish(device, oldStatus, device.Status);
                deviceOfflinePublisher.Publish(device);
            }
        }

        await unitOfWork.CompleteAsync(cancellationToken);

        logger.LogInformation(
            "Marked {Count} device(s) OFFLINE (no telemetry since {Threshold:O}).",
            staleDevices.Count,
            threshold);
    }
}
