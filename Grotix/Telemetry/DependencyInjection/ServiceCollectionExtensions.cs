using GrotixBackend.Telemetry.Application.ACL;
using Microsoft.Extensions.DependencyInjection;
using GrotixBackend.Telemetry.Application.Internal;
using GrotixBackend.Telemetry.Application.Internal.Alerting;
namespace GrotixBackend.Telemetry.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrotixTelemetryModule(this IServiceCollection services)
    {
        services.AddScoped<IZoneAuthorizationService, ZoneAuthorizationService>();
        services.AddScoped<IEffectiveThresholdResolver, EffectiveThresholdResolver>();
        services.AddScoped<ITelemetryIngestService, TelemetryIngestService>();
        services.AddScoped<ITelemetryQueryService, TelemetryQueryService>();
        services.AddScoped<IZoneThresholdService, ZoneThresholdService>();
        services.AddScoped<IAlertEvaluationService, AlertEvaluationService>();
        return services;
    }
}
