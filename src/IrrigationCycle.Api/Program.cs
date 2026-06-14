using GrotixBackend.BuildingBlocks.Auth;
using GrotixBackend.BuildingBlocks.Configuration;
using GrotixBackend.CultivationArea.DependencyInjection;
using GrotixBackend.IAM.Infrastructure.Tokens.JWT.Configuration;
using GrotixBackend.IrrigationCycle.DependencyInjection;
using GrotixBackend.IrrigationCycle.Infrastructure.Integration;
using GrotixBackend.IrrigationCycle.Infrastructure.Persistence.EFC.DependencyInjection;
using GrotixBackend.Profiles.DependencyInjection;
using GrotixBackend.Shared.Infrastructure.OpenApi;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using System.Reflection;

DotEnvBootstrap.LoadFromCurrentDirectory();

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

builder.Services.Configure<TokenSettings>(
    builder.Configuration.GetSection("TokenSettings"));

builder.Services.AddGrotixJwt(builder.Configuration);
builder.Services.AddGrotixAppPersistence(builder.Configuration);
builder.Services.AddGrotixCultivationAreaPersistence(builder.Configuration);
builder.Services.AddGrotixProfilesAccessModule();
builder.Services.AddGrotixCultivationAreaModule();
builder.Services.AddGrotixIrrigationCyclePersistence(builder.Configuration);
builder.Services.AddGrotixIrrigationCycleModule();
builder.Services.AddGrotixIrrigationRabbitMq(builder.Configuration);

builder.Services
    .AddControllers()
    .ConfigureApplicationPartManager(manager =>
    {
        var defaultProvider = manager.FeatureProviders
            .OfType<ControllerFeatureProvider>()
            .FirstOrDefault();
        if (defaultProvider != null)
            manager.FeatureProviders.Remove(defaultProvider);
        manager.FeatureProviders.Add(new IrrigationControllerFeatureProvider());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAllPolicy", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "IrrigationCycle API", Version = "v1" });
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

var rabbitMqEnabled = app.Configuration.GetValue("RabbitMq:Enabled", true);
if (rabbitMqEnabled)
{
    app.Logger.LogInformation(
        "RabbitMQ habilitado. Broker esperado en {Host}:{Port}.",
        app.Configuration["RabbitMq:HostName"] ?? "localhost",
        app.Configuration.GetValue("RabbitMq:Port", 5672));
}
else
{
    app.Logger.LogWarning(
        "RabbitMQ deshabilitado (RabbitMq:Enabled=false). No se registran topología ni consumidores.");
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

file sealed class IrrigationControllerFeatureProvider : ControllerFeatureProvider
{
    private static readonly string[] AllowedNamespaces =
    [
        "GrotixBackend.IrrigationCycle.Interfaces.REST.Controllers"
    ];

    protected override bool IsController(TypeInfo typeInfo)
    {
        if (!base.IsController(typeInfo)) return false;
        return typeInfo.Namespace != null && AllowedNamespaces.Contains(typeInfo.Namespace);
    }
}
