using GrotixBackend.BuildingBlocks.Configuration;
using GrotixBackend.IrrigationCycle.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace GrotixBackend.IrrigationCycle.Infrastructure.Persistence.EFC;

public sealed class IrrigationCycleDbContextFactory : IDesignTimeDbContextFactory<IrrigationCycleDbContext>
{
    public IrrigationCycleDbContext CreateDbContext(string[] args)
    {
        // Load environment variables before creating the design-time DbContext.
        DotEnvBootstrap.LoadFromCurrentDirectory();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=127.0.0.1;Port=3306;Database=grotix_core;Uid=root;Pwd=root;";

        var optionsBuilder = new DbContextOptionsBuilder<IrrigationCycleDbContext>();
        optionsBuilder.UseMySql(
            connectionString,
            new MySqlServerVersion(new Version(8, 0, 36)),
            mysql =>
            {
                mysql.MigrationsHistoryTable("__EFMigrationsHistory_IrrigationCycle");
                mysql.MigrationsAssembly("Grotix.Persistence.IrrigationCycle");
            });

        return new IrrigationCycleDbContext(optionsBuilder.Options);
    }
}
