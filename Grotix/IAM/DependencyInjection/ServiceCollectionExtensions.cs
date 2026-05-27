using GrotixBackend.Contracts.Auth.Admin;
using GrotixBackend.Contracts.Auth.Lookup;
using GrotixBackend.IAM.Application.ACL;
using GrotixBackend.IAM.Application.Internal.OutboundServices;
using GrotixBackend.IAM.Infrastructure.Tokens.JWT.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GrotixBackend.IAM.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrotixIamModule(this IServiceCollection services)
    {
        services.AddScoped<IIdentityLookupService, IdentityLookupService>();
        services.AddScoped<IAdminIdentityRegistrationService, AdminIdentityRegistrationService>();
        services.AddScoped<ITokenService, TokenService>();
        return services;
    }
}
