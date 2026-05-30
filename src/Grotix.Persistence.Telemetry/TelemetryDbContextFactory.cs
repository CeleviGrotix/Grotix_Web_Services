using GrotixBackend.BuildingBlocks.Configuration;
using GrotixBackend.Telemetry.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GrotixBackend.Telemetry.Infrastructure.Persistence.EFC;

public sealed class TelemetryDbContextFactory : IDesignTimeDbContextFactory<TelemetryDbContext>
{
    public TelemetryDbContext CreateDbContext(string[] args)
    {
        DotEnvBootstrap.LoadFromCurrentDirectory();

        var optionsBuilder = new DbContextOptionsBuilder<TelemetryDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__TelemetryTimescale")
            ?? "Host=localhost;Port=5432;Database=grotix_telemetry;Username=postgres;Password=CHANGE_ME;";

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsAssembly("Grotix.Persistence.Telemetry"));

        return new TelemetryDbContext(optionsBuilder.Options);
    }
}
