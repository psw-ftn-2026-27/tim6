using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFollowRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    RespondedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FollowRelationshipId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FollowRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FollowRequests_FollowRelationships_FollowRelationshipId",
                        column: x => x.FollowRelationshipId,
                        principalSchema: "social",
                        principalTable: "FollowRelationships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FollowRelationships_FirstUserId_SecondUserId",
                schema: "social",
                table: "FollowRelationships",
                columns: new[] { "FirstUserId", "SecondUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FollowRequests_FollowRelationshipId",
                schema: "social",
                table: "FollowRequests",
                column: "FollowRelationshipId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FollowRequests",
                schema: "social");

            migrationBuilder.DropTable(
                name: "FollowRelationships",
                schema: "social");
        }
    }
}
