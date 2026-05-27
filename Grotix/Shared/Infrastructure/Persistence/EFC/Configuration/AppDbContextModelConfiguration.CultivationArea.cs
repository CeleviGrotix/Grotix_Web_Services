using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;

public static partial class AppDbContextModelConfiguration
{
    public static void ConfigureCultivationAreaSchema(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Crop>(e =>
        {
            e.ToTable("crop");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnName("CropID").ValueGeneratedOnAdd();
            e.Property(c => c.CommonName).HasMaxLength(120).IsRequired();
            e.Property(c => c.ScientificName).HasMaxLength(180).IsRequired();
            e.Property(c => c.OptimalTemperature);
            e.Property(c => c.OptimalHumidity);
            e.Property(c => c.OptimalLight);
            e.Property(c => c.MaxStressTime);
            e.Property(c => c.ImageUrl).HasColumnName("ImageURL").HasMaxLength(512);
        });

        modelBuilder.Entity<Farm>(e =>
        {
            e.ToTable("farm");
            e.HasKey(f => f.Id);
            e.Property(f => f.Id).HasColumnName("FarmID").ValueGeneratedOnAdd();
            e.Property(f => f.UserId).HasColumnName("UserID");
            e.Property(f => f.Name).HasMaxLength(200).IsRequired();
            e.Property(f => f.Location).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<Zone>(e =>
        {
            e.ToTable("zone");
            e.HasKey(z => z.Id);
            e.Property(z => z.Id).HasColumnName("ZoneID").ValueGeneratedOnAdd();
            e.Property(z => z.FarmId).HasColumnName("FarmID");
            e.Property(z => z.CropId).HasColumnName("CropID");
            e.Property(z => z.CurrentPhase).HasMaxLength(80);
            e.Property(z => z.PhaseStartDate);
            e.Property(z => z.ImageUrl).HasColumnName("ImageURL").HasMaxLength(512);
            e.Property(z => z.Latitude);
            e.Property(z => z.Longitude);

            e.HasOne<Farm>().WithMany().HasForeignKey(z => z.FarmId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne<Crop>().WithMany().HasForeignKey(z => z.CropId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
