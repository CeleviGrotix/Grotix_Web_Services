using GrotixBackend.Contracts.Auth.Lookup;
using GrotixBackend.Contracts.Auth.Security;
using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.Contracts.Profiles.Invites;
using GrotixBackend.Contracts.Profiles.Provisioning;
using GrotixBackend.Profiles.Application.ACL;
using GrotixBackend.Profiles.Application.Internal.CommandServices;
using GrotixBackend.Profiles.Application.Internal.OutboundServices;
using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Infrastructure.Adapters;
using GrotixBackend.Profiles.Infrastructure.Security;
using Microsoft.Extensions.DependencyInjection;

namespace GrotixBackend.Profiles.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGrotixProfilesAccessModule(this IServiceCollection services)
    {
        services.AddScoped<IUserQueryService, UserQueryService>();
        services.AddScoped<IUserAccessContextService, UserAccessContextService>();
        services.AddScoped<IAssociationExistenceService, AssociationExistenceService>();
        services.AddScoped<IAssociationOwnerLookupService, AssociationOwnerLookupService>();
        return services;
    }

    public static IServiceCollection AddGrotixProfilesModule(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<INotificationServiceAdapter, NoOpNotificationServiceAdapter>();

        services.AddGrotixProfilesAccessModule();
        services.AddScoped<IUserCommandService, UserCommandService>();
        services.AddScoped<IUserNotificationCommandService, UserNotificationCommandService>();
        services.AddScoped<IUserNotificationQueryService, UserNotificationQueryService>();
        services.AddScoped<IUserAuthorizationContextService, UserAuthorizationContextService>();
        services.AddScoped<IRoleQueryService, RoleQueryService>();
        services.AddScoped<IStaffQueryService, StaffQueryService>();
        services.AddScoped<IStaffCommandService, StaffCommandService>();
        services.AddScoped<IContractQueryService, ContractQueryService>();
        services.AddScoped<IContractCommandService, ContractCommandService>();
        services.AddScoped<IAssociationInviteCommandService, AssociationInviteCommandService>();

        services.AddScoped<IExternalProfileService, ExternalProfileService>();
        services.AddScoped<IAdminProfileRegistrationService, AdminProfileRegistrationService>();
        services.AddScoped<IAssociationInviteAccessService, AssociationInviteAccessService>();

        return services;
    }
}
