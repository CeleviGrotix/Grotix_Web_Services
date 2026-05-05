using GrotixBackend.IAM.Domain.Model.Aggregates;
using GrotixBackend.Profiles.Domain.Model.Aggregates;
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
        public DbSet<User> Users { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- IAM Context Configuration ---
            modelBuilder.Entity<Identity>(entity =>
            {
                // Forzamos el nombre de la tabla en minúsculas
                entity.ToTable("identity");

                // Mapeo de la Llave Primaria: IdentityID
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Id)
                    .HasColumnName("IdentityID")
                    .ValueGeneratedOnAdd();

                // Mapeo de Username (Case sensitive en algunos entornos)
                entity.Property(i => i.UserName)
                    .HasColumnName("Username")
                    .IsRequired()
                    .HasMaxLength(50);

                // Mapeo del Value Object HashedPassword a la columna PasswordHash
                entity.OwnsOne(i => i.HashedPassword, ph =>
                {
                    ph.Property(p => p.HashedValue)
                        .HasColumnName("PasswordHash")
                        .IsRequired();
                });

                // IMPORTANTE: Si tu clase Identity tiene UserId, 
                // pero NO está en el DESCRIBE de arriba, debes ignorarlo o fallará.
                entity.Ignore(i => i.UserId);
            });

            // --- Profiles Context Configuration ---
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("UserID");
                entity.Property(e => e.IdentityId).HasColumnName("IdentityID");
                entity.Property(e => e.RoleId).HasColumnName("RoleID");
                entity.Property(e => e.Name).IsRequired(false);
                // entity.Property(e => e.Name).HasColumnName("Name");
                entity.Property(e => e.Email).HasColumnName("Email");
                entity.Property(e => e.Email).IsRequired();
                entity.Ignore(e => e.TaxId);
                entity.Ignore(e => e.Phone);
                entity.Ignore(e => e.Preferences);
                entity.Ignore(e => e.AssociationId);
                entity.Ignore(e => e.ProfilePicture);
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