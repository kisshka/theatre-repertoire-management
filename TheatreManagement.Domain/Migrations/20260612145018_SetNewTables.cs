using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheatreManagement.Domain.Migrations
{
    /// <inheritdoc />
    public partial class SetNewTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Institutions_InstitutionType_InstitutionTypeId",
                table: "Institutions");

            migrationBuilder.DropForeignKey(
                name: "FK_Plays_SceneType_SceneTypeId",
                table: "Plays");

            migrationBuilder.DropForeignKey(
                name: "FK_Stationars_HallType_HallTypeId",
                table: "Stationars");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SceneType",
                table: "SceneType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InstitutionType",
                table: "InstitutionType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HallType",
                table: "HallType");

            migrationBuilder.RenameTable(
                name: "SceneType",
                newName: "SceneTypes");

            migrationBuilder.RenameTable(
                name: "InstitutionType",
                newName: "InstitutionTypes");

            migrationBuilder.RenameTable(
                name: "HallType",
                newName: "HallTypes");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SceneTypes",
                table: "SceneTypes",
                column: "SceneTypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InstitutionTypes",
                table: "InstitutionTypes",
                column: "InstitutionTypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HallTypes",
                table: "HallTypes",
                column: "HallTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Institutions_InstitutionTypes_InstitutionTypeId",
                table: "Institutions",
                column: "InstitutionTypeId",
                principalTable: "InstitutionTypes",
                principalColumn: "InstitutionTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Plays_SceneTypes_SceneTypeId",
                table: "Plays",
                column: "SceneTypeId",
                principalTable: "SceneTypes",
                principalColumn: "SceneTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Stationars_HallTypes_HallTypeId",
                table: "Stationars",
                column: "HallTypeId",
                principalTable: "HallTypes",
                principalColumn: "HallTypeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Institutions_InstitutionTypes_InstitutionTypeId",
                table: "Institutions");

            migrationBuilder.DropForeignKey(
                name: "FK_Plays_SceneTypes_SceneTypeId",
                table: "Plays");

            migrationBuilder.DropForeignKey(
                name: "FK_Stationars_HallTypes_HallTypeId",
                table: "Stationars");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SceneTypes",
                table: "SceneTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InstitutionTypes",
                table: "InstitutionTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HallTypes",
                table: "HallTypes");

            migrationBuilder.RenameTable(
                name: "SceneTypes",
                newName: "SceneType");

            migrationBuilder.RenameTable(
                name: "InstitutionTypes",
                newName: "InstitutionType");

            migrationBuilder.RenameTable(
                name: "HallTypes",
                newName: "HallType");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SceneType",
                table: "SceneType",
                column: "SceneTypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InstitutionType",
                table: "InstitutionType",
                column: "InstitutionTypeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HallType",
                table: "HallType",
                column: "HallTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Institutions_InstitutionType_InstitutionTypeId",
                table: "Institutions",
                column: "InstitutionTypeId",
                principalTable: "InstitutionType",
                principalColumn: "InstitutionTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Plays_SceneType_SceneTypeId",
                table: "Plays",
                column: "SceneTypeId",
                principalTable: "SceneType",
                principalColumn: "SceneTypeId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Stationars_HallType_HallTypeId",
                table: "Stationars",
                column: "HallTypeId",
                principalTable: "HallType",
                principalColumn: "HallTypeId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
