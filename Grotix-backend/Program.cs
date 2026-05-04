using GrotixBackend.IAM.Application.Internal.CommandServices;
using GrotixBackend.IAM.Application.Internal.OutboundServices;
using GrotixBackend.IAM.Domain.Model.Aggregates;
using GrotixBackend.IAM.Domain.Model.Commands;
using GrotixBackend.IAM.Domain.Repositories;
using GrotixBackend.IAM.Domain.Model.Services;
using GrotixBackend.IAM.Infrastructure.Persistence.Repositories;
using GrotixBackend.IAM.Infrastructure.Tokens.JWT.Configuration;
using GrotixBackend.IAM.Infrastructure.Tokens.JWT.Services;
using GrotixBackend.Shared.Domain.Repositories;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));
builder.Services.AddScoped<ITokenService, TokenService>();

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration.GetSection("TokenSettings:Secret").Value)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IIdentityRepository, IdentityRepository>();
builder.Services.AddScoped<IIdentityCommandService, IdentityCommandService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

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
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    var identityRepository = services.GetRequiredService<IIdentityRepository>();
    var unitOfWork = services.GetRequiredService<IUnitOfWork>();

    context.Database.EnsureCreated();

    const string defaultAdmin = "admin";
    var existingAdmin = await identityRepository.GetByUsernameAsync(defaultAdmin);

    if (existingAdmin == null)
    {
        var adminIdentity = new Identity(1, defaultAdmin, "admin1234");
        await identityRepository.AddAsync(adminIdentity);
        await unitOfWork.CompleteAsync();
        Console.WriteLine("--> Admin identity created successfully.");
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