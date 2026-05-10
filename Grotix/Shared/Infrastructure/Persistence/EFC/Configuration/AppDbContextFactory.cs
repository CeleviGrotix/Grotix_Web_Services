using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Permite que las herramientas <c>dotnet ef</c> creen <see cref="AppDbContext"/> sin arrancar la API.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = ResolveConfigurationBasePath();

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets(typeof(AppDbContextFactory).Assembly, optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection.");

        var mysqlVersionString = configuration["MySql:ServerVersion"];
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

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(mysqlVersion));

        return new AppDbContext(optionsBuilder.Options);
    }

    /// <summary>
    /// Busca la carpeta con <c>appsettings.json</c> del API (build output o cwd), necesario si <c>dotnet ef</c> se ejecuta desde la raíz del repo.
    /// </summary>
    private static string ResolveConfigurationBasePath()
    {
        var candidates = new List<string>(4);

        var asmPath = typeof(AppDbContextFactory).Assembly.Location;
        if (!string.IsNullOrEmpty(asmPath))
        {
            var dir = Path.GetDirectoryName(asmPath);
            if (!string.IsNullOrEmpty(dir))
                candidates.Add(dir);
        }

        var baseDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        if (!string.IsNullOrEmpty(baseDir))
            candidates.Add(baseDir);

        candidates.Add(Directory.GetCurrentDirectory());

        foreach (var dir in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (string.IsNullOrEmpty(dir))
                continue;
            var appsettings = Path.Combine(dir, "appsettings.json");
            if (File.Exists(appsettings))
                return dir;
        }

        throw new InvalidOperationException(
            "No se encontró appsettings.json del API. Ejecute 'dotnet build' en el proyecto de arranque y use --startup-project src/Profiles.Api/Profiles.Api.csproj, " +
            "o ejecute dotnet ef desde src/Profiles.Api. También puede definir la variable de entorno ConnectionStrings__DefaultConnection.");
    }
}
