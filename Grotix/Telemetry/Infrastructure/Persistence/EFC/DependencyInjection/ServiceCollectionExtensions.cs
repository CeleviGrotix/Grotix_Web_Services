using GrotixBackend.Shared.Infrastructure.Health;
using GrotixBackend.Telemetry.Application.Internal.Alerting;
using GrotixBackend.Telemetry.Domain.Repositories;
using GrotixBackend.Telemetry.Infrastructure.Integration;
using GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Telemetry.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrotixTelemetryPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TelemetryTimescale");
        services.AddDbContext<TelemetryDbContext>(options =>
            options
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
                .UseNpgsql(
                    connectionString,
                    npgsql => npgsql.MigrationsAssembly("Grotix.Persistence.Telemetry")));

        services.AddScoped<ISensorRepository, SensorRepository>();
        services.AddScoped<ISensorReadingRepository, SensorReadingRepository>();
        services.AddScoped<IActiveThresholdRepository, ActiveThresholdRepository>();
        services.AddScoped<IThresholdBreachTrackerRepository, ThresholdBreachTrackerRepository>();
        services.AddScoped<IAlertPublisher, RabbitMqAlertPublisher>();

        services.AddHealthChecks()
            .AddCheck<TimescaleTelemetryHealthCheck>("timescale", tags: ["timescale"]);

        services.AddHostedService<TelemetryDatabaseInitializer>();

        return services;
    }
}
