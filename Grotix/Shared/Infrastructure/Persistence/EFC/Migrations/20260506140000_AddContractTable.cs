#nullable disable

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GrotixBackend.Shared.Infrastructure.Persistence.EFC.Migrations;

public partial class AddContractTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "contract",
            columns: table => new
            {
                ContractID = table.Column<int>(type: "int", nullable: false)
                    .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                AssociationID = table.Column<int>(type: "int", nullable: false),
                StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                Status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                MaxZones = table.Column<int>(type: "int", nullable: false),
                MaxMicrocontrollers = table.Column<int>(type: "int", nullable: false),
                TotalAmount = table.Column<float>(type: "float", nullable: false),
                Currency = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                PaymentFrequency = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                    .Annotation("MySql:CharSet", "utf8mb4"),
                IsSuspended = table.Column<bool>(type: "tinyint(1)", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_contract", x => x.ContractID);
                table.ForeignKey(
                    name: "FK_contract_association_AssociationID",
                    column: x => x.AssociationID,
                    principalTable: "association",
                    principalColumn: "AssociationID",
                    onDelete: ReferentialAction.Restrict);
            })
            .Annotation("MySql:CharSet", "utf8mb4");

        migrationBuilder.CreateIndex(
            name: "IX_contract_AssociationID",
            table: "contract",
            column: "AssociationID");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "contract");
    }
}
