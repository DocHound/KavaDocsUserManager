using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KavaDocsUserManager.Business.Migrations
{
    public partial class updatestructure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserType",
                table: "UserRepositories");

            migrationBuilder.AddColumn<int>(
                name: "UserTypes",
                table: "UserRepositories",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "RepositoryId",
                table: "Roles",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserTypes",
                table: "UserRepositories");

            migrationBuilder.DropColumn(
                name: "RepositoryId",
                table: "Roles");

            migrationBuilder.AddColumn<int>(
                name: "UserType",
                table: "UserRepositories",
                nullable: false,
                defaultValue: 0);
        }
    }
}
