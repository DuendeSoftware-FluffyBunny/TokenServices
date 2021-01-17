using Microsoft.EntityFrameworkCore.Migrations;

namespace SqlServer.Migrations.Tenant
{
    public partial class AllowedTokenExchangeSubjectTokenType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AllowedTokenExchangeSubjectTokenType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectTokenType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ClientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllowedTokenExchangeSubjectTokenType", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllowedTokenExchangeSubjectTokenType_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllowedTokenExchangeSubjectTokenType_ClientId",
                table: "AllowedTokenExchangeSubjectTokenType",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_AllowedTokenExchangeSubjectTokenType_SubjectTokenType",
                table: "AllowedTokenExchangeSubjectTokenType",
                column: "SubjectTokenType",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllowedTokenExchangeSubjectTokenType");
        }
    }
}
