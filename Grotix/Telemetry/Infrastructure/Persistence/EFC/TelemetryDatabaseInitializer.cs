using GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GrotixBackend.Telemetry.Infrastructure.Persistence.EFC;

/// <summary>Aplica migraciones EF y, si está disponible, convierte <c>sensor_reading</c> en hypertable Timescale.</summary>
public sealed class TelemetryDatabaseInitializer(
    IServiceProvider services,
    ILogger<TelemetryDatabaseInitializer> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TelemetryDbContext>();

        try
        {
            await db.Database.MigrateAsync(cancellationToken);
            await TryCreateHypertableAsync(db, logger, cancellationToken);
            logger.LogInformation("Telemetry DB migrations applied.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Telemetry DB initialization failed. Check ConnectionStrings:TelemetryTimescale and that grotix-telemetry is running.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task TryCreateHypertableAsync(
        TelemetryDbContext db,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            // Timescale requires the time column in the partitioning index; drop/recreate PK if needed.
            await db.Database.ExecuteSqlRawAsync(
                """
                ALTER TABLE sensor_reading DROP CONSTRAINT IF EXISTS "PK_sensor_reading";
                SELECT create_hypertable('sensor_reading', 'timestamp', if_not_exists => TRUE);
                DO $$
                BEGIN
                  IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint
                    WHERE conname = 'PK_sensor_reading' AND conrelid = 'sensor_reading'::regclass
                  ) THEN
                    ALTER TABLE sensor_reading ADD CONSTRAINT "PK_sensor_reading" PRIMARY KEY (sensor_id, timestamp);
                  END IF;
                END $$;
                """,
                cancellationToken);
            logger.LogInformation("Timescale hypertable sensor_reading is ready.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Hypertable sensor_reading not created; time-series still work on plain PostgreSQL.");
        }
    }
}
