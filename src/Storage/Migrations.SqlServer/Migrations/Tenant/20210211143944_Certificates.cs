using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SqlServer.Migrations.Tenant
{
    public partial class Certificates : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Certificate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SigningAlgorithm = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    PFXBase64 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NotBefore = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Expiration = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JWK = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certificate", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Certificate");
        }
    }
}
