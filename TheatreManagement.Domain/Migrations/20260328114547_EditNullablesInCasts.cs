using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheatreManagement.Domain.Migrations
{
    /// <inheritdoc />
    public partial class EditNullablesInCasts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeRoles_Castes_CastId",
                table: "EmployeeRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeRoles_Events_EventId",
                table: "EmployeeRoles");

            migrationBuilder.AlterColumn<int>(
                name: "EventId",
                table: "EmployeeRoles",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "CastId",
                table: "EmployeeRoles",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeRoles_Castes_CastId",
                table: "EmployeeRoles",
                column: "CastId",
                principalTable: "Castes",
                principalColumn: "CastId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeRoles_Events_EventId",
                table: "EmployeeRoles",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeRoles_Castes_CastId",
                table: "EmployeeRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeRoles_Events_EventId",
                table: "EmployeeRoles");

            migrationBuilder.AlterColumn<int>(
                name: "EventId",
                table: "EmployeeRoles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CastId",
                table: "EmployeeRoles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeRoles_Castes_CastId",
                table: "EmployeeRoles",
                column: "CastId",
                principalTable: "Castes",
                principalColumn: "CastId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeRoles_Events_EventId",
                table: "EmployeeRoles",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
