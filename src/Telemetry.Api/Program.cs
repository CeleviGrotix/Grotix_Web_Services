using GrotixBackend.BuildingBlocks.Auth;
using GrotixBackend.BuildingBlocks.Configuration;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.CultivationArea.DependencyInjection;
using GrotixBackend.IAM.Infrastructure.Tokens.JWT.Configuration;
using GrotixBackend.Profiles.DependencyInjection;
using GrotixBackend.Shared.Infrastructure.Health;
using GrotixBackend.Shared.Infrastructure.OpenApi;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.DependencyInjection;
using GrotixBackend.Telemetry.DependencyInjection;
using GrotixBackend.Telemetry.Infrastructure.Integration;
using GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using System.Reflection;

DotEnvBootstrap.LoadFromCurrentDirectory();

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<TokenSettings>(
    builder.Configuration.GetSection("TokenSettings"));

builder.Services.AddGrotixJwt(builder.Configuration);
builder.Services.AddGrotixAppPersistence(builder.Configuration);
builder.Services.AddGrotixCultivationAreaPersistence(builder.Configuration);
builder.Services.AddGrotixProfilesAccessModule();
builder.Services.AddGrotixCultivationAreaModule();
builder.Services.AddGrotixTelemetryPersistence(builder.Configuration);
builder.Services.AddGrotixTelemetryModule(builder.Configuration);
builder.Services.AddGrotixRabbitMqPublisher(builder.Configuration);
builder.Services.AddGrotixTelemetryAlertPublisher();
builder.Services.AddGrotixTelemetryRabbitMq(builder.Configuration);

builder.Services
    .AddControllers()
    .ConfigureApplicationPartManager(manager =>
    {
        var defaultProvider = manager.FeatureProviders
            .OfType<ControllerFeatureProvider>()
            .FirstOrDefault();
        if (defaultProvider != null)
            manager.FeatureProviders.Remove(defaultProvider);
        manager.FeatureProviders.Add(new TelemetryControllerFeatureProvider());
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
    options.AddPolicy("AllowAllPolicy", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Telemetry API", Version = "v1" });
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

file sealed class TelemetryControllerFeatureProvider : ControllerFeatureProvider
{
    private static readonly string[] AllowedNamespaces =
    [
        "GrotixBackend.Telemetry.Interfaces.REST.Controllers"
    ];

    protected override bool IsController(TypeInfo typeInfo)
    {
        if (!base.IsController(typeInfo)) return false;
        return typeInfo.Namespace != null && AllowedNamespaces.Contains(typeInfo.Namespace);
    }
}
