using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrotixBackend.CultivationArea.Infrastructure.Persistence.EFC.Migrations;

[DbContext(typeof(CultivationAreaDbContext))]
[Migration("20260612130000_AddZoneIrrigationMode")]
public partial class AddZoneIrrigationMode : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "IrrigationMode",
            table: "zone",
            type: "varchar(16)",
            maxLength: 16,
            nullable: false,
            defaultValue: "AUTOMATIC");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "IrrigationMode",
            table: "zone");
    }
}
