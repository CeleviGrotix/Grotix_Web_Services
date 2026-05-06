using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;

/// <summary>Estado del modelo alineado con <see cref="AppDbContext"/> para migraciones y snapshot.</summary>
internal static class AppDbContextModelSnapshotFactory
{
    /// <summary>Estado del modelo solo tras <c>InitialCreate</c> (sin tabla <c>contract</c>).</summary>
    internal static void ApplyInitialBaseline(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "9.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 64);

        modelBuilder.ConfigureGrotixCoreSchema();
    }

    internal static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "9.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 64);

        modelBuilder.ConfigureGrotixSchema();
    }
}
