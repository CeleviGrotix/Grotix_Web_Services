using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC;

public sealed class HardwareDatabaseInitializer(
    IServiceProvider services,
    ILogger<HardwareDatabaseInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<HardwareDeviceDbContext>();

        try
        {
            await db.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Hardware DB migrations applied.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Hardware DB initialization failed.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
