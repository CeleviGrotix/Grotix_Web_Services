using GrotixBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrotixBackend.CultivationArea.Infrastructure.Persistence.EFC.Migrations;

[DbContext(typeof(CultivationAreaDbContext))]
[Migration("20260601200000_AddZoneName")]
public partial class AddZoneName : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Name",
            table: "zone",
            type: "varchar(120)",
            maxLength: 120,
            nullable: false,
            defaultValue: "Zona sin nombre");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Name", table: "zone");
    }
}
