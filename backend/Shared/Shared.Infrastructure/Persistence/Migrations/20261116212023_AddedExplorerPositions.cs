using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddedExplorerPositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExplorerPositions",
                schema: "shared",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExplorerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Coordinates_Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Coordinates_Longitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExplorerPositions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExplorerPositions_ExplorerId",
                schema: "shared",
                table: "ExplorerPositions",
                column: "ExplorerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExplorerPositions",
                schema: "shared");
        }
    }
}
