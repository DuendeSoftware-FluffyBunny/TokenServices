using Microsoft.EntityFrameworkCore.Migrations;

namespace SqlServer.Migrations.Tenant
{
    public partial class AllowedArbitraryIssuers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedArbitraryIssuer",
                table: "Clients");

            migrationBuilder.CreateTable(
                name: "AllowedArbitraryIssuer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Issuer = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedArbitraryIssuer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllowedArbitraryIssuer_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllowedArbitraryIssuer_ClientId",
                table: "AllowedArbitraryIssuer",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_AllowedArbitraryIssuer_Issuer",
                table: "AllowedArbitraryIssuer",
                column: "Issuer",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllowedArbitraryIssuer");

            migrationBuilder.AddColumn<string>(
                name: "AllowedArbitraryIssuer",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
