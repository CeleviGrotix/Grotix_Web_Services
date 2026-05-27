using GrotixBackend.IrrigationCycle.Application.Internal;
using GrotixBackend.IrrigationCycle.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Integration;

/// <summary>Completa ciclos IN_PROGRESS cuando expira la duración planificada.</summary>
public sealed class IrrigationCycleCompletionHostedService(
    IServiceProvider services,
    ILogger<IrrigationCycleCompletionHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = services.CreateAsyncScope();
                var cycleRepository = scope.ServiceProvider.GetRequiredService<IIrrigationCycleRepository>();
                var commandService = scope.ServiceProvider.GetRequiredService<IIrrigationCommandService>();

                var due = await cycleRepository.ListDueForCompletionAsync(DateTime.UtcNow);
                foreach (var cycle in due)
                    await commandService.CompleteCycleAsync(cycle.Id, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error completing irrigation cycles");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
