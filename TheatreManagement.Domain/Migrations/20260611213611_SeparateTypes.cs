using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheatreManagement.Domain.Migrations
{
    /// <inheritdoc />
    public partial class SeparateTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hall",
                table: "Stationars");

            migrationBuilder.DropColumn(
                name: "IsCanceled",
                table: "Events");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Stationars",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HallTypeId",
                table: "Stationars",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SceneTypeId",
                table: "Plays",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InstitutionTypeId",
                table: "Institutions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Institutions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "Events",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HallType",
                columns: table => new
                {
                    HallTypeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HallType", x => x.HallTypeId);
                });

            migrationBuilder.CreateTable(
                name: "InstitutionType",
                columns: table => new
                {
                    InstitutionTypeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstitutionType", x => x.InstitutionTypeId);
                });

            migrationBuilder.CreateTable(
                name: "SceneType",
                columns: table => new
                {
                    SceneTypeId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SceneType", x => x.SceneTypeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stationars_HallTypeId",
                table: "Stationars",
                column: "HallTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Plays_SceneTypeId",
                table: "Plays",
                column: "SceneTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Institutions_InstitutionTypeId",
                table: "Institutions",
                column: "InstitutionTypeId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropTable(
                name: "HallType");

            migrationBuilder.DropTable(
                name: "InstitutionType");

            migrationBuilder.DropTable(
                name: "SceneType");

            migrationBuilder.DropIndex(
                name: "IX_Stationars_HallTypeId",
                table: "Stationars");

            migrationBuilder.DropIndex(
                name: "IX_Plays_SceneTypeId",
                table: "Plays");

            migrationBuilder.DropIndex(
                name: "IX_Institutions_InstitutionTypeId",
                table: "Institutions");

            migrationBuilder.DropColumn(
                name: "HallTypeId",
                table: "Stationars");

            migrationBuilder.DropColumn(
                name: "SceneTypeId",
                table: "Plays");

            migrationBuilder.DropColumn(
                name: "InstitutionTypeId",
                table: "Institutions");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Institutions");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "Events");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Stationars",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "Hall",
                table: "Stationars",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCanceled",
                table: "Events",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }
    }
}
