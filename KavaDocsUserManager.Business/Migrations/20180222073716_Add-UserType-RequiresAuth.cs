using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace KavaDocsUserManager.Business.Migrations
{
    public partial class AddUserTypeRequiresAuth : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserType",
                table: "UserRepositories",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresAuth",
                table: "Repositories",
                nullable: false,
                defaultValue: false);

            var sql = @"update UserRepositories set UserType = 4 where IsOwner = 1;
update UserRepositories set UserType = 2 where IsOwner = 0;
";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserType",
                table: "UserRepositories");

            migrationBuilder.DropColumn(
                name: "RequiresAuth",
                table: "Repositories");
        }
    }
}
