using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GrotixBackend.IAM.Infrastructure.Persistence.EFC.Migrations
{
    /// <summary>
    /// Baseline de IAM sobre una base existente: solo toma ownership lógico del esquema identity.
    /// </summary>
    public partial class InitialIamContext : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
