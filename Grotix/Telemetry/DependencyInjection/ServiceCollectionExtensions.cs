using GrotixBackend.Telemetry.Application.ACL;
using GrotixBackend.Telemetry.Application.Internal;
using GrotixBackend.Telemetry.Application.Internal.Alerting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GrotixBackend.Telemetry.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrotixTelemetryModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<AlertEvaluationOptions>(configuration.GetSection(AlertEvaluationOptions.SectionName));

        services.AddScoped<IZoneAuthorizationService, ZoneAuthorizationService>();
        services.AddScoped<IEffectiveThresholdResolver, EffectiveThresholdResolver>();
        services.AddScoped<ITelemetryIngestService, TelemetryIngestService>();
        services.AddScoped<ITelemetryQueryService, TelemetryQueryService>();
        services.AddScoped<IZoneThresholdService, ZoneThresholdService>();
        services.AddScoped<IAlertEvaluationService, AlertEvaluationService>();
        services.AddScoped<IAlertQueryService, AlertQueryService>();
        return services;
    }
}
