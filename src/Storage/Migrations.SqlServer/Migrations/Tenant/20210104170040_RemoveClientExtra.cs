using Microsoft.EntityFrameworkCore.Migrations;

namespace SqlServer.Migrations.Tenant
{
    public partial class RemoveClientExtra : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Issuer",
                table: "Clients");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Issuer",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
