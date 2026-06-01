using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrotixBackend.CultivationArea.Infrastructure.Persistence.EFC.Migrations;

[DbContext(typeof(CultivationAreaDbContext))]
[Migration("20260531190000_SplitCropHumidityFields")]
public partial class SplitCropHumidityFields : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "OptimalHumidity",
            table: "crop",
            newName: "OptimalHumiditySoil");

        migrationBuilder.AddColumn<double>(
            name: "OptimalHumidityAir",
            table: "crop",
            type: "double",
            nullable: false,
            defaultValue: 0.0);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "OptimalHumidityAir",
            table: "crop");

        migrationBuilder.RenameColumn(
            name: "OptimalHumiditySoil",
            table: "crop",
            newName: "OptimalHumidity");
    }
}
