using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Games.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VremenaIzvedbeUUtc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                schema: "games",
                table: "SocialChallengeRuns",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastCheckedAt",
                schema: "games",
                table: "RunParticipants",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "StartedAt",
                schema: "games",
                table: "SocialChallengeRuns",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastCheckedAt",
                schema: "games",
                table: "RunParticipants",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }
    }
}
