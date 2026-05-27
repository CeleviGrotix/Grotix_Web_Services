using GrotixBackend.CultivationArea.Application.Internal.CommandServices;
using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using Microsoft.Extensions.DependencyInjection;

namespace GrotixBackend.CultivationArea.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrotixCultivationAreaModule(this IServiceCollection services)
    {
        services.AddScoped<IFarmCommandService, FarmCommandService>();
        services.AddScoped<IFarmQueryService, FarmQueryService>();
        services.AddScoped<IZoneCommandService, ZoneCommandService>();
        services.AddScoped<IZoneQueryService, ZoneQueryService>();
        services.AddScoped<ICropCommandService, CropCommandService>();
        services.AddScoped<ICropQueryService, CropQueryService>();

        return services;
    }
}
