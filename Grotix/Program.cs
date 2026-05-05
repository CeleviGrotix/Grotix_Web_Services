using GrotixBackend.IAM.Application.Internal.OutboundServices;
using GrotixBackend.IAM.Application.Internal.OutboundServices.ACL;
using GrotixBackend.IAM.Domain.Model.Aggregates;
using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.IAM.Infrastructure.Repositories;
using GrotixBackend.IAM.Infrastructure.Tokens.JWT.Configuration;
using GrotixBackend.IAM.Infrastructure.Tokens.JWT.Services;
using GrotixBackend.Profiles.Application.ACL;
using GrotixBackend.Profiles.Application.Internal.CommandServices;
using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Profiles.Infrastructure.Repositories;
using GrotixBackend.Shared.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// JWT config
builder.Services.Configure<TokenSettings>(
    builder.Configuration.GetSection("TokenSettings"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(
                builder.Configuration["TokenSettings:Secret"]!)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// IAM
builder.Services.AddScoped<IIdentityRepository, IdentityRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Profiles
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();

// ACL
builder.Services.AddScoped<IExternalProfileService, ExternalProfileService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddCors(options =>
    options.AddPolicy("AllowAllPolicy", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Grotix API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {{
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id   = "Bearer"
            }
        },
        Array.Empty<string>()
    }});
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var identityRepository = scope.ServiceProvider.GetRequiredService<IIdentityRepository>();
    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
    var aclService = scope.ServiceProvider.GetRequiredService<IExternalProfileService>();

    const string adminEmail = "admin@grotix.com";

    if (await identityRepository.GetByEmailAsync(adminEmail) == null)
    {
        var adminIdentity = new Identity(adminEmail, "Admin123$");

        await identityRepository.AddAsync(adminIdentity);
        await unitOfWork.CompleteAsync();

        await aclService.CreateUserAndReturnId(adminIdentity.Id, adminIdentity.UserName);

        Console.WriteLine("--> Admin identity and profile created successfully.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAllPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();