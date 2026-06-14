using GrotixBackend.BuildingBlocks.Configuration;
using GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace GrotixBackend.HardwareDevice.Infrastructure.Persistence.EFC;

public sealed class HardwareDeviceDbContextFactory : IDesignTimeDbContextFactory<HardwareDeviceDbContext>
{
    public HardwareDeviceDbContext CreateDbContext(string[] args)
    {
        DotEnvBootstrap.LoadFromCurrentDirectory();

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=127.0.0.1;Port=3306;Database=grotix_core;Uid=root;Pwd=root;";

        var optionsBuilder = new DbContextOptionsBuilder<HardwareDeviceDbContext>();
        optionsBuilder.UseMySql(
            connectionString,
            new MySqlServerVersion(new Version(8, 0, 36)),
            mysql =>
            {
                mysql.MigrationsHistoryTable("__EFMigrationsHistory_HardwareDevice");
                mysql.MigrationsAssembly("Grotix.Persistence.HardwareDevice");
            });

        return new HardwareDeviceDbContext(optionsBuilder.Options);
    }
}
