#nullable disable

using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;

[DbContext(typeof(ProfilesDbContext))]
[Migration("20260511120000_CropImageUrl")]
public partial class CropImageUrl
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        AppDbContextModelSnapshotFactory.Apply(modelBuilder);
}
