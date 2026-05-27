using GrotixBackend.Profiles.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;

public static partial class AppDbContextModelConfiguration
{
    public static void ConfigureProfilesCoreSchema(this ModelBuilder modelBuilder)
    {
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
            e.Property(u => u.Name).HasColumnName("Name").IsRequired(false).HasMaxLength(200);
            e.Property(u => u.TaxId).HasColumnName("TaxID").IsRequired(false).HasMaxLength(50);
            e.Property(u => u.Phone).HasColumnName("Phone").IsRequired(false).HasMaxLength(20);
            e.Property(u => u.CreatedAt).HasColumnName("CreatedAt");
            e.Property(u => u.UpdatedAt).HasColumnName("UpdatedAt");
            e.Property(u => u.AssociationId).HasColumnName("AssociationID");
            e.Property(u => u.ProfilePicture).HasColumnName("profilePicture").HasMaxLength(255);
            e.Property(u => u.PreferencesJson).HasColumnName("Preferences").HasColumnType("json");
            e.Property(u => u.IsActive).HasColumnName("IsActive");

            e.HasOne<Role>().WithMany().HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<Association>().WithMany().HasForeignKey(u => u.AssociationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<UserNotification>(e =>
        {
            e.ToTable("user_notification");
            e.HasKey(n => n.Id);
            e.Property(n => n.Id).HasColumnName("NotificationID").ValueGeneratedOnAdd();
            e.Property(n => n.UserId).HasColumnName("UserID");
            e.Property(n => n.Title).HasColumnName("Title").HasMaxLength(150).IsRequired();
            e.Property(n => n.Message).HasColumnName("Message").HasMaxLength(1000).IsRequired();
            e.Property(n => n.Type).HasColumnName("Type").HasMaxLength(32).IsRequired();
            e.Property(n => n.IsRead).HasColumnName("IsRead");
            e.Property(n => n.CreatedAt).HasColumnName("CreatedAt");
            e.Property(n => n.ReadAt).HasColumnName("ReadAt").IsRequired(false);

            e.HasIndex(n => new { n.UserId, n.IsRead });
            e.HasIndex(n => n.CreatedAt);

            e.HasOne<User>().WithMany().HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
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
                .HasConversion<string>()
                .HasMaxLength(32);
            e.Property(s => s.LastSystemAccess)
                .HasColumnName("LastSystemAccess")
                .HasColumnType("datetime(6)");

            e.HasOne<User>().WithMany().HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
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

    public static void ConfigureAssociationInviteSchema(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssociationInvite>(e =>
        {
            e.ToTable("association_invite");
            e.HasKey(i => i.Id);
            e.Property(i => i.Id).HasColumnName("InviteID").ValueGeneratedOnAdd();
            e.Property(i => i.AssociationId).HasColumnName("AssociationID");
            e.Property(i => i.InviteEmail).HasColumnName("InviteEmail").HasMaxLength(100).IsRequired();
            e.Property(i => i.TokenHash).HasMaxLength(64).IsRequired();
            e.HasIndex(i => i.TokenHash).IsUnique();
            e.Property(i => i.RoleId).HasColumnName("RoleID");
            e.Property(i => i.ExpiresAt).HasColumnName("ExpiresAt").HasColumnType("datetime(6)");
            e.Property(i => i.UsedAt).HasColumnName("UsedAt").HasColumnType("datetime(6)");
            e.Property(i => i.CreatedAt).HasColumnName("CreatedAt").HasColumnType("datetime(6)");
            e.Property(i => i.CreatedByUserId).HasColumnName("CreatedByUserID");

            e.HasOne<Association>().WithMany().HasForeignKey(i => i.AssociationId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne<Role>().WithMany().HasForeignKey(i => i.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne<User>().WithMany().HasForeignKey(i => i.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
