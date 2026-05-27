#nullable disable

using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Grotix.Persistence.GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;

[DbContext(typeof(ProfilesDbContext))]
[Migration("20260527210000_AddUserUniqueConstraints")]
public partial class AddUserUniqueConstraints
{
    protected override void BuildTargetModel(ModelBuilder modelBuilder) =>
        ProfilesDbContextModelSnapshotFactory.Apply(modelBuilder);
}
