#nullable disable

using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260512120000_UserIsActive")]
public partial class UserIsActive
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        AppDbContextModelSnapshotFactory.Apply(modelBuilder);
}
