using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DodatPokusajLokacijskogIzazova : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocationChallengeAttempts",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationChallengeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExplorerId = table.Column<Guid>(type: "uuid", nullable: false),
                    RadiusInMeters = table.Column<double>(type: "double precision", nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EarnedXp = table.Column<int>(type: "integer", nullable: true),
                    RevealedHiddenPointIds = table.Column<Guid[]>(type: "uuid[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationChallengeAttempts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationChallengeAttempts_LocationChallengeId_ExplorerId",
                schema: "games",
                table: "LocationChallengeAttempts",
                columns: new[] { "LocationChallengeId", "ExplorerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationChallengeAttempts",
                schema: "games");
        }
    }
}
