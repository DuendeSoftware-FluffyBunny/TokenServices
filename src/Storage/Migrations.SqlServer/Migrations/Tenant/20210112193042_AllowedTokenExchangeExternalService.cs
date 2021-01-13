using Microsoft.EntityFrameworkCore.Migrations;

namespace SqlServer.Migrations.Tenant
{
    public partial class AllowedTokenExchangeExternalService : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllowedTokenExchangeExternalService",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalService = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedTokenExchangeExternalService", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllowedTokenExchangeExternalService_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllowedTokenExchangeExternalService_ClientId",
                table: "AllowedTokenExchangeExternalService",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_AllowedTokenExchangeExternalService_ExternalService",
                table: "AllowedTokenExchangeExternalService",
                column: "ExternalService",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllowedTokenExchangeExternalService");
        }
    }
}
