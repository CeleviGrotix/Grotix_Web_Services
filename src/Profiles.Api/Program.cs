using GrotixBackend.BuildingBlocks.Auth;
using GrotixBackend.BuildingBlocks.Configuration;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.Contracts.Auth.Security;
using GrotixBackend.Contracts.Profiles.Provisioning;
using GrotixBackend.IAM.Application.ACL;
using GrotixBackend.IAM.DependencyInjection;
using GrotixBackend.IAM.Domain.Model.Aggregates;
using GrotixBackend.IAM.Domain.Model.ValueObjects;
using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.IAM.Infrastructure.Tokens.JWT.Configuration;
using GrotixBackend.Profiles.Application.Internal.CommandServices;
using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.DependencyInjection;
using GrotixBackend.Profiles.Infrastructure.Integration;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Health;
using GrotixBackend.Shared.Infrastructure.OpenApi;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.DependencyInjection;
using MediatR;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text.Json.Serialization;

DotEnvBootstrap.LoadFromCurrentDirectory();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(
        typeof(Program).Assembly,
        typeof(AdminIdentityRegistrationService).Assembly,
        typeof(UserCommandService).Assembly,
        typeof(SearchQueryHandler).Assembly));

builder.Services.Configure<TokenSettings>(
    builder.Configuration.GetSection("TokenSettings"));

builder.Services.AddGrotixJwt(builder.Configuration);
builder.Services.AddGrotixProfilesPersistence(builder.Configuration);
builder.Services.AddGrotixCultivationAreaPersistence(builder.Configuration);
builder.Services.AddGrotixIamPersistence(builder.Configuration);
builder.Services.AddGrotixIamModule();
builder.Services.AddGrotixProfilesModule();
builder.Services.AddGrotixRabbitMqPublisher(builder.Configuration);
builder.Services.AddGrotixProfilesRabbitMq(builder.Configuration);

builder.Services
    .AddControllers()
    .ConfigureApplicationPartManager(manager =>
    {
        var defaultProvider = manager.FeatureProviders
            .OfType<ControllerFeatureProvider>()
            .FirstOrDefault();
        if (defaultProvider != null)
            manager.FeatureProviders.Remove(defaultProvider);
        manager.FeatureProviders.Add(new ProfilesControllerFeatureProvider());
    })
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHealthChecks()
    .AddCheck<TimescaleTelemetryHealthCheck>("timescale", tags: ["timescale"]);

builder.Services.AddCors(options =>
    options.AddPolicy("AllowAllPolicy", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Profiles API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT sin prefijo Bearer en este campo (Swagger lo añade).",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.OperationFilter<BearerAuthOperationFilter>();
});

var app = builder.Build();

try
{
    using var scope = app.Services.CreateScope();
    var identityRepository = scope.ServiceProvider.GetRequiredService<IIdentityRepository>();
    var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
    var unitOfWork = scope.ServiceProvider.GetRequiredService<IProfilesUnitOfWork>();
    var iamUnitOfWork = scope.ServiceProvider.GetRequiredService<IIamUnitOfWork>();
    var aclService = scope.ServiceProvider.GetRequiredService<IExternalProfileService>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    const string adminEmail = "admin@grotix.com";
    const int adminRoleId = 1;

    if (await identityRepository.GetByEmailAsync(adminEmail) == null)
    {
        const string adminPassword = "Admin123$";
        Identity.VerifyPasswordStrength(adminPassword);
        var hash = passwordHasher.Hash(adminPassword);
        var adminIdentity = new Identity(adminEmail, PasswordHash.FromHash(hash));

        await identityRepository.AddAsync(adminIdentity);
        await iamUnitOfWork.CompleteAsync();

        await aclService.CreateUserAndReturnId(new CreateProfileUserRequest(
            adminIdentity.Id,
            adminIdentity.UserName,
            adminRoleId));
    }

    var existingAdminIdentity = await identityRepository.GetByEmailAsync(adminEmail);
    if (existingAdminIdentity != null)
    {
        var adminProfile = await userRepository.GetByIdentityIdAsync(existingAdminIdentity.Id);
        if (adminProfile == null)
        {
            await aclService.CreateUserAndReturnId(new CreateProfileUserRequest(
                existingAdminIdentity.Id,
                existingAdminIdentity.UserName,
                adminRoleId));
        }
        else
        {
            var canonicalAdminEmail = UserEmail.Create(adminEmail);
            if (adminProfile.Email.Equals(canonicalAdminEmail) && adminProfile.RoleId != adminRoleId)
            {
                adminProfile.AssignRole(adminRoleId);
                await unitOfWork.CompleteAsync();
            }
        }
    }
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Startup DB bootstrap skipped in Profiles.Api.");
}

var enableSwagger = app.Environment.IsDevelopment()
    || app.Configuration.GetValue("Swagger:Enabled", false);
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAllPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

file sealed class ProfilesControllerFeatureProvider : ControllerFeatureProvider
{
    private static readonly string[] AllowedNamespaces =
    [
        "GrotixBackend.IAM.Interfaces.REST.Controllers",
        "GrotixBackend.Profiles.Interfaces.REST.Controllers",
        "GrotixBackend.Shared.Interfaces.REST.Controllers"
    ];

    protected override bool IsController(TypeInfo typeInfo)
    {
        if (!base.IsController(typeInfo)) return false;
        var ns = typeInfo.Namespace ?? string.Empty;
        return AllowedNamespaces.Any(prefix => ns.StartsWith(prefix, StringComparison.Ordinal));
    }
}
