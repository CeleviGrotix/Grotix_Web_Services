using GrotixBackend.IAM.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Identity> Identities { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // IAM
        modelBuilder.Entity<Identity>(e =>
        {
            e.ToTable("identity");
            e.HasKey(i => i.Id);
            e.Property(i => i.Id).HasColumnName("IdentityID").ValueGeneratedOnAdd();
            e.Property(i => i.UserName).HasColumnName("Username").IsRequired().HasMaxLength(100);
            e.OwnsOne(i => i.HashedPassword, ph =>
                ph.Property(p => p.HashedValue).HasColumnName("PasswordHash").IsRequired());
        });

        // Profiles
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("user");
            e.HasKey(u => u.Id);
            e.Property(u => u.Id).HasColumnName("UserID");
            e.Property(u => u.IdentityId).HasColumnName("IdentityID");
            e.Property(u => u.RoleId).HasColumnName("RoleID");
            e.Property(u => u.Email).HasColumnName("Email").IsRequired();
            e.Property(u => u.Name).HasColumnName("Name").IsRequired(false);
            e.Property(u => u.TaxId).HasColumnName("TaxID").IsRequired(false);
            e.Property(u => u.Phone).HasColumnName("Phone").IsRequired(false);

            e.Ignore("AssociationID");
            e.Ignore("Preferences");
            e.Ignore("profilePicture");
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added && entry.Metadata.FindProperty("CreatedAt") != null)
                entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
            else if (entry.State == EntityState.Modified && entry.Metadata.FindProperty("UpdatedAt") != null)
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
        }
        return await base.SaveChangesAsync(ct);
    }
}