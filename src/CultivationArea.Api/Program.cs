using GrotixBackend.BuildingBlocks.Auth;
using GrotixBackend.BuildingBlocks.Configuration;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.CultivationArea.Application.Internal.CommandServices;
using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.Contracts.Profiles.Access;
using GrotixBackend.Contracts.Profiles.Provisioning;
using GrotixBackend.CultivationArea.Domain.Repositories;
using GrotixBackend.CultivationArea.Infrastructure.Repositories;
using GrotixBackend.IAM.Application.Internal.OutboundServices;
using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.IAM.Infrastructure.Repositories;
using GrotixBackend.IAM.Infrastructure.Tokens.JWT.Configuration;
using GrotixBackend.IAM.Infrastructure.Tokens.JWT.Services;
using GrotixBackend.Profiles.Application.ACL;
using GrotixBackend.Profiles.Application.Internal.CommandServices;
using GrotixBackend.Profiles.Application.Internal.OutboundServices;
using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Profiles.Domain.Services;
using GrotixBackend.Profiles.Infrastructure.Adapters;
using GrotixBackend.Profiles.Infrastructure.Repositories;
using GrotixBackend.Profiles.Infrastructure.Security;
using GrotixBackend.Shared.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Health;
using GrotixBackend.Shared.Infrastructure.OpenApi;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using MediatR;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using System.Reflection;

DotEnvBootstrap.LoadFromCurrentDirectory();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(
        typeof(Program).Assembly,
        typeof(FarmCommandService).Assembly));

builder.Services.Configure<TokenSettings>(
    builder.Configuration.GetSection("TokenSettings"));

builder.Services.AddGrotixJwt(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var mysqlVersionString = builder.Configuration["MySql:ServerVersion"];
Version mysqlVersion;
if (string.IsNullOrWhiteSpace(mysqlVersionString))
{
    mysqlVersion = new Version(8, 0, 36);
}
else
{
    var segments = mysqlVersionString.Trim().Split('.', StringSplitOptions.RemoveEmptyEntries);
    var major = segments.Length > 0 ? int.Parse(segments[0]) : 8;
    var minor = segments.Length > 1 ? int.Parse(segments[1]) : 0;
    var build = segments.Length > 2 ? int.Parse(segments[2]) : 0;
    mysqlVersion = new Version(major, minor, build);
}

var mysqlServerVersion = new MySqlServerVersion(mysqlVersion);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, mysqlServerVersion));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddScoped<INotificationServiceAdapter, NoOpNotificationServiceAdapter>();

builder.Services.AddScoped<IIdentityRepository, IdentityRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IExternalProfileService, ExternalProfileService>();

builder.Services.AddScoped<IUserRepository, CoreDbUserRepository>();
builder.Services.AddScoped<IAssociationRepository, CoreDbAssociationRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<IUserAccessContextService, UserAccessContextService>();

builder.Services.AddScoped<IAssociationInviteRepository, AssociationInviteRepository>();
builder.Services.AddScoped<IAssociationInviteCommandService, AssociationInviteCommandService>();

builder.Services.AddScoped<IFarmRepository, FarmRepository>();
builder.Services.AddScoped<IZoneRepository, ZoneRepository>();
builder.Services.AddScoped<ICropRepository, CropRepository>();
builder.Services.AddScoped<IFarmCommandService, FarmCommandService>();
builder.Services.AddScoped<IFarmQueryService, FarmQueryService>();
builder.Services.AddScoped<IZoneCommandService, ZoneCommandService>();
builder.Services.AddScoped<IZoneQueryService, ZoneQueryService>();
builder.Services.AddScoped<ICropCommandService, CropCommandService>();
builder.Services.AddScoped<ICropQueryService, CropQueryService>();

builder.Services.AddGrotixRabbitMqConsumer(builder.Configuration);

// El mismo ensamblado incluye handlers de Profiles (p. ej. RabbitMqUserRegisteredPublishHandler); este API no publica a RabbitMQ.
builder.Services.AddSingleton<IRabbitMqPublisher, NoOpRabbitMqPublisher>();

builder.Services
    .AddControllers()
    .ConfigureApplicationPartManager(manager =>
    {
        var defaultProvider = manager.FeatureProviders
            .OfType<ControllerFeatureProvider>()
            .FirstOrDefault();
        if (defaultProvider != null)
            manager.FeatureProviders.Remove(defaultProvider);
        manager.FeatureProviders.Add(new CultivationControllerFeatureProvider());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks()
    .AddCheck<MySqlReadinessHealthCheck>("mysql", tags: ["ready"])
    .AddCheck<TimescaleTelemetryHealthCheck>("timescale", tags: ["timescale"]);

builder.Services.AddCors(options =>
    options.AddPolicy("AllowAllPolicy", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CultivationArea API", Version = "v1" });
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

file sealed class CultivationControllerFeatureProvider : ControllerFeatureProvider
{
    private static readonly string[] AllowedNamespaces =
    [
        "GrotixBackend.CultivationArea.Interfaces.REST.Controllers",
        "GrotixBackend.Shared.Interfaces.REST.Controllers"
    ];

    protected override bool IsController(TypeInfo typeInfo)
    {
        if (!base.IsController(typeInfo)) return false;
        var ns = typeInfo.Namespace ?? string.Empty;
        return AllowedNamespaces.Any(prefix => ns.StartsWith(prefix, StringComparison.Ordinal));
    }
}
