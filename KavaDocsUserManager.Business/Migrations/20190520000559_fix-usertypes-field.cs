using Microsoft.EntityFrameworkCore.Migrations;

namespace KavaDocsUserManager.Business.Migrations
{
    public partial class fixusertypesfield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserTypes",
                table: "UserRepositories");

            migrationBuilder.AddColumn<int>(
                name: "UserType",
                table: "UserRepositories",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserType",
                table: "UserRepositories");

            migrationBuilder.AddColumn<int>(
                name: "UserTypes",
                table: "UserRepositories",
                nullable: false,
                defaultValue: 0);
        }
    }
}
