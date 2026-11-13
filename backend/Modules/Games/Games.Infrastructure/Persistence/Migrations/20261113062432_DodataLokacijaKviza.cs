using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DodataLokacijaKviza : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Location_Latitude",
                schema: "games",
                table: "Quizzes",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Location_Longitude",
                schema: "games",
                table: "Quizzes",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location_Latitude",
                schema: "games",
                table: "Quizzes");

            migrationBuilder.DropColumn(
                name: "Location_Longitude",
                schema: "games",
                table: "Quizzes");
        }
    }
}
