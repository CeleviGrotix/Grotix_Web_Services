#nullable disable

using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;

[DbContext(typeof(ProfilesDbContext))]
public partial class ProfilesDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder) =>
        AppDbContextModelSnapshotFactory.Apply(modelBuilder);
}
