using GrotixBackend.BuildingBlocks.Auth;
using GrotixBackend.BuildingBlocks.Configuration;
using GrotixBackend.BuildingBlocks.RabbitMq;
using GrotixBackend.CultivationArea.Application.Internal.CommandServices;
using GrotixBackend.CultivationArea.DependencyInjection;
using GrotixBackend.IAM.Infrastructure.Tokens.JWT.Configuration;
using GrotixBackend.Profiles.DependencyInjection;
using GrotixBackend.Shared.Infrastructure.Health;
using GrotixBackend.Shared.Infrastructure.OpenApi;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.DependencyInjection;
using MediatR;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
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
builder.Services.AddGrotixAppPersistence(builder.Configuration);
builder.Services.AddGrotixCultivationAreaPersistence(builder.Configuration);
builder.Services.AddGrotixProfilesAccessModule();
builder.Services.AddGrotixCultivationAreaModule(builder.Configuration);
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
