using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheatreManagement.Domain.Migrations
{
    /// <inheritdoc />
    public partial class FixEventError : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Plays_PlayId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayEvent_Events_EventId",
                table: "PlayEvent");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayEvent_Plays_PlayId",
                table: "PlayEvent");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlayEvent",
                table: "PlayEvent");

            migrationBuilder.RenameTable(
                name: "PlayEvent",
                newName: "PlayEvents");

            migrationBuilder.RenameIndex(
                name: "IX_PlayEvent_PlayId",
                table: "PlayEvents",
                newName: "IX_PlayEvents_PlayId");

            migrationBuilder.RenameIndex(
                name: "IX_PlayEvent_EventId",
                table: "PlayEvents",
                newName: "IX_PlayEvents_EventId");

            migrationBuilder.AlterColumn<int>(
                name: "PlayId",
                table: "Events",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlayEvents",
                table: "PlayEvents",
                column: "PlayEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Plays_PlayId",
                table: "Events",
                column: "PlayId",
                principalTable: "Plays",
                principalColumn: "PlayId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayEvents_Events_EventId",
                table: "PlayEvents",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayEvents_Plays_PlayId",
                table: "PlayEvents",
                column: "PlayId",
                principalTable: "Plays",
                principalColumn: "PlayId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Plays_PlayId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayEvents_Events_EventId",
                table: "PlayEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayEvents_Plays_PlayId",
                table: "PlayEvents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlayEvents",
                table: "PlayEvents");

            migrationBuilder.RenameTable(
                name: "PlayEvents",
                newName: "PlayEvent");

            migrationBuilder.RenameIndex(
                name: "IX_PlayEvents_PlayId",
                table: "PlayEvent",
                newName: "IX_PlayEvent_PlayId");

            migrationBuilder.RenameIndex(
                name: "IX_PlayEvents_EventId",
                table: "PlayEvent",
                newName: "IX_PlayEvent_EventId");

            migrationBuilder.AlterColumn<int>(
                name: "PlayId",
                table: "Events",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlayEvent",
                table: "PlayEvent",
                column: "PlayEventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Plays_PlayId",
                table: "Events",
                column: "PlayId",
                principalTable: "Plays",
                principalColumn: "PlayId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayEvent_Events_EventId",
                table: "PlayEvent",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayEvent_Plays_PlayId",
                table: "PlayEvent",
                column: "PlayId",
                principalTable: "Plays",
                principalColumn: "PlayId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
