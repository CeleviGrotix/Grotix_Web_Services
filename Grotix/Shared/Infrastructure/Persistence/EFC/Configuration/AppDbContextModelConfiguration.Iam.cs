using GrotixBackend.IAM.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;

public static partial class AppDbContextModelConfiguration
{
    public static void ConfigureIamSchema(this ModelBuilder modelBuilder)
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
    }
}
