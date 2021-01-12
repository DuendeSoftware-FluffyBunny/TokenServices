using Microsoft.EntityFrameworkCore.Migrations;

namespace SqlServer.Migrations.Tenant
{
    public partial class AllowedRevokeTokenTypeHint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Namespace",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AllowedRevokeTokenTypeHint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TokenTypeHint = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedRevokeTokenTypeHint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllowedRevokeTokenTypeHint_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllowedRevokeTokenTypeHint_ClientId",
                table: "AllowedRevokeTokenTypeHint",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_AllowedRevokeTokenTypeHint_TokenTypeHint",
                table: "AllowedRevokeTokenTypeHint",
                column: "TokenTypeHint",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllowedRevokeTokenTypeHint");

            migrationBuilder.DropColumn(
                name: "Namespace",
                table: "Clients");
        }
    }
}
