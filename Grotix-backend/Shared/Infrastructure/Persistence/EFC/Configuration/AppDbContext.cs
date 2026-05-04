using GrotixBackend.IAM.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration
{
    /// <summary>
    /// Contexto de base de datos optimizado para el módulo de IAM en Grotix.
    /// </summary>
    public class AppDbContext : DbContext
    {
        public DbSet<Identity> Identities { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- IAM Context Configuration ---

            modelBuilder.Entity<Identity>(entity =>
            {
                entity.ToTable("Identities");
                entity.HasKey(i => i.Id);

                entity.Property(i => i.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(i => i.UserId)
                    .IsRequired();

                // Aseguramos que un usuario solo tenga una identidad (1:1)
                entity.HasIndex(i => i.UserId).IsUnique();

                entity.Property(i => i.UserName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(i => i.UserName).IsUnique();

                // Mapeo del Value Object PasswordHash
                entity.OwnsOne(i => i.HashedPassword, ph =>
                {
                    ph.Property(p => p.HashedValue)
                        .HasColumnName("HashedPassword")
                        .IsRequired();
                });
            });
        }

        /// <summary>
        /// Manejo automático de auditoría básica.
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State == EntityState.Added && entry.Metadata.FindProperty("CreatedAt") != null)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified && entry.Metadata.FindProperty("UpdatedAt") != null)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}