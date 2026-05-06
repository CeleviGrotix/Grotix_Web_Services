using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>
/// Permite que las herramientas <c>dotnet ef</c> creen <see cref="AppDbContext"/> sin arrancar la API.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
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
}
