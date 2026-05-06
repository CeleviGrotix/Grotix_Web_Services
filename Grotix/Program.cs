using GrotixBackend.IAM.Application.Internal.OutboundServices;
using GrotixBackend.IAM.Application.Internal.OutboundServices.ACL;
using GrotixBackend.IAM.Domain.Model.Aggregates;
using GrotixBackend.IAM.Domain.Model.ValueObjects;
using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.IAM.Infrastructure.Repositories;
using GrotixBackend.IAM.Infrastructure.Tokens.JWT.Configuration;
using GrotixBackend.IAM.Infrastructure.Tokens.JWT.Services;
using GrotixBackend.Profiles.Application.ACL;
using GrotixBackend.Profiles.Application.Internal.CommandServices;
using GrotixBackend.Profiles.Application.Internal.QueryServices;
using GrotixBackend.Profiles.Domain.Repositories;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using GrotixBackend.Profiles.Domain.Services;
using GrotixBackend.Profiles.Infrastructure.Adapters;
using GrotixBackend.Profiles.Infrastructure.Repositories;
using GrotixBackend.Profiles.Infrastructure.Security;
using GrotixBackend.Profiles.Application.Internal.OutboundServices;
using GrotixBackend.CultivationArea.Application.Internal.CommandServices;
using GrotixBackend.CultivationArea.Application.Internal.QueryServices;
using GrotixBackend.CultivationArea.Domain.Repositories;
using GrotixBackend.CultivationArea.Infrastructure.Repositories;
using GrotixBackend.Shared.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Health;
using GrotixBackend.Shared.Infrastructure.OpenApi;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

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

// DB — versión fija: AutoDetect() abre conexión en arranque y falla si MySQL está apagado.
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

// IAM
builder.Services.AddScoped<IIdentityRepository, IdentityRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Profiles
builder.Services.AddScoped<IUserRepository, CoreDbUserRepository>();
builder.Services.AddScoped<IAssociationRepository, CoreDbAssociationRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IStaffRepository, StaffRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<IRoleQueryService, RoleQueryService>();
builder.Services.AddScoped<IStaffQueryService, StaffQueryService>();
builder.Services.AddScoped<IStaffCommandService, StaffCommandService>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IContractQueryService, ContractQueryService>();
builder.Services.AddScoped<IContractCommandService, ContractCommandService>();

// Cultivation Area
builder.Services.AddScoped<IFarmRepository, FarmRepository>();
builder.Services.AddScoped<IZoneRepository, ZoneRepository>();
builder.Services.AddScoped<ICropRepository, CropRepository>();
builder.Services.AddScoped<IFarmCommandService, FarmCommandService>();
builder.Services.AddScoped<IFarmQueryService, FarmQueryService>();
builder.Services.AddScoped<IZoneCommandService, ZoneCommandService>();
builder.Services.AddScoped<IZoneQueryService, ZoneQueryService>();
builder.Services.AddScoped<ICropCommandService, CropCommandService>();
builder.Services.AddScoped<ICropQueryService, CropQueryService>();

// ACL
builder.Services.AddScoped<IExternalProfileService, ExternalProfileService>();

builder.Services.AddControllers().AddJsonOptions(o =>
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHealthChecks()
    .AddCheck<MySqlReadinessHealthCheck>("mysql", tags: ["ready"]);

builder.Services.AddCors(options =>
    options.AddPolicy("AllowAllPolicy", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Grotix API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT sin prefijo Bearer en este campo (Swagger lo añade). Solo endpoints con candado envían el header.",
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
    using (var scope = app.Services.CreateScope())
    {
        var identityRepository = scope.ServiceProvider.GetRequiredService<IIdentityRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
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
            await unitOfWork.CompleteAsync();

            await aclService.CreateUserAndReturnId(adminIdentity.Id, adminIdentity.UserName, roleId: adminRoleId);

            Console.WriteLine("--> Admin identity and profile created successfully.");
        }

        var existingAdminIdentity = await identityRepository.GetByEmailAsync(adminEmail);
        if (existingAdminIdentity != null)
        {
            var adminProfile = await userRepository.GetByIdentityIdAsync(existingAdminIdentity.Id);
            if (adminProfile == null)
            {
                await aclService.CreateUserAndReturnId(
                    existingAdminIdentity.Id,
                    existingAdminIdentity.UserName,
                    roleId: adminRoleId);
                Console.WriteLine($"--> Perfil {adminEmail} creado (la identidad ya existía sin fila en user).");
            }
            else
            {
                var canonicalAdminEmail = UserEmail.Create(adminEmail);
                if (adminProfile.Email.Equals(canonicalAdminEmail) && adminProfile.RoleId != adminRoleId)
                {
                    adminProfile.AssignRole(adminRoleId);
                    await unitOfWork.CompleteAsync();
                    Console.WriteLine($"--> Perfil {adminEmail}: RoleID actualizado a {adminRoleId} (Admin).");
                }
            }
        }
    }
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex,
        "Startup DB bootstrap skipped (MySQL unavailable). API will run; seed admin / role sync will apply once the database is reachable.");
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