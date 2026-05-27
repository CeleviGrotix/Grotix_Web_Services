#nullable disable

using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GrotixBackend.CultivationArea.Infrastructure.Persistence.EFC.Migrations;

[DbContext(typeof(CultivationAreaDbContext))]
[Migration("20260527200000_AddZoneMemberAssignments")]
public partial class AddZoneMemberAssignments
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        CultivationAreaDbContextModelSnapshotFactory.Apply(modelBuilder);
}
