#nullable disable

using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;

public partial class AddLastSystemAccessToStaff : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<DateTime>(
            name: "LastSystemAccess",
            table: "staff",
            type: "datetime(6)",
            nullable: false,
            defaultValueSql: "CURRENT_TIMESTAMP(6)");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "LastSystemAccess",
            table: "staff");
    }
}
