using GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GrotixBackend.Telemetry.Infrastructure.Persistence.EFC;

public sealed class TelemetryDbContextFactory : IDesignTimeDbContextFactory<TelemetryDbContext>
{
    public TelemetryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TelemetryDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__TelemetryTimescale")
            ?? "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=Grotix2026!";

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsAssembly("Grotix.Persistence.Telemetry"));

        return new TelemetryDbContext(optionsBuilder.Options);
    }
}
