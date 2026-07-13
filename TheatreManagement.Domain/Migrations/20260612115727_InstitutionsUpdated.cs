using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheatreManagement.Domain.Migrations
{
    /// <inheritdoc />
    public partial class InstitutionsUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Institutions",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Institutions");
        }
    }
}
