using GrotixBackend.Profiles.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;

public abstract class ProfilesDbContextBase(DbContextOptions options) : DbContext(options)
{
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserNotification> UserNotifications { get; set; }
    public DbSet<Staff> StaffMembers { get; set; }
    public DbSet<Association> Associations { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<AssociationInvite> AssociationInvites { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ConfigureGrotixSchema();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added && entry.Metadata.FindProperty("CreatedAt") != null)
            {
                entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                if (entry.Metadata.FindProperty("UpdatedAt") != null)
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified && entry.Metadata.FindProperty("UpdatedAt") != null)
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
        }
        return await base.SaveChangesAsync(ct);
    }
}

public class ProfilesDbContext(DbContextOptions<ProfilesDbContext> options) : ProfilesDbContextBase(options);

/// <summary>
/// Alias temporal de compatibilidad mientras los hosts y servicios migran a <see cref="ProfilesDbContext"/>.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : ProfilesDbContextBase(options);
