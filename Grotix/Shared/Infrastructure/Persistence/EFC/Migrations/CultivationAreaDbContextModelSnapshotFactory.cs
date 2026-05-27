using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;

/// <summary>Estado del modelo alineado con <see cref="CultivationAreaDbContext"/> para migraciones y snapshot.</summary>
internal static class CultivationAreaDbContextModelSnapshotFactory
{
    internal static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "9.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 64);

        modelBuilder.ConfigureCultivationAreaSchema();
    }
}
