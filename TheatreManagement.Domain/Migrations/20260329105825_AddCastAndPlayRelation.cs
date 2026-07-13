using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheatreManagement.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddCastAndPlayRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeRoles_AspNetUsers_UserId",
                table: "EmployeeRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_RoleInPlays_AspNetUsers_UserId",
                table: "RoleInPlays");

            migrationBuilder.DropIndex(
                name: "IX_RoleInPlays_UserId",
                table: "RoleInPlays");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeRoles_UserId",
                table: "EmployeeRoles");

            migrationBuilder.DropColumn(
                name: "LastEditTime",
                table: "RoleInPlays");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RoleInPlays");

            migrationBuilder.DropColumn(
                name: "LastEditTime",
                table: "EmployeeRoles");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "EmployeeRoles");

            migrationBuilder.AddColumn<int>(
                name: "PlayId",
                table: "Castes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Castes_PlayId",
                table: "Castes",
                column: "PlayId");

            migrationBuilder.AddForeignKey(
                name: "FK_Castes_Plays_PlayId",
                table: "Castes",
                column: "PlayId",
                principalTable: "Plays",
                principalColumn: "PlayId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Castes_Plays_PlayId",
                table: "Castes");

            migrationBuilder.DropIndex(
                name: "IX_Castes_PlayId",
                table: "Castes");

            migrationBuilder.DropColumn(
                name: "PlayId",
                table: "Castes");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEditTime",
                table: "RoleInPlays",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "RoleInPlays",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEditTime",
                table: "EmployeeRoles",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "EmployeeRoles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleInPlays_UserId",
                table: "RoleInPlays",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeRoles_UserId",
                table: "EmployeeRoles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeRoles_AspNetUsers_UserId",
                table: "EmployeeRoles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RoleInPlays_AspNetUsers_UserId",
                table: "RoleInPlays",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
