using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DodatoResavanjeDrustvenogIzazova : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SocialChallengeRuns",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SocialChallengeId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialChallengeRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialChallengeRuns_SocialChallenges_SocialChallengeId",
                        column: x => x.SocialChallengeId,
                        principalSchema: "games",
                        principalTable: "SocialChallenges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RunParticipants",
                schema: "games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TouristId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastCheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SocialChallengeRunId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RunParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RunParticipants_SocialChallengeRuns_SocialChallengeRunId",
                        column: x => x.SocialChallengeRunId,
                        principalSchema: "games",
                        principalTable: "SocialChallengeRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RunParticipants_SocialChallengeRunId",
                schema: "games",
                table: "RunParticipants",
                column: "SocialChallengeRunId");

            migrationBuilder.CreateIndex(
                name: "IX_SocialChallengeRuns_SocialChallengeId",
                schema: "games",
                table: "SocialChallengeRuns",
                column: "SocialChallengeId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RunParticipants",
                schema: "games");

            migrationBuilder.DropTable(
                name: "SocialChallengeRuns",
                schema: "games");
        }
    }
}
