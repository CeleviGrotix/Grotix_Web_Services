using GrotixBackend.BuildingBlocks.Configuration;
using GrotixBackend.IrrigationCycle.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Persistence.EFC;

public sealed class IrrigationCycleDbContextFactory : IDesignTimeDbContextFactory<IrrigationCycleDbContext>
{
    public IrrigationCycleDbContext CreateDbContext(string[] args)
    {
        DotEnvBootstrap.LoadFromCurrentDirectory();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=127.0.0.1;Port=3306;Database=grotix_core;Uid=root;Pwd=root;";

        var serverVersion = ServerVersion.AutoDetect(connectionString);

        var optionsBuilder = new DbContextOptionsBuilder<IrrigationCycleDbContext>();
        optionsBuilder.UseMySql(
            connectionString,
            serverVersion,
            mysql => mysql.MigrationsAssembly("Grotix.Persistence.IrrigationCycle"));

        return new IrrigationCycleDbContext(optionsBuilder.Options);
    }
}
