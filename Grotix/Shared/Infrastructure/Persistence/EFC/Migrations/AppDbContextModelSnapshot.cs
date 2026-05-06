#nullable disable

using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;

[DbContext(typeof(AppDbContext))]
public partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder) =>
        AppDbContextModelSnapshotFactory.Apply(modelBuilder);
}
