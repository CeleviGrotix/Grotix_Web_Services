using GrotixBackend.IrrigationCycle.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Persistence.EFC;

public sealed class IrrigationDatabaseInitializer(
    IServiceProvider services,
    ILogger<IrrigationDatabaseInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IrrigationCycleDbContext>();

        try
        {
            await db.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Irrigation DB migrations applied.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Irrigation DB initialization failed.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
