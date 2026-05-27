#nullable disable

using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GrotixBackend.CultivationArea.Infrastructure.Persistence.EFC.Migrations;

[DbContext(typeof(CultivationAreaDbContext))]
[Migration("20260527180000_AddFarmAssociationId")]
public partial class AddFarmAssociationId
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        CultivationAreaDbContextModelSnapshotFactory.Apply(modelBuilder);
}
