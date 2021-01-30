using Microsoft.EntityFrameworkCore.Migrations;

namespace SqlServer.Migrations.Tenant
{
    public partial class Duende501 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "SelfHelpUser",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequireResourceIndicator",
                table: "ApiResources",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_SelfHelpUser_Email",
                table: "SelfHelpUser",
                column: "Email",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SelfHelpUser_Email",
                table: "SelfHelpUser");

            migrationBuilder.DropColumn(
                name: "RequireResourceIndicator",
                table: "ApiResources");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "SelfHelpUser",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
