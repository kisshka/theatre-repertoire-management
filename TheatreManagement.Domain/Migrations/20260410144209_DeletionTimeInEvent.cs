using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheatreManagement.Domain.Migrations
{
    /// <inheritdoc />
    public partial class DeletionTimeInEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "Events",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "Events");
        }
    }
}
