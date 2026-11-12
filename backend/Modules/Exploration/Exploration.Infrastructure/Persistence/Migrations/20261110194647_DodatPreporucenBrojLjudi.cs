using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exploration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DodatPreporucenBrojLjudi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecommendedGroupSize_Max",
                schema: "exploration",
                table: "Tours",
                type: "integer",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.AddColumn<int>(
                name: "RecommendedGroupSize_Min",
                schema: "exploration",
                table: "Tours",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecommendedGroupSize_Max",
                schema: "exploration",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "RecommendedGroupSize_Min",
                schema: "exploration",
                table: "Tours");
        }
    }
}
