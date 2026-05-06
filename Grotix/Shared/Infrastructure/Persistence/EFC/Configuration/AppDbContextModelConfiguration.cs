using GrotixBackend.CultivationArea.Domain.Model.Aggregates;
using GrotixBackend.IAM.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;

/// <summary>Configuración única del modelo EF (compartida por <see cref="AppDbContext"/> y migraciones).</summary>
public static class AppDbContextModelConfiguration
{
    /// <summary>Modelo hasta la migración inicial (sin <c>contract</c>).</summary>
    public static void ConfigureGrotixCoreSchema(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Identity>(e =>
        {
            e.ToTable("identity");
            e.HasKey(i => i.Id);
            e.Property(i => i.Id).HasColumnName("IdentityID").ValueGeneratedOnAdd();
            e.Property(i => i.UserName).HasColumnName("Username").IsRequired().HasMaxLength(100);
            e.OwnsOne(i => i.HashedPassword, ph =>
                ph.Property(p => p.HashedValue).HasColumnName("PasswordHash").IsRequired());
        });

        modelBuilder.Entity<RolePermissionLink>(e =>
        {
            e.ToTable("role_permission");
            e.HasKey(x => new { x.RoleId, x.PermissionId });
            e.Property(x => x.RoleId).HasColumnName("RoleID");
            e.Property(x => x.PermissionId).HasColumnName("PermissionID");
        });

        modelBuilder.Entity<Role>(e =>
        {
            e.ToTable("role");
            e.HasKey(r => r.Id);
            e.Property(r => r.Id).HasColumnName("RoleID").ValueGeneratedOnAdd();
            e.Property(r => r.Name).HasColumnName("Name").HasMaxLength(50).IsRequired();
            e.Property(r => r.Description).HasColumnName("Description").HasMaxLength(255);

            e.HasMany(r => r.Permissions)
                .WithMany(p => p.Roles)
                .UsingEntity<RolePermissionLink>(
                    j => j.HasOne(rp => rp.Permission).WithMany().HasForeignKey(rp => rp.PermissionId),
                    j => j.HasOne(rp => rp.Role).WithMany().HasForeignKey(rp => rp.RoleId));
        });

        modelBuilder.Entity<Permission>(e =>
        {
            e.ToTable("permission");
            e.HasKey(p => p.Id);
            e.Property(p => p.Id).HasColumnName("PermissionID").ValueGeneratedOnAdd();
            e.Property(p => p.Code).HasMaxLength(64).IsRequired();
            e.Property(p => p.Description).HasMaxLength(255);
        });

        modelBuilder.Entity<Association>(e =>
        {
            e.ToTable("association");
            e.HasKey(a => a.Id);
            e.Property(a => a.Id).HasColumnName("AssociationID").ValueGeneratedOnAdd();
            e.Property(a => a.Name).HasMaxLength(200).IsRequired();
            e.Property(a => a.ContactEmail)
                .HasColumnName("Email")
                .HasMaxLength(100)
                .IsRequired()
                .HasConversion(v => v.Value, v => UserEmail.Create(v));
        });

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("user");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasColumnName("UserID").ValueGeneratedOnAdd();
            e.Property(u => u.IdentityId).HasColumnName("IdentityID");
            e.Property(u => u.RoleId).HasColumnName("RoleID");
            e.Property(u => u.Email).HasColumnName("Email").IsRequired()
                .HasConversion(v => v.Value, v => UserEmail.Create(v)).HasMaxLength(100);
            e.Property(u => u.Name).HasColumnName("Name").IsRequired(false);
            e.Property(u => u.TaxId).HasColumnName("TaxID").IsRequired(false);
            e.Property(u => u.Phone).HasColumnName("Phone").IsRequired(false).HasMaxLength(20);
            e.Property(u => u.CreatedAt).HasColumnName("CreatedAt");
            e.Property(u => u.UpdatedAt).HasColumnName("UpdatedAt");
            e.Property(u => u.AssociationId).HasColumnName("AssociationID");
            e.Property(u => u.ProfilePicture).HasColumnName("profilePicture").HasMaxLength(255);
            e.Property(u => u.PreferencesJson).HasColumnName("Preferences").HasColumnType("json");

            e.HasOne<Role>().WithMany().HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Association>().WithMany().HasForeignKey(u => u.AssociationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Staff>(e =>
        {
            e.ToTable("staff");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasColumnName("StaffID").ValueGeneratedOnAdd();
            e.Property(s => s.UserId).HasColumnName("UserID");
            e.Property(s => s.IsActive).HasColumnName("IsActive");
            e.Property(s => s.TechnicalRole)
                .HasColumnName("TechnicalRole")
                .HasConversion<string>();
            e.Property(s => s.LastSystemAccess)
                .HasColumnName("LastSystemAccess")
                .HasColumnType("datetime(6)");

            e.HasOne<User>().WithMany().HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

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
        });

        modelBuilder.Entity<Farm>(e =>
        {
            e.ToTable("farm");
            e.HasKey(f => f.Id);
            e.Property(f => f.Id).HasColumnName("FarmID").ValueGeneratedOnAdd();
            e.Property(f => f.UserId).HasColumnName("UserID");
            e.Property(f => f.Name).HasMaxLength(200).IsRequired();
            e.Property(f => f.Location).HasMaxLength(500).IsRequired();

            e.HasOne<User>().WithMany().HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);
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

    public static void ConfigureContractSchema(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Contract>(e =>
        {
            e.ToTable("contract");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasColumnName("ContractID").ValueGeneratedOnAdd();
            e.Property(c => c.AssociationId).HasColumnName("AssociationID");
            e.Property(c => c.StartDate).HasColumnType("datetime(6)");
            e.Property(c => c.EndDate).HasColumnType("datetime(6)");
            e.Property(c => c.Status)
                .HasColumnName("Status")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            e.Property(c => c.MaxZones).HasColumnName("MaxZones");
            e.Property(c => c.MaxMicrocontrollers).HasColumnName("MaxMicrocontrollers");
            e.Property(c => c.TotalAmount).HasColumnName("TotalAmount");
            e.Property(c => c.Currency)
                .HasConversion<string>()
                .HasMaxLength(16)
                .IsRequired();
            e.Property(c => c.PaymentFrequency)
                .HasColumnName("PaymentFrequency")
                .HasConversion<string>()
                .HasMaxLength(32)
                .IsRequired();
            e.Property(c => c.IsSuspended).HasColumnName("IsSuspended");

            e.HasOne<Association>().WithMany().HasForeignKey(c => c.AssociationId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public static void ConfigureGrotixSchema(this ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureGrotixCoreSchema();
        modelBuilder.ConfigureContractSchema();
    }
}
