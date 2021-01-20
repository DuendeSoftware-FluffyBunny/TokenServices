using Microsoft.EntityFrameworkCore.Migrations;

namespace SqlServer.Migrations.Tenant
{
    public partial class SelfHelpUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SelfHelpUser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalUserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfHelpUser", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AllowedSelfHelpClient",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SelfHelpUserId = table.Column<int>(type: "int", nullable: false),
                    ClientId = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedSelfHelpClient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllowedSelfHelpClient_SelfHelpUser_SelfHelpUserId",
                        column: x => x.SelfHelpUserId,
                        principalTable: "SelfHelpUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllowedSelfHelpClient_ClientId",
                table: "AllowedSelfHelpClient",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AllowedSelfHelpClient_SelfHelpUserId",
                table: "AllowedSelfHelpClient",
                column: "SelfHelpUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SelfHelpUser_ExternalUserId",
                table: "SelfHelpUser",
                column: "ExternalUserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllowedSelfHelpClient");

            migrationBuilder.DropTable(
                name: "SelfHelpUser");
        }
    }
}
