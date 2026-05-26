using GrotixBackend.BuildingBlocks.Auth;
using GrotixBackend.BuildingBlocks.Configuration;

DotEnvBootstrap.LoadFromCurrentDirectory();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrotixJwt(builder.Configuration);

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAllPolicy", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseCors("AllowAllPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.Run();
