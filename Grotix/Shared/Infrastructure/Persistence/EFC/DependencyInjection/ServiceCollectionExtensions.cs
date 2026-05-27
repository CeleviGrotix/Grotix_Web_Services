using GrotixBackend.CultivationArea.Domain.Repositories;
using GrotixBackend.CultivationArea.Infrastructure.Repositories;
using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.IAM.Infrastructure.Repositories;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Profiles.Infrastructure.Repositories;
using GrotixBackend.Shared.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Health;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrotixPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var mysqlServerVersion = ResolveMySqlServerVersion(configuration["MySql:ServerVersion"]);

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, mysqlServerVersion));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddHealthChecks()
            .AddCheck<MySqlReadinessHealthCheck>("mysql", tags: ["ready"]);

        services.AddIamPersistence();
        services.AddProfilesPersistence();
        services.AddCultivationAreaPersistence();

        return services;
    }

    private static IServiceCollection AddIamPersistence(this IServiceCollection services)
    {
        services.AddScoped<IIdentityRepository, IdentityRepository>();
        return services;
    }

    private static IServiceCollection AddProfilesPersistence(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, CoreDbUserRepository>();
        services.AddScoped<IAssociationRepository, CoreDbAssociationRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IAssociationInviteRepository, AssociationInviteRepository>();
        return services;
    }

    private static IServiceCollection AddCultivationAreaPersistence(this IServiceCollection services)
    {
        services.AddScoped<IFarmRepository, FarmRepository>();
        services.AddScoped<IZoneRepository, ZoneRepository>();
        services.AddScoped<ICropRepository, CropRepository>();
        return services;
    }

    private static MySqlServerVersion ResolveMySqlServerVersion(string? mysqlVersionString)
    {
        if (string.IsNullOrWhiteSpace(mysqlVersionString))
            return new MySqlServerVersion(new Version(8, 0, 36));

        var segments = mysqlVersionString.Trim().Split('.', StringSplitOptions.RemoveEmptyEntries);
        var major = segments.Length > 0 ? int.Parse(segments[0]) : 8;
        var minor = segments.Length > 1 ? int.Parse(segments[1]) : 0;
        var build = segments.Length > 2 ? int.Parse(segments[2]) : 0;

        return new MySqlServerVersion(new Version(major, minor, build));
    }
}
