using GrotixBackend.HardwareDevice.Application.Internal.Presence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GrotixBackend.HardwareDevice.Infrastructure.Integration;

/// <summary>
/// Revisa periódicamente dispositivos ONLINE sin telemetría reciente y los marca OFFLINE.
/// </summary>
public sealed class DevicePresenceOfflineHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<DevicePresenceOptions> options,
    ILogger<DevicePresenceOfflineHostedService> logger) : BackgroundService
{
    private readonly DevicePresenceOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
            return;

        var interval = TimeSpan.FromMinutes(Math.Max(1, _options.CheckIntervalMinutes));
        logger.LogInformation(
            "Device presence offline check enabled (every {IntervalMinutes} min, threshold {OfflineAfterMinutes} min).",
            _options.CheckIntervalMinutes,
            _options.OfflineAfterMinutes);

        using var timer = new PeriodicTimer(interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var presenceService = scope.ServiceProvider.GetRequiredService<IDevicePresenceService>();
                await presenceService.MarkStaleDevicesOfflineAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Device presence offline check failed.");
            }
        }
    }
}
