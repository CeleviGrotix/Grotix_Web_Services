using GrotixBackend.IAM.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;

public class IamDbContext(DbContextOptions<IamDbContext> options) : DbContext(options)
{
    public DbSet<Identity> Identities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ConfigureIamSchema();
    }
}
