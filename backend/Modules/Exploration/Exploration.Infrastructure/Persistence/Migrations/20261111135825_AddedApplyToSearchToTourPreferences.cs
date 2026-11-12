using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exploration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedApplyToSearchToTourPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ApplyToSearch",
                schema: "exploration",
                table: "TourPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplyToSearch",
                schema: "exploration",
                table: "TourPreferences");
        }
    }
}
