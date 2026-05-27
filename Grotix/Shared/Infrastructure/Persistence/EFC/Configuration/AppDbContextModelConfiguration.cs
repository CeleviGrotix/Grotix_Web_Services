using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>Configuración única del modelo EF (compartida por <see cref="AppDbContext"/> y migraciones).</summary>
public static partial class AppDbContextModelConfiguration
{
    /// <summary>Modelo hasta la migración inicial (sin <c>contract</c>).</summary>
    public static void ConfigureGrotixCoreSchema(this ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureProfilesCoreSchema();
        modelBuilder.ConfigureCultivationAreaSchema();
    }

    public static void ConfigureGrotixSchema(this ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureGrotixCoreSchema();
        modelBuilder.ConfigureContractSchema();
        modelBuilder.ConfigureAssociationInviteSchema();
    }
}
