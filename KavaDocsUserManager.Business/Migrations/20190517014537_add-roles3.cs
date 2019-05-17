using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KavaDocsUserManager.Business.Migrations
{
    public partial class addroles3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RepositoryId",
                table: "UserRoles",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RepositoryId",
                table: "UserRoles",
                column: "RepositoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRoles_Repositories_RepositoryId",
                table: "UserRoles",
                column: "RepositoryId",
                principalTable: "Repositories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserRoles_Repositories_RepositoryId",
                table: "UserRoles");

            migrationBuilder.DropIndex(
                name: "IX_UserRoles_RepositoryId",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "RepositoryId",
                table: "UserRoles");
        }
    }
}
