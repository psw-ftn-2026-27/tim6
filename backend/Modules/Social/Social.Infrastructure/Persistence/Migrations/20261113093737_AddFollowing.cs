using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFollowing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FollowRequests",
                schema: "social",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Follows",
                schema: "social",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FollowerId = table.Column<Guid>(type: "uuid", nullable: false),
                    FollowedUserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Follows", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FollowRequests_RecipientId",
                schema: "social",
                table: "FollowRequests",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowRequests_SenderId",
                schema: "social",
                table: "FollowRequests",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowRequests_SenderId_RecipientId_Pending",
                schema: "social",
                table: "FollowRequests",
                columns: new[] { "SenderId", "RecipientId" },
                unique: true,
                filter: "\"Status\" = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "IX_Follows_FollowedUserId",
                schema: "social",
                table: "Follows",
                column: "FollowedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Follows_FollowerId_FollowedUserId",
                schema: "social",
                table: "Follows",
                columns: new[] { "FollowerId", "FollowedUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FollowRequests",
                schema: "social");

            migrationBuilder.DropTable(
                name: "Follows",
                schema: "social");
        }
    }
}
