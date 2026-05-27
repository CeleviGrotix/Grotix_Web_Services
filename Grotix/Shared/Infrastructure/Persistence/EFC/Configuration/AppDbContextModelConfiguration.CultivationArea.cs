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

            e.HasIndex(c => c.CommonName).IsUnique();
            e.HasIndex(c => c.ScientificName).IsUnique();
        });

        modelBuilder.Entity<Farm>(e =>
        {
            e.ToTable("farm");
            e.HasKey(f => f.Id);
            e.Property(f => f.Id).HasColumnName("FarmID").ValueGeneratedOnAdd();
            e.Property(f => f.UserId).HasColumnName("UserID").IsRequired(false);
            e.Property(f => f.AssociationId).HasColumnName("AssociationID");
            e.Property(f => f.Name).HasMaxLength(200).IsRequired();
            e.Property(f => f.Location).HasMaxLength(500).IsRequired();
            e.HasIndex(f => f.AssociationId);
            e.HasIndex(f => new { f.AssociationId, f.Name }).IsUnique();
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

        modelBuilder.Entity<ZoneMember>(e =>
        {
            e.ToTable("zone_member");
            e.HasKey(m => m.Id);
            e.Property(m => m.Id).HasColumnName("ZoneMemberID").ValueGeneratedOnAdd();
            e.Property(m => m.ZoneId).HasColumnName("ZoneID");
            e.Property(m => m.UserId).HasColumnName("UserID");
            e.Property(m => m.AssignedAt).HasColumnName("AssignedAt");
            e.Property(m => m.AssignedByUserId).HasColumnName("AssignedByUserID");

            e.HasIndex(m => new { m.ZoneId, m.UserId }).IsUnique();
            e.HasIndex(m => m.UserId);

            e.HasOne<Zone>().WithMany().HasForeignKey(m => m.ZoneId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
