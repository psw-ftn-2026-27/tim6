using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Splits the FollowRelationships aggregate into directed Follows and standalone FollowRequests.
    /// Existing follows and requests are carried over.
    /// <para>
    /// The old schema had no database constraint on pending requests, so two requests racing past the
    /// aggregate's check could both be stored as pending for the same sender and recipient. The new
    /// partial unique index would reject such data and abort the migration. Before the index is built,
    /// Up therefore keeps the oldest pending request of each sender and recipient and deletes the later
    /// ones. They carry no information the kept request lacks, and deleting a pending request is exactly
    /// what cancelling one does. Answered requests are never touched.
    /// </para>
    /// </summary>
    public partial class SplitFollowRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE FROM social.""FollowRequests"" AS redundant
                USING (
                    SELECT ""Id"",
                           ROW_NUMBER() OVER (
                               PARTITION BY ""SenderId"", ""RecipientId""
                               ORDER BY ""CreatedAt"", ""Id"") AS position
                    FROM social.""FollowRequests""
                    WHERE ""Status"" = 'Pending'
                ) AS ranked
                WHERE redundant.""Id"" = ranked.""Id"" AND ranked.position > 1;");

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

            migrationBuilder.Sql(@"
                INSERT INTO social.""Follows"" (""Id"", ""FollowerId"", ""FollowedUserId"")
                SELECT gen_random_uuid(), ""FirstUserId"", ""SecondUserId""
                FROM social.""FollowRelationships""
                WHERE ""FirstFollowsSecond""
                UNION ALL
                SELECT gen_random_uuid(), ""SecondUserId"", ""FirstUserId""
                FROM social.""FollowRelationships""
                WHERE ""SecondFollowsFirst"";");

            migrationBuilder.DropForeignKey(
                name: "FK_FollowRequests_FollowRelationships_FollowRelationshipId",
                schema: "social",
                table: "FollowRequests");

            migrationBuilder.DropTable(
                name: "FollowRelationships",
                schema: "social");

            migrationBuilder.DropIndex(
                name: "IX_FollowRequests_FollowRelationshipId",
                schema: "social",
                table: "FollowRequests");

            migrationBuilder.DropColumn(
                name: "FollowRelationshipId",
                schema: "social",
                table: "FollowRequests");

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
        }

        /// <inheritdoc />
        /// <remarks>
        /// DESTRUCTIVE. Restores the previous schema only and discards everything the new model holds:
        /// every active follow and every follow request, pending or answered. Nothing is copied back into
        /// FollowRelationships, so after a Down users have to follow each other again and the request
        /// history is gone. The old model kept one row per pair, keyed by Guid.CompareTo ordering, and
        /// hung every request off that row; that ordering cannot be reproduced reliably in SQL.
        /// </remarks>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM social.""FollowRequests"";");

            migrationBuilder.DropIndex(
                name: "IX_FollowRequests_RecipientId",
                schema: "social",
                table: "FollowRequests");

            migrationBuilder.DropIndex(
                name: "IX_FollowRequests_SenderId",
                schema: "social",
                table: "FollowRequests");

            migrationBuilder.DropIndex(
                name: "IX_FollowRequests_SenderId_RecipientId_Pending",
                schema: "social",
                table: "FollowRequests");

            migrationBuilder.DropTable(
                name: "Follows",
                schema: "social");

            migrationBuilder.AddColumn<Guid>(
                name: "FollowRelationshipId",
                schema: "social",
                table: "FollowRequests",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "FollowRelationships",
                schema: "social",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecondUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstFollowsSecond = table.Column<bool>(type: "boolean", nullable: false),
                    SecondFollowsFirst = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowRelationships", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FollowRequests_FollowRelationshipId",
                schema: "social",
                table: "FollowRequests",
                column: "FollowRelationshipId");

            migrationBuilder.CreateIndex(
                name: "IX_FollowRelationships_FirstUserId_SecondUserId",
                schema: "social",
                table: "FollowRelationships",
                columns: new[] { "FirstUserId", "SecondUserId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FollowRequests_FollowRelationships_FollowRelationshipId",
                schema: "social",
                table: "FollowRequests",
                column: "FollowRelationshipId",
                principalSchema: "social",
                principalTable: "FollowRelationships",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
